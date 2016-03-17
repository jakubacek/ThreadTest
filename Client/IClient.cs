using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public interface IClient : IDisposable
    {
        IEnumerable<string> CommunicationLog { get;}
        bool IsConnected { get; }
        bool Connect();
        bool Send(string input, out string response);
        void Close();
    }
}
