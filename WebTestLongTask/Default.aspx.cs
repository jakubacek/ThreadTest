using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using ClientAsync;

namespace WebTestLongTask
{
    public partial class Default : System.Web.UI.Page
    {
        protected static CancellationTokenSource Cancelation = new CancellationTokenSource();


        protected void Page_Load(object sender, EventArgs e)
        {

            if (!Page.IsPostBack)
            {
                Func<CancellationToken, Task> fn = LongRunningTask;
                HostingEnvironment.QueueBackgroundWorkItem(canc => LongRunningTask(Cancelation.Token));
            }

            Trace.Write(string.Format("Default page. Load finished. Postback: {0}", IsPostBack));
        }


        protected Task LongRunningTask(CancellationToken token)
        {
            try
            {
                Trace.Write("Long running task started");

                using (var client = new Client(ClientAsync.Helper.GetLocalIpv4(), 11000))
                {
                    client.Connect();
                    for (int i = 0; i < 10000; i++)
                    {
                        
                        if (token.IsCancellationRequested)
                            break;

                        string command = string.Format("Sending message {0}", i);
                        string response;
                        
                        client.SendMessageToServer(command, out response);
                        Trace.Write("Message sent to server");
                        Trace.Write(string.Format("{0} > {1}", command, response));
                    }
                    client.Close();
                }



            }
            catch (Exception e)
            {

                Trace.Write("Error in long running task");
                Trace.Write(e.Message);
                Trace.Write(e.StackTrace);
            }
            return Task.FromResult(true);

        }

        protected void CancelTask(object sender, EventArgs e)
        {
            Cancelation.Cancel();
        }
    }
}