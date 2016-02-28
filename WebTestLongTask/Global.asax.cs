using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace WebTestLongTask
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {            
            Trace.WriteLine(String.Format("Application started {0}", DateTime.Now));
        }

        void Application_Error(object sender, EventArgs e)
        {
            Trace.WriteLine("Error");
        }



    }
}