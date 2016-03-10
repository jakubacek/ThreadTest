using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    public class BlockClient : IClient
    {

        private readonly List<string> _communicationLog;
         
        private readonly Socket _client;
        // Data buffer for incoming data.
        readonly byte[] _bytes = new byte[1024];

        public int ServerPort { get; private set; }
        public IPAddress ServerIpAddress { get; private set; }

        public BlockClient(IPAddress serverAddress, int port)
        {
            ServerPort = port;
            ServerIpAddress = serverAddress;
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _communicationLog = new List<string>();
        }


        public bool Send(string message, out string response)
        {
            try
            {
                SendData(message);
                response = ReceiveData();
                _communicationLog.Add(response);
                return true;
            }
            catch (ArgumentNullException ane)
            {
                Trace.WriteLine(string.Format("ArgumentNullException : {0}", ane));
            }
            catch (SocketException se)
            {
                Trace.WriteLine(string.Format("SocketException : {0}", se));
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format("Unexpected exception : {0}", e));
            }
            response = string.Empty;
            return false;
        }

        public bool IsConnected => _client != null && _client.Connected;
        

        public bool Connect()
        {


            // Connect to a remote device.
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(ServerIpAddress, ServerPort);
                _client.Connect(remoteEP);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;

        }

        private void SendData(string dataToSend)
        {
            // Encode the data string into a byte array.
            byte[] msg = Encoding.ASCII.GetBytes(dataToSend);

            // Send the data through the socket.
            int bytesSent = _client.Send(msg);

        }

        private string ReceiveData()
        {
            int bytesRec = _client.Receive(_bytes);
            return Encoding.ASCII.GetString(_bytes, 0, bytesRec);
        }

        public void Close()
        {
            // Release the socket.
            _client.Shutdown(SocketShutdown.Both);
            _client.Close();
        }

        public IEnumerable<string> CommunicationLog => _communicationLog;

        public void Dispose()
        {
            _client.Close();
            _client.Dispose();
        }
    }    
}
