using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebTestHangfire
{
    public class Helper
    {
        public static void TraceLine(String format, params object[] args)
        {
            var newFormat = DateTime.Now + ": " + format;
            System.Diagnostics.Trace.WriteLine(string.Format(format, args));
        }

    }
}



