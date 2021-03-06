﻿using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using Client;
using Di = System.Diagnostics;
namespace WebTestLongTask
{
    public partial class Default : System.Web.UI.Page
    {

        public static CancellationTokenSource LongTaskCancelation = new CancellationTokenSource();

        protected void Page_Load(object sender, EventArgs e)
        {

            //Di.Trace.WriteLine(string.Format("Default page. Load finished. Postback: {0}", IsPostBack));
        }


        protected Task LongRunningTask(CancellationToken issResetToken)
        {
            CancellationToken manualCancelation = LongTaskCancelation.Token;
            try
            {
                Di.Trace.Write("Long running task started");

                using (var client = new BlockClient(Client.Helper.GetLocalIpv4(), 11000))
                {
                    client.Connect();
                    for (int i = 0; i < 10000; i++)
                    {

                        if (issResetToken.IsCancellationRequested || manualCancelation.IsCancellationRequested)
                            break;

                        string command = string.Format("Sending message {0}", i);
                        string response;

                        if (client.Send(command, out response))
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

        protected void StartTask(object sender, EventArgs e)
        {
            int sleepTime, workerAmount;
            WorkerType workerType;
            ClientType clientType;
            List<IClient> clients = new List<IClient>();

            if (int.TryParse(txtSleepTime.Text, out sleepTime) && int.TryParse(txtWorkerAmount.Text, out workerAmount) &&
                Enum.TryParse(rbtWorkerType.SelectedValue, out workerType) && Enum.TryParse(rbtClientType.SelectedValue, out clientType))
            {
                Di.Trace.WriteLine(string.Format("{0} Starting test. Worker type: {1}, workers count: {2}, clientType: {3}", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture), workerType, workerAmount, clientType));
                var states = new List<WorkerState>();

                var watch = System.Diagnostics.Stopwatch.StartNew();

                if (workerType == WorkerType.StartThread)
                {
                    clients = new List<IClient>(workerAmount);
                    var threadList = new List<Thread>(workerAmount);
                    for (int i = 0; i < workerAmount; i++)
                    {
                        clients.Add(GetClient(clientType));
                        var st = new WorkerState
                        {
                            Client = clients[i],
                            SleepTime = sleepTime,
                            WorkerId = "Thread " + i,
                            Event = new ManualResetEvent(false)
                        };
                        states.Add(st);

                        var thread = new Thread(RunCommunicationInThread);
                        thread.Start(st);
                        threadList.Add(thread);

                    }
                    foreach (var th in threadList)
                    {
                        th.Join();
                    }
                }
                else if (workerType == WorkerType.StartThreadPool)
                {
                    clients = new List<IClient>(workerAmount);
                    for (int i = 0; i < workerAmount; i++)
                    {
                        clients.Add(GetClient(clientType));
                        var st = new WorkerState
                        {
                            Client = clients[i],
                            SleepTime = sleepTime,
                            WorkerId = "ThreadPool " + i,
                            Event = new ManualResetEvent(false)
                        };
                        ThreadPool.QueueUserWorkItem(RunCommunicationInThread, st);
                        states.Add(st);
                    }
                    ManualResetEvent[] eve = states.Where(s => s != null).Select(s => s.Event).ToArray();
                    WaitHandle.WaitAll(eve);
                }

                else if (workerType == WorkerType.StartTask)
                {
                    clients = new List<IClient>(workerAmount);
                    var taskList = new List<Task>();

                    var factory = new TaskFactory(LongTaskCancelation.Token, TaskCreationOptions.LongRunning, TaskContinuationOptions.LongRunning, null);
                    for (int i = 0; i < workerAmount; i++)
                    {
                        clients.Add(GetClient(clientType));
                        var st = new WorkerState
                        {
                            Client = clients[i],
                            SleepTime = sleepTime,
                            WorkerId = "Task " + i,
                            Event = new ManualResetEvent(false)
                        };
                        taskList.Add(factory.StartNew(RunCommunicationInThread, st, LongTaskCancelation.Token));
                    }
                    Task[] arr = taskList.ToArray();
                    Task.WaitAll(arr);
                }

                else if (workerType == WorkerType.QueueTask)
                {
                    clients = new List<IClient>(workerAmount);

                    for (int i = 0; i < workerAmount; i++)
                    {
                        clients.Add(GetClient(clientType));
                        var st = new WorkerState
                        {
                            Client = clients[i],
                            SleepTime = sleepTime,
                            WorkerId = "QueueTask " + i,
                            Event = new ManualResetEvent(false)
                        };
                        HostingEnvironment.QueueBackgroundWorkItem(ct => RunCommunicationInTaskAsync(st));
                        states.Add(st);
                    }

                    ManualResetEvent[] eve = states.Where(s => s != null).Select(s => s.Event).ToArray(); //max 64
                    WaitHandle.WaitAll(eve);
                }

                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Di.Trace.WriteLine(string.Format("{0} Finished test. Worker type: {1}, workers count: {2}, clientType: {3}", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"), workerType, workerAmount, clientType));
                Di.Trace.WriteLine(string.Format("Duration {0}", elapsedMs));
                foreach (var client in clients)
                {
                    try
                    {
                        client.Close();
                        client.Dispose();
                    }
                    catch (Exception)
                    {

                        //ignore
                    }
                }
                foreach (var st in states.Where(st => st.Event != null))
                {
                    try
                    {
                        st.Event.Close();
                        st.Event.Dispose();
                    }
                    catch (Exception)
                    {

                        //ignore
                    }

                }
            }

        }





        private Client.IClient GetClient(ClientType type)
        {
            IPAddress address;

            if (!IPAddress.TryParse(txtIpaddress.Text, out address))
            {
                address = Client.Helper.GetLocalIpv4();
            }


            IClient client;
            if (type == ClientType.Client)
                client = new BlockClient(address, 11000);
            else
                client = new AsyncClient(address, 11000);
            client.Connect();
            return client;
        }

        private static void RunCommunicationInThread(object workerState)
        {

            var state = (WorkerState)workerState;
            IClient client = state.Client;
            string threadId = state.WorkerId;
            ManualResetEvent ev = state.Event;
            
            using (var timerEvent = new AutoResetEvent(false))
            using (var timer = new System.Threading.Timer(st => { timerEvent.Set(); }, null, state.SleepTime, state.SleepTime))
            {
                try
                {
                    if (client.IsConnected)
                    {
                        for (int i = 0; i < 2000; i++)
                        {
                            string response;
                            if (LongTaskCancelation.IsCancellationRequested || !client.Send(string.Format("{0} \t{1} Message id: {2}. \tHere is communication stuff to make message even more longer.", threadId, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"), i), out response))
                            {
                                client.Close();
                                ev.Set();
                                return;
                            }
                            timerEvent.WaitOne();
                        }
                    }
                }
                catch (Exception e)
                {

                    System.Diagnostics.Trace.WriteLine(e);
                }
                ev.Set();
            }

        }

        private async void RunCommunicationInTaskAsync(object workerState)
        {
            var state = (WorkerState)workerState;
            IClient client = state.Client;
            string threadId = state.WorkerId;
            ManualResetEvent ev = state.Event;
            int sleep = Math.Max(state.SleepTime - 10, 10);
            try
            {

                if (client.IsConnected)
                {
                    for (int i = 0; i < 2000; i++)
                    {
                        var timer = Task.Delay(sleep, LongTaskCancelation.Token).ConfigureAwait(false);
                        string response;
                        if (LongTaskCancelation.IsCancellationRequested || !client.Send(string.Format("{0} \t{1} Message id: {2}. \tHere is communication stuff to make message even more longer.", threadId, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff"), i), out response))
                        {
                            client.Close();
                            ev.Set();
                            return;
                        }
                        await timer;
                    }
                }
            }
            catch (Exception e)
            {

                System.Diagnostics.Trace.WriteLine(e);
            }
            ev.Set();
        }





    }


    public class WorkerState
    {
        public IClient Client { get; set; }
        public int SleepTime { get; set; }
        public string WorkerId { get; set; }
        public ManualResetEvent Event { get; set; }
    }

    enum WorkerType
    {
        QueueTask,
        StartTask,
        StartThread,
        StartThreadPool
    }

    enum ClientType
    {
        Client,
        ClientAsync
    }
}