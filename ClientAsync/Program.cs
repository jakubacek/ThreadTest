using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace ClientAsync
{
    class Program
    {
        static void Main(string[] args)
        {

            using (var client = new Client(Helper.GetLocalIpv4(),11000))
            {
                client.Connect();
                for (int i = 0; i < 100; i++)
                {
                    string response;
                    client.SendMessageToServer(string.Format("Ahoj {0}",i), out response);
                    Thread.Sleep(100);
                    Console.WriteLine(response);
                }
                client.Close();                
            }
        }
    }
}
