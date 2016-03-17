using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
using System.Threading;
using System.IO;
using System.Text;
using System.Web.UI.WebControls.WebParts;

namespace WebTestLongTask.Test
{
    public class Communicator
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private int _port;
        private string _address;
        private int _readTimeout;

        private readonly Encoding _encoding = Encoding.ASCII;
        public string WorkerId
        {
            get;
            private set;
        }
        public Boolean IsWorking { get; set; }


        public List<CommunicationLog> Log = new List<CommunicationLog>();

        public Communicator(string address, int port, int workerId, int readTimeOut = 500)
        {
            _port = port;
            _address = address;
            _readTimeout = readTimeOut;
            WorkerId = String.Format("Task {0}", workerId);
            
        }

        public async Task<bool> Connect()
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_address, _port).ConfigureAwait(false);
                _stream = _client.GetStream();
                IsWorking = true;

            }
            catch (Exception exp)
            {
                System.Diagnostics.Trace.Write(exp.Message);
            }
            return await Task.FromResult(_client.Connected).ConfigureAwait(false);
        }

        public async Task Close()
        {
            await Task.Yield();

            if (_client != null)
            {
                _client.Close();
                _client = null;
            }

            if (_stream != null)
            {
                _stream.Close();
                _stream.Dispose();
                _stream = null;
            }           
        }

        private async Task SendMessage(string message, CancellationToken token)
        {
            var sendData = _encoding.GetBytes(message);
            await _stream.WriteAsync(sendData, 0, sendData.Length, token).ConfigureAwait(false);
            await _stream.FlushAsync(token).ConfigureAwait(false);
        }

        private async Task<string> ReceiveMessage(CancellationToken token)
        {
            var buffer = new byte[1024];
            var sb = new StringBuilder();

            using (var readTimeOutTokenSource = new CancellationTokenSource(_readTimeout)) //TODO poresit co delat s taskem kdyz timeoutne            
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(readTimeOutTokenSource.Token, token))
            {
                do
                {
                    var readbytes = await _stream.ReadAsync(buffer, 0, 1024, linkedCts.Token).ConfigureAwait(false);
                    sb.Append(_encoding.GetString(buffer, 0, readbytes));

                } while (_stream.DataAvailable);
            }
            return sb.ToString();
        }

        public async Task<bool> ReadData(CancellationToken token)
        {
            var log = new CommunicationLog { Created = DateTime.Now };
            log.Request = string.Format("{0} \t{1} Message id: {2}. \tHere is communication stuff to make message even more longer.", WorkerId, log.Created.ToString("yyyy-MM-ddTHH:mm:ss.fff"), Log.Count);
            Log.Add(log);
            try
            {
                await SendMessage(log.Request, token).ConfigureAwait(false);                
                log.Response = await ReceiveMessage(token).ConfigureAwait(false); //TODO poresit co delat s taskem kdyz timeoutne
                log.Received = DateTime.Now;
            }
            catch (Exception exp)
            {
                System.Diagnostics.Trace.Write(exp.Message);
                return false;
            }
            return true;
        }
    }

    public class CommunicationLog
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Received { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
    }
}