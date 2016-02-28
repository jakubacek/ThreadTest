using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;


namespace TestThread
{
    class Server : IDisposable
    {
        private readonly Socket _listenerSocket;
        public IPAddress IpAddress { get; private set; }
        public int Port { get; private set; }

        public Func<string, string> ProcessCommand = null;


        public Server(IPAddress address, int port)
        {
            Port = port;
            IpAddress = address;
            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Trace.WriteLine(string.Format("Async Server created. Address: {0}:{1}", IpAddress, port));
        }


        public void Start(object startState)
        {
            Trace.WriteLine(string.Format("Async Server start listening ...... "));
            _listenerSocket.Bind(new IPEndPoint(IpAddress, 11000));
            _listenerSocket.Listen(10);
            while (true)
            {
                _listenerSocket.BeginAccept(AcceptCallback, null);
                Thread.Sleep(100);
            }

        }

        private void AcceptCallback(IAsyncResult asyncResult)
        {

            Socket handlerSocket = _listenerSocket.EndAccept(asyncResult);
            StateObject state = new StateObject { CommunicationSocket = handlerSocket, ClientIpAddress = Helper.GetIpAddressString(handlerSocket.RemoteEndPoint as IPEndPoint) };
            try
            {
                Trace.WriteLine(string.Format("Async Server Client {0} connected.", state.ClientIpAddress));

                handlerSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveCallback, state);
            }
            catch (Exception exp)
            {
                Trace.WriteLine(exp);
                HandleClientDisconnected(state);
            }

        }

        private void ReceiveCallback(IAsyncResult asyncResult)
        {


            var state = (StateObject)asyncResult.AsyncState;
            var handlerSocket = state.CommunicationSocket;
            var commandBuilder = state.CommandBuilder;

            Trace.WriteLine(string.Format("Async Server receiving data from client {0}", state.ClientIpAddress));

            try
            {

                int bytesRead = handlerSocket.EndReceive(asyncResult);

                if (bytesRead > 0)
                {
                    commandBuilder.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));
                    state.ReadCounter = 0;

                    if (handlerSocket.Available > 0)
                    {
                        handlerSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveCallback, state);
                        return;
                    }
                }

                var command = commandBuilder.ToString();
                if (string.IsNullOrEmpty(command))
                {
                    if (state.ReadCounter < 10)
                    {
                        state.ReadCounter++;
                        Thread.Sleep(1000);
                        handlerSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveCallback, state);
                        return;
                    }
                    else
                    {
                        Trace.WriteLine(string.Format("Async Server client is not responding. Closing connection with client {0}", state.ClientIpAddress));
                        handlerSocket.Close();
                        return;
                    }
                }

                Trace.WriteLine(string.Format("Async Server received data from client {0}", state.ClientIpAddress));
                Trace.WriteLine(command);
                var responseForClient = PrepareResponse(command);
                ReplyToCommand(responseForClient, state);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                HandleClientDisconnected(state);
            }
        }


        private string PrepareResponse(string command)
        {
            if (ProcessCommand != null)
                return ProcessCommand(command);
            else            
                return command.ToUpper();
        }

        private void ReplyToCommand(string response, StateObject state)
        {
            Socket handlerSocket = state.CommunicationSocket;
            try
            {
                byte[] byteData = Encoding.ASCII.GetBytes(response);
                handlerSocket.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, state);

                Trace.WriteLine(string.Format("Async Server sending data to client {0}", state.ClientIpAddress));
                Trace.WriteLine(response);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                HandleClientDisconnected(state);
            }
        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            StateObject state = (StateObject)asyncResult.AsyncState;
            Socket handlerSocket = state.CommunicationSocket;
            try
            {

                int bytesSent = handlerSocket.EndSend(asyncResult);

                Trace.WriteLine(string.Format("Async Server response sent to client {0}", state.ClientIpAddress));


                state.ReadCounter = 0;
                state.CommandBuilder = new StringBuilder();
                handlerSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveCallback, state);
            }
            catch (Exception exp)
            {
                Trace.WriteLine(exp);
                HandleClientDisconnected(state);
            }

        }

        private void HandleClientDisconnected(StateObject state)
        {
            try
            {
                var command = state.CommandBuilder.ToString();
                Trace.WriteLine(string.Format("Async Server Client {0} disconnected.", state.ClientIpAddress));
                Trace.WriteLine(string.Format("Last data from client: {0}", command ?? ""));
                state.CommunicationSocket.Shutdown(SocketShutdown.Both);
                state.CommunicationSocket.Close();
            }
            catch (Exception)
            {
                
                
            }
            
        }

        public void Dispose()
        {
            _listenerSocket.Close();
            _listenerSocket.Dispose();
        }
    }

    public class StateObject
    {
        public byte[] Buffer = new byte[4096];
        public Socket CommunicationSocket = null;
        public StringBuilder CommandBuilder = new StringBuilder();
        public Int32 ReadCounter = 0;
        public string ClientIpAddress = string.Empty;
    }
}
