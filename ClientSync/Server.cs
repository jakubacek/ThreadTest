using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ClientSync
{
  public class Server
    {
        byte[] bytes = new byte[1024];
        public static string data = null;
        private Socket _listener;

        public Server(int port = 11000)
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private void StartListening()
        {
            IPEndPoint localEndPoint = new IPEndPoint(GetLocalIp(), 11000);
            _listener.Bind(localEndPoint);
            _listener.Listen(10);
        }

        private Socket WaitForConnection()
        {
            Console.WriteLine("Waiting for a connection...");
            // Program is suspended while waiting for an incoming connection.
            return _listener.Accept();
        }

        private string ReadData(Socket handler)
        {
            data = null;
            // An incoming connection needs to be processed.
            while (true)
            {
                Console.WriteLine("Server available: {0}", handler.Available);
                bytes = new byte[1024];
                int bytesRec = handler.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                if(handler.Available==0) //if (data.IndexOf("<EOF>") > -1)                
                {
                    break;
                }
            }

            return data;
        }

        private void SendData(Socket handler, string message)
        {
            // Echo the data back to the client.
            byte[] msg = Encoding.ASCII.GetBytes(message);
            handler.Send(msg);
        }

        private void CloseConnection(Socket handler)
        {
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }

        private static IPAddress GetLocalIp()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            return ipHostInfo.AddressList.First(ip=> ip.AddressFamily == AddressFamily.InterNetwork);
        }



        public void StartServer(object argument)
        {
            try
            {
                
                // Start listening for connections.
                StartListening();

                var handler = WaitForConnection();

                string incommingData;
                do
                {
                    incommingData = ReadData(handler);

                    // Show the data on the console.
                    Console.WriteLine("Text received : {0}", incommingData);

                    SendData(handler, incommingData.ToUpperInvariant());
                } while (!string.IsNullOrEmpty(incommingData));
                
                CloseConnection(handler);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }
    }
}
