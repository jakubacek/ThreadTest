using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Client;

namespace WebTestHangFireTasks
{
    public static class TaskRepo
    {

        public static ICollection<string> GetFeeds()
        {
            var titles = new List<string>();
            using (var reader = new RssReader("http://idnes.cz.feedsportal.com/c/34387/f/625936/index.rss"))
            {
                titles.AddRange(reader.Execute().Select(i => i.Title));
            }
            using (var reader = new RssReader("http://idnes.cz.feedsportal.com/c/34387/f/625937/index.rss"))
            {
                titles.AddRange(reader.Execute().Select(i => i.Title));
            }
            return titles;
        }

        public static void GetFeedsAndWriteTrace()
        {
            var feeds = GetFeeds();
            Helper.TraceLine("Started GetFeedsAndWriteTrace");
            feeds.ToList().ForEach(i=> Helper.TraceLine(i));
            Helper.TraceLine("Finished GetFeedsAndWriteTrace");
        }


        public static void SendMessagesToServer(IEnumerable<string> messages)
        {
            Helper.TraceLine("Started SendMessagesToClient Task");
            using (var client = new BlockClient(Client.Helper.GetLocalIpv4(), 11000))
            {
                client.Connect();
                foreach (var message in messages)
                {
                    string response;
                    client.Send(message, out response);
                    Thread.Sleep(500);
                    Helper.TraceLine("SendMessagesToClient(...{0}{1} > {2}", Environment.NewLine, message, response);
                }
                client.Close();
            }
            Helper.TraceLine("Finished SendMessagesToClient Task");
        }


    }
}
