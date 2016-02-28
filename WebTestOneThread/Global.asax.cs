using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Threading;
using System.IO;

namespace WebTest3
{
    public class Global : System.Web.HttpApplication
    {
        //TODO Change path!
        private const string FilePath = @"D:\DEV_TST\MASA\ProofOfConcept\WebTestOneThread\Log\Exception.txt";
        private const int HandledExceptionBreakModulo = 13;
        private const int UnhandledExceptionBreakModulo = 47;

        protected void Application_Start(object sender, EventArgs e)
        {
            Application["Counter"] = 0;
            File.AppendAllText(FilePath, string.Format("{0}*******************{0}Application restarted.{0}", Environment.NewLine));

            StartDaemon();
        }

        private void StartDaemon()
        {
            Thread t = new Thread(new ThreadStart(Childthreadcall)) {IsBackground = true};
            t.Start();
            File.AppendAllText(FilePath, string.Format("{0}------------------{0}Thread started.{0}", Environment.NewLine));            
        }


        void Application_Error(object sender, EventArgs e)
        {
            //This will never happend.
            File.AppendAllText(FilePath, string.Format("{0}------------------{0}Application error handler.{0}", Environment.NewLine));
        }

        private void Childthreadcall()
        {

            int counter = (Int32)Application["Counter"];
            do
            {
                counter++;

                if (counter % UnhandledExceptionBreakModulo == 0)
                {
                    //This error crash whole application.
                    //Unhandled exception
                    var ex = new Exception("Unhandled fake exception. Simulation of unhandled exception.");
                    File.AppendAllText(FilePath, string.Format("{1}=============={1}Fatal {1}Last counter: {0}{1}{2}{1}{3}{1}", counter, Environment.NewLine, ex.Message, ex.StackTrace));
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
                    File.AppendAllText(FilePath, string.Format("{1}/////////////{1}Exception{1}Last counter: {0}{1}{2}{1}{3}{1}", counter, Environment.NewLine, ex.Message, ex.StackTrace));
                }
            } while (true);
        }  

    }
}