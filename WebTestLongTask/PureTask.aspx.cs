using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using WebTestLongTask.Test;
using System.Net;
using System.Threading.Tasks;
using System.Web.Hosting;
using Di = System.Diagnostics;
using System.Globalization;

namespace WebTestLongTask
{
    public partial class PureTask : System.Web.UI.Page
    {


        public static CancellationTokenSource TokenSource = new CancellationTokenSource();
        private static Di.Stopwatch _watch;
        private static int _taskCouter;
        private readonly object _lock = new object();
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        protected void btnStop_OnClick(object sender, EventArgs e)
        {
            TokenSource.Cancel();
        }

        protected void btnStart_OnClick(object sender, EventArgs e)
        {
            TokenSource = new CancellationTokenSource();
            _watch = Di.Stopwatch.StartNew();
            _taskCouter = 64;

            Di.Trace.WriteLine(string.Format("{0} Starting test. Worker type: {1}, workers count: {2}, clientType: {3}", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture), "Pure Async", 64, "Async TcpClient"));

            TokenSource = new CancellationTokenSource();
            int delay;
            if (!int.TryParse(txtDelay.Text, out delay))
                delay = 20;

            for (int i = 0; i < 64; i++)
            {
                var i1 = i;
                HostingEnvironment.QueueBackgroundWorkItem(ct =>  RunCommunicationInTaskAsync(TokenSource.Token, delay, i1));
                
            }
        }


        private async void RunCommunicationInTaskAsync(CancellationToken token, int delay, int workerId)
        {          

            Communicator comm = null;            
            try
            {
                comm = new Communicator(txtIpdAddress.Text, 11000, workerId);

                await comm.Connect();

                for (int i = 0; i < 2000; i++)
                {

                    bool isWorking = await comm.ReadData(token).ConfigureAwait(false);
                    if (!isWorking)
                    {
                        comm.IsWorking = false;
                        break;
                    }                        
                    await Task.Delay(delay, token).ConfigureAwait(false);                    
                }

                //TODO !comm.IsWorkint restart ....

            }
            catch (Exception e)
            {
                
                Di.Trace.WriteLine(e);
            }
            finally
            {
                lock (_lock)
                {
                    _taskCouter--;
                    if (_taskCouter == 0)
                    {
                        _watch.Stop();
                        var elapsedMs = _watch.ElapsedMilliseconds;
                        Di.Trace.WriteLine(string.Format("{0} Finished test. Worker type: {1}, workers count: {2}, clientType: {3}", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"), "Pure Async", 64, "Async TcpClient"));
                        Di.Trace.WriteLine(string.Format("Duration {0}", elapsedMs));
                    }
                }
               
                if (comm != null)
                    await comm.Close().ConfigureAwait(false);
            }
        }

    }
}