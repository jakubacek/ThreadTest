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
    public class ClientAwaitable : IDisposable
    {
        private readonly Socket _client;

        private readonly int _timeout = 5000;                
        public int ServerPort { get; private set; }
        public IPAddress ServerIpAddress;

        public ClientAwaitable(IPAddress serverAddress, int port)
        {
            ServerPort = port;
            ServerIpAddress = serverAddress;
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect()
        {
            // Connect to the remote endpoint.
            try
            {
               var result = _client.BeginConnect(new IPEndPoint(ServerIpAddress, ServerPort), ConnectCallback, null);
                result.AsyncWaitHandle.WaitOne();                
            }
            catch (Exception exp)
            {
                Trace.WriteLine("Error while connecting to server.");
                Trace.WriteLine(exp);
                Close();
                throw;
            }
            
        }

        public async Task<bool> SendMessageToServer(string message)
        {            
            try
            {
                if (_client != null && _client.Connected)
                {
                    byte[] byteData = Encoding.ASCII.GetBytes(message);

                    // Begin sending the data to the remote device.
                   var resultSend = _client.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, message);
                    resultSend.AsyncWaitHandle.WaitOne();                    

                    StateObject state = new StateObject();
                    var resultReceive = _client.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, ReceiveCallback, state);
                    resultReceive.AsyncWaitHandle.WaitOne(_timeout);
                    await Task.Delay(10);
                    Console.WriteLine(state.CommandBuilder.ToString());   
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
            }
            catch (Exception e)
            {             
                Trace.WriteLine("Error while connecting:");
                Trace.WriteLine(e.ToString());
            }
        }

        public void Dispose()
        {

            _client.Close();
            _client.Dispose();
        }
    }

   
}
