using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTP_Server
{
    abstract class Command
    {
        public abstract string Execute(string arguments, ClientConnection connection);
    }
}
