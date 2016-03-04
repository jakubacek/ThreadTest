using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ClientSync
{
    public class Client
    {
        private Socket _sender;
        // Data buffer for incoming data.
        byte[] bytes = new byte[1024];

        public bool StartClient(object argument)
        {


            // Connect to a remote device.
            try
            {
                StartClient();
                Connect();
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;
        }

        public bool SendMessage(string message, out string response)
        {
            try
            {
                SendData(message);
                response = ReceiveData();
                return true;
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane);
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e);
            }
            response = string.Empty;
            return false;
        }

        private void StartClient()
        {

            // Create a TCP/IP  socket.
            _sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private void Connect()
        {
            // Establish the remote endpoint for the socket.
            // This example uses port 11000 on the local computer.
            var ipAddress = IpAddress();
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);
            _sender.Connect(remoteEP);
            Console.WriteLine("Socket connected to {0}", _sender.RemoteEndPoint);
        }

        private void SendData(string dataToSend)
        {
            // Encode the data string into a byte array.
            byte[] msg = Encoding.ASCII.GetBytes(dataToSend);

            // Send the data through the socket.
            int bytesSent = _sender.Send(msg);
            Console.WriteLine("Client sent data: {0}", dataToSend);

        }

        private string ReceiveData()
        {
            int bytesRec = _sender.Receive(bytes);
            return Encoding.ASCII.GetString(bytes, 0, bytesRec);
        }

        public void Close()
        {
            // Release the socket.
            _sender.Shutdown(SocketShutdown.Both);
            _sender.Close();
        }

        private static IPAddress IpAddress()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            return ipHostInfo.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }
    }
}
