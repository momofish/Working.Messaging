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

        public bool LogMsg { get; set; }
        public SocketState SocketState { get; private set; }

        public MessageClient(string loginId)
        {
            _logger = LogManager.GetLogger(this.GetType());

            LogMsg = true;
            SocketState = new SocketState { Loginid = loginId };
        }

        public void Connect()
        {
            try
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                SocketState.Socket = socket;
                socket.Connect("localhost", 2000);
                socket.BeginReceive(SocketState.Buffer, 0, SocketState.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), null);
                Send(new Message { Id = DateTime.Now.Ticks, MsgType = MsgType.Login, From = SocketState.Loginid });
            }
            catch (Exception ex)
            {
                _logger.Error("connecting failed", ex);
                Connect();
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var handler = SocketState.Socket;

            SocketError socketError;
            var bytesRead = handler.EndReceive(ar, out socketError);
            if (socketError != SocketError.Success)
            {
                _logger.ErrorFormat("connection error: {0}", socketError);
                Connect();
                return;
            }
            if (bytesRead > 0)
            {
                SocketState.Content.Write(SocketState.Buffer, 0, bytesRead);

                if (SocketState.Buffer[bytesRead - 1] != Message.EndTag[0])
                {
                    handler.BeginReceive(SocketState.Buffer, 0, SocketState.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), null);
                    return;
                }

                Exception exception = null;
                var message = CoreHelper.Try(() => Message.ParseData(SocketState.Content.ToArray())[0], out exception, _logger);
                SocketState.Content.SetLength(0);
                _logger.DebugFormat("receive from {0}: {1}", handler.RemoteEndPoint, message);

                CoreHelper.Try(() => HandleMessage(message), out exception, _logger);

                handler.BeginReceive(SocketState.Buffer, 0, SocketState.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), null);
            }
        }

        public void Send(Message message)
        {
            var handler = SocketState.Socket;
            var toSendData = message.Serialize();

            handler.Send(toSendData, 0, toSendData.Length, 0);
            _logger.DebugFormat("sent to {0}: {1}", handler.RemoteEndPoint, message);
        }

        private void HandleMessage(Message message)
        {
            if (LogMsg)
                _logger.InfoFormat("received from {0}: {1}", message.From, message.Content);
        }
    }
}
