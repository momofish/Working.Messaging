﻿using Common.Logging;
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
        public string LoginId { get; private set; }

        public MessageClient(string loginId)
        {
            _logger = LogManager.GetLogger(this.GetType());

            LoginId = loginId;
            SocketState = new SocketState();
        }

        public void Connect(string loginId)
        {
            try
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                SocketState.Socket = socket;
                socket.Connect("localhost", 2000);
                socket.BeginReceive(SocketState.Buffer, 0, SocketState.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), null);
                Send(new Message { Id = DateTime.Now.Ticks, MsgType = MsgType.Login, From = loginId });
            }
            catch (Exception ex)
            {
                _logger.Error("connecting failed", ex);
                Connect(loginId);
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
                Connect(SocketState.Loginid);
                return;
            }
            if (bytesRead > 0)
            {
                SocketState.Content.Write(SocketState.Buffer, 0, bytesRead - 1);

                if (SocketState.Buffer[bytesRead - 1] != 26)
                {
                    handler.BeginReceive(SocketState.Buffer, 0, SocketState.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), null);
                    return;
                }

                Exception exception = null;
                var message = CoreHelper.Try(() => _serialize.Deserialize<Message>(SocketState.Content.ToArray()), out exception, _logger);
                SocketState.Content.SetLength(0);
                _logger.DebugFormat("receive from {0}: {1}", handler.RemoteEndPoint, message);

                CoreHelper.Try(() => HandleMessage(message), out exception, _logger);

                handler.BeginReceive(SocketState.Buffer, 0, SocketState.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), null);
            }
        }

        public void Send(Message message)
        {
            var handler = SocketState.Socket;
            var toSendData = _serialize.Serialize(message);
            Array.Resize(ref toSendData, toSendData.Length + 1);
            toSendData[toSendData.Length - 1] = 26;

            handler.BeginSend(toSendData, 0, toSendData.Length, 0, sendAsync =>
            {
                handler.EndSend(sendAsync);
                _logger.DebugFormat("sent to {0}: {1}", handler.RemoteEndPoint, message);
            }, null);
        }

        private void HandleMessage(Message message)
        {
            _logger.InfoFormat("received from {0}: {1}", message.From, message.Content);
        }
    }
}
