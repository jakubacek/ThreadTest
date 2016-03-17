using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class StateObject
    {
        public byte[] Buffer = new byte[1024];
        public StringBuilder CommandBuilder = new StringBuilder();
    }
}
