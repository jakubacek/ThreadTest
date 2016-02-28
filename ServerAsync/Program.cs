using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Net.Http;
using System.Threading;


namespace TestThread
{
    class Program
    {
        

        static void Main(string[] args)
        {
            var ip = Helper.GetLocalIpv4();
            using (var server = new Server(ip, 11000))
            {
                ThreadPool.QueueUserWorkItem(server.Start,null);
                Console.ReadLine();
            }
          
        }
    }
}
