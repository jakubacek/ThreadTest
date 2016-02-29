using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientAsync;
using Hangfire;
using WebGrease.Css.Extensions;
using Di = System.Diagnostics;
using System.Threading;
using System.Net;

namespace WebTestHangfire
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnStartClient_OnClick(object sender, EventArgs e)
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

            //SendMessagesToClient(titles);

            //Background task
            BackgroundJob.Enqueue(() => SendMessagesToClient(titles.ToArray()));
        }

        public void SendMessagesToClient(ICollection<string> messages)
        {
            using (var client = new Client(ClientAsync.Helper.GetLocalIpv4(), 11000))
            {
                client.Connect();
                foreach (var message in messages)
                {
                    string response;
                    client.SendMessageToServer(message, out response);
                    Thread.Sleep(500);
                    Helper.TraceLine("SendMessagesToClient(...{0}{1} > {2}", Environment.NewLine, message, response);
                }
                client.Close();
            }
        }
    }
}