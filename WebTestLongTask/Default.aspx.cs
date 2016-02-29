using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using ClientAsync;
using Di = System.Diagnostics;
namespace WebTestLongTask
{
    public partial class Default : System.Web.UI.Page
    {
        protected static CancellationTokenSource LongTaskCancelation = new CancellationTokenSource();        


        protected void Page_Load(object sender, EventArgs e)
        {

            if (!Page.IsPostBack)
            {                
                HostingEnvironment.QueueBackgroundWorkItem(iisReset => LongRunningTask(iisReset));
            }

            Di.Trace.WriteLine(string.Format("Default page. Load finished. Postback: {0}", IsPostBack));
        }


        protected Task LongRunningTask(CancellationToken issResetToken)
        {
            CancellationToken manualCancelation = LongTaskCancelation.Token;
            try
            {
                Di.Trace.Write("Long running task started");

                using (var client = new Client(ClientAsync.Helper.GetLocalIpv4(), 11000))
                {
                    client.Connect();
                    for (int i = 0; i < 10000; i++)
                    {
                        
                        if (issResetToken.IsCancellationRequested || manualCancelation.IsCancellationRequested)
                            break;

                        string command = string.Format("Sending message {0}", i);
                        string response;

                        if (client.SendMessageToServer(command, out response))
                        {
                            Di.Trace.WriteLine("Message sent to server");
                            Di.Trace.WriteLine(string.Format("{0} > {1}", command, response));
                        }
                        else
                        {
                            Di.Trace.WriteLine("Can't send message to server. Probably error in communication");
                            Di.Trace.WriteLine("Ending long running task.");
                            break;                            
                        }
                    }
                    client.Close();
                }
            }
            catch (Exception e)
            {

                Di.Trace.WriteLine("Error in long running task");
                Di.Trace.WriteLine(e.Message);
                Di.Trace.WriteLine(e.StackTrace);
            }
            return Task.FromResult(false);
        }

        protected void CancelTask(object sender, EventArgs e)
        {
            LongTaskCancelation.Cancel();
            Di.Trace.WriteLine("Task cancelled.");
        }
    }
}