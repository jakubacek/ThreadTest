using System;
using System.Diagnostics;
using System.Threading;

namespace WebTestOneThread
{
    public class Global : System.Web.HttpApplication
    {        
        private const int HandledExceptionBreakModulo = 13;
        private const int UnhandledExceptionBreakModulo = 47;

        protected void Application_Start(object sender, EventArgs e)
        {
            Application["Counter"] = 0;
            Trace.WriteLine(string.Format("{0}*******************{0}Application restarted.", Environment.NewLine));            
            StartDaemon();
        }

        private void StartDaemon()
        {
            Thread t = new Thread(new ThreadStart(Childthreadcall)) {IsBackground = true};
            t.Start();
            Trace.WriteLine(string.Format("{0}------------------{0}Thread started.", Environment.NewLine));            
        }


        void Application_Error(object sender, EventArgs e)
        {
            //This will never happend.
            Trace.WriteLine(string.Format("{0}------------------{0}Application error handler.", Environment.NewLine));            
        }

        private void Childthreadcall()
        {

            int counter = (Int32)Application["Counter"];
            do
            {
                counter++;

                if (counter % UnhandledExceptionBreakModulo == 0)
                {
                    //This error crash whole application - Unhandled exception
                    var ex = new Exception("Unhandled fake exception. Simulation of unhandled exception.");
                    Trace.WriteLine(string.Format("{1}=============={1}Fatal {1}Last counter: {0}{1}{2}{1}{3}", counter, Environment.NewLine, ex.Message, ex.StackTrace));                    
                    throw ex;
                }
                try
                {                    
                    Application["Counter"] = counter;
                    Thread.Sleep(1000);
                    if (counter % HandledExceptionBreakModulo == 0)
                        throw new Exception("Handled fake exception. Simulation of error.");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("{1}/////////////{1}Exception{1}Last counter: {0}{1}{2}{1}{3}", counter, Environment.NewLine, ex.Message, ex.StackTrace));
                    
                }
            } while (true);
        }  

    }
}