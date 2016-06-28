using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Working.Messaging.Utils;

namespace Working.Messaging.ConsoleClient
{
    public class MessageClient
    {
        private readonly ILog _logger;

        private readonly BsonSerializer _serialize = new BsonSerializer();

        public SocketState SocketState { get; private set; }
        public string LoginId {get;private set;}

        public MessageClient(string loginId)
        {
            _logger = LogManager.GetLogger(this.GetType());

            LoginId = loginId;
            SocketState = new SocketState();
        }

        public void Connect()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketState.Socket = socket;
            socket.Connect("localhost", 2000);
            socket.BeginReceive(SocketState.Buffer, 0, SocketState.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), SocketState);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var state = (SocketState)ar.AsyncState;
            var handler = state.Socket;

            var bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                state.Content.Write(state.Buffer, 0, bytesRead - 1);

                if (state.Buffer[bytesRead - 1] != 26)
                {
                    handler.BeginReceive(state.Buffer, 0, SocketState.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), state);
                    return;
                }

                Exception exception = null;
                var message = CoreHelper.Try(() => _serialize.Deserialize<Message>(state.Content.ToArray()), out exception, _logger);
                state.Content.SetLength(0);
                _logger.DebugFormat("receive from {0}: {1}", handler.RemoteEndPoint, message);

                CoreHelper.Try(() => HandleMessage(message), out exception, _logger);

                handler.BeginReceive(state.Buffer, 0, SocketState.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), state);
            }
        }

        public void Send(Message message)
        {
            var handler = SocketState.Socket;
            var toSendData = _serialize.Serialize(message);
            handler.BeginSend(toSendData, 0, toSendData.Length, 0, sendAsync =>
            {
                handler.EndSend(sendAsync);
                _logger.DebugFormat("sent to {0}: {1}", handler.RemoteEndPoint, message);
            }, null);
        }

        private void HandleMessage(Message message)
        {
            _logger.InfoFormat("received: ", message);
        }
    }
}
