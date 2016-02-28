using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ClientAsync
{
    public class Client : IDisposable
    {
        private readonly Socket _client;
        public int ServerPort { get; private set; }
        public IPAddress ServerIpAddress;

        private ManualResetEvent _connectDone = new ManualResetEvent(false);
        private ManualResetEvent _sendDone = new ManualResetEvent(false);
        private ManualResetEvent _receiveDone = new ManualResetEvent(false);

        public Client(IPAddress serverAddress, int port)
        {
            ServerPort = port;
            ServerIpAddress = serverAddress;
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect()
        {
            // Connect to the remote endpoint.
            _client.BeginConnect(new IPEndPoint(ServerIpAddress, ServerPort), new AsyncCallback(ConnectCallback), null);
            _connectDone.WaitOne();
        }

        public void SendMessageToServer(string message, out string response)
        {
            response = string.Empty;
            try
            {
                if (_client != null && _client.Connected)
                {
                    byte[] byteData = Encoding.ASCII.GetBytes(message);

                    // Begin sending the data to the remote device.
                    _client.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, null);
                    _sendDone.WaitOne();

                    StateObject state = new StateObject();
                    _client.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, ReceiveCallback, state);
                    _receiveDone.WaitOne();

                    response = state.CommandBuilder.ToString();
                    _sendDone.Reset();
                    _receiveDone.Reset();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

        }

        public void Close()
        {
            // Release the socket.
            _client.Shutdown(SocketShutdown.Both);
            _client.Close();

            _connectDone.Close();
            _receiveDone.Close();
            _sendDone.Close();
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                // Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = _client.EndSend(ar);
                Trace.WriteLine(String.Format("Sent {0} bytes to server.", bytesSent));

                _sendDone.Set(); // Signal that all bytes have been sent.
            }
            catch (Exception e)
            {
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
                _receiveDone.Set();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                //Socket client = (Socket)ar.AsyncState;                
                _client.EndConnect(ar);

                Trace.WriteLine(string.Format("Socket connected to {0}", _client.RemoteEndPoint));
                _connectDone.Set();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }
        }

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

    public class StateObject
    {
        public byte[] Buffer = new byte[1024];
        public StringBuilder CommandBuilder = new StringBuilder();
    }
}
