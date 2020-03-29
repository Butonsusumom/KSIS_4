using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTP_Server
{
    class LISTcommand : Command
    {
        public override string Execute(string pathname, ClientConnection connection)
        {
            pathname = connection.NormalizeFilename(pathname);

            if (pathname != null)
            {
                var state = new ClientConnection.DataConnectionOperation { Arguments = pathname, Operation = connection.ListOperation };

                connection.SetupDataConnectionOperation(state);

                return string.Format("150 Opening {0} mode data transfer for LIST", connection.dataConnectionType);
            }

            return "450 Requested file action not taken";
        }
    }
}
