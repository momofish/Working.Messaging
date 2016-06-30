using Common.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Working.Messaging.Utils;

namespace Working.Messaging.Server
{
    public class MessageServer
    {
        private readonly ILog _logger;

        private readonly Socket _listener = null;
        private readonly Dictionary<string, SocketState> _states = new Dictionary<string, SocketState>();

        public MessageServer()
        {
            _logger = LogManager.GetLogger(this.GetType());
        }

        public void Start()
        {
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Any, 2000));
            listener.Listen(0);
            _logger.InfoFormat("listening on {0}", listener.LocalEndPoint);

            Accept(listener);
            return;
        }

        public void Accept(Socket listener)
        {
            listener.BeginAccept(acceptResult =>
            {
                // accept new
                Accept(listener);

                var handler = listener.EndAccept(acceptResult);
                _logger.DebugFormat("accept from {0}", handler.RemoteEndPoint);

                var state = new SocketState();
                state.Socket = handler;
                handler.BeginReceive(state.Buffer, 0, SocketState.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), state);
            }, null);
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            var state = (SocketState)ar.AsyncState;
            var handler = state.Socket;

            SocketError socketError;
            var bytesRead = handler.EndReceive(ar, out socketError);
            if (socketError != SocketError.Success)
            {
                _logger.ErrorFormat("client [{0}]{1} error: {2}", state.Loginid, handler.RemoteEndPoint, socketError);
                handler.Close();
                _states.Remove(state.Loginid);
                return;
            }
            if (bytesRead > 0)
            {
                state.Content.Write(state.Buffer, 0, bytesRead);

                if (!state.Buffer.EndWith(Message.EndTag, bytesRead))
                {
                    handler.BeginReceive(state.Buffer, 0, SocketState.BUFFER_SIZE, 0, out socketError, new AsyncCallback(ReceiveCallback), state);
                    if (socketError != SocketError.Success)
                    {
                        _logger.ErrorFormat("client [{0}]{1} error: {2}", state.Loginid, handler.RemoteEndPoint, socketError);
                        handler.Close();
                        _states.Remove(state.Loginid);
                        return;
                    }
                    return;
                }

                Exception exception = null;
                var messages = CoreHelper.Try(() => Message.ParseData(state.Content.ToArray()), out exception, _logger);
                state.Content.SetLength(0);

                try
                {
                    foreach (var message in messages)
                    {
                        _logger.DebugFormat("receive from {0}: {1}", handler.RemoteEndPoint, message);
                        if (exception != null)
                            Send(state, new Message { MsgType = MsgType.Exception, Content = exception.Message });

                        CoreHelper.Try(() => DealMessage(state, message), out exception, _logger);
                        if (exception != null)
                            Send(state, new Message { MsgType = MsgType.Exception, Content = exception.Message });
                    }

                    handler.BeginReceive(state.Buffer, 0, SocketState.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), state);
                }
                catch (SocketException ex)
                {
                    _logger.ErrorFormat("client [{0}]{1} error: {2}", state.Loginid, handler.RemoteEndPoint, ex.Message);
                    handler.Close();
                    _states.Remove(state.Loginid);
                    return;
                }
            }
        }

        private void DealMessage(SocketState state, Message message)
        {
            message.Validate();

            if (message.MsgType == MsgType.Login)
            {
                lock (_states)
                {
                    var from = message.From;
                    state.Loginid = from;
                    if (!_states.ContainsKey(from) || _states[from] != state)
                        _states[from] = state;
                    _logger.InfoFormat("client [{0}]{1} logged in", state.Loginid, state.Socket.RemoteEndPoint);
                }
            }
            else if (message.MsgType == MsgType.Content)
            {
                message.From = state.Loginid;
                var to = message.To;
                if (_states.ContainsKey(to))
                {
                    var toState = _states[to];
                    Send(toState, message);
                }
            }
        }

        private void Send(SocketState state, Message message)
        {
            var handler = state.Socket;
            var toSendData = message.Serialize();

            handler.Send(toSendData, 0, toSendData.Length, 0);
            _logger.DebugFormat("sent to {0}: {1}", handler.RemoteEndPoint, message);
        }

        public void Stop()
        {
            if (_listener != null)
            {
                _listener.Shutdown(SocketShutdown.Both);
                _listener.Close();
            }
        }
    }
}
