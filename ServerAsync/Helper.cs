using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


namespace TestThread
{
    public class Helper
    {


        public static IPAddress GetLocalIpv4()
        {
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            return ipHostInfo.AddressList.First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

        public static string GetIpAddressString(IPEndPoint remoteEndPoint)
        {
            if (remoteEndPoint != null)
            {
                return remoteEndPoint.Address.ToString();
            }
            return string.Empty;
        }
    }
}
