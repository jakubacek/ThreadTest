using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace Client
{
    public class AsyncClient : IClient
    {
        private readonly List<string> _communicationLog; 
        private readonly Socket _client;

        private readonly int _timeout = 5000;
        private readonly ManualResetEvent _connectDone = new ManualResetEvent(false);
        private readonly ManualResetEvent _sendDone = new ManualResetEvent(false);
        private readonly ManualResetEvent _receiveDone = new ManualResetEvent(false);

        public int ServerPort { get; private set; }
        public IPAddress ServerIpAddress { get; private set; }

        public AsyncClient(IPAddress serverAddress, int port)
        {
            ServerPort = port;
            ServerIpAddress = serverAddress;
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _communicationLog = new List<string>();
        }

        public bool Connect()
        {
            // Connect to the remote endpoint.
            try
            {
                _client.BeginConnect(new IPEndPoint(ServerIpAddress, ServerPort), ConnectCallback, null);
                _connectDone.WaitOne(_timeout);
                return true;
            }
            catch (Exception exp)
            {
                Trace.WriteLine("Error while connecting to server.");
                Trace.WriteLine(exp);
                Close();                
            }
            return false;

        }

        public bool Send(string message, out string response)
        {
            response = string.Empty;
            try
            {
                if (_client != null && _client.Connected)
                {
                    byte[] byteData = Encoding.ASCII.GetBytes(message);

                    // Begin sending the data to the remote device.
                    _client.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, message);
                    _sendDone.WaitOne(_timeout);

                    StateObject state = new StateObject();
                    _client.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, ReceiveCallback, state);
                    _receiveDone.WaitOne(_timeout);

                    response = state.CommandBuilder.ToString();
                    _communicationLog.Add(response);
                    _sendDone.Reset();
                    _receiveDone.Reset();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error in communication with server.");
                Trace.WriteLine(ex);
                Close();
            }
            return false;
        }

        public void Close()
        {
            try
            {
                // Release the socket.
                _client.Shutdown(SocketShutdown.Both);
                _client.Close();

                _connectDone.Close();
                _receiveDone.Close();
                _sendDone.Close();
            }
            catch
            {
                // ignored
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Complete sending the data to the remote device.
                int bytesSent = _client.EndSend(ar);
                Trace.WriteLine(String.Format("Sent to server: {0}", ar.AsyncState));
                Trace.WriteLine(String.Format("Sent {0} bytes to server.", bytesSent));

                _sendDone.Set(); // Signal that all bytes have been sent.
            }
            catch (Exception e)
            {
                Trace.WriteLine("Error while sending data:");
                Trace.WriteLine(e);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                int bytesRead = _client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    state.CommandBuilder.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

                    if (_client.Available > 0)
                    {
                        _client.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, ReceiveCallback, state);
                        return;
                    }
                }
                Trace.WriteLine(String.Format("Received response {0}", state.CommandBuilder));
                _receiveDone.Set();
            }
            catch (Exception e)
            {
                Trace.WriteLine("Error while receiving data:");
                Trace.WriteLine(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                _client.EndConnect(ar);

                Trace.WriteLine(string.Format("Socket connected to {0}", _client.RemoteEndPoint));
                _connectDone.Set();
            }
            catch (Exception e)
            {
                Trace.WriteLine("Error while connecting:");
                Trace.WriteLine(e.ToString());
            }
        }

        public bool IsConnected => _client != null && _client.Connected;

        public IEnumerable<string> CommunicationLog => _communicationLog;

        public void Dispose()
        {

            _client.Close();
            _client.Dispose();

            _connectDone.Close();
            _connectDone.Dispose();

            _receiveDone.Close();
            _receiveDone.Dispose();

            _sendDone.Close();
            _sendDone.Dispose();

        }
    }
}
