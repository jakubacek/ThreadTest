using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {

            using (var client = new AsyncClient(Helper.GetLocalIpv4(),11000))
            {
                client.Connect();
                for (int i = 0; i < 100; i++)
                {
                    string response;
                    client.Send(string.Format("Ahoj {0}",i), out response);
                    Thread.Sleep(100);
                    Console.WriteLine(response);
                }
                client.Close();                
            }
        }
    }
}
