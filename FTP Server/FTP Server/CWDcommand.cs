using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FTP_Server
{
    class CWDcommand : Command
    {
        public override string Execute(string pathname, ClientConnection connection)
        {
            if (pathname == "/")
            {
                connection.currentDirectory = connection.root;
            }
            else
            {
                string newDir;

                if (pathname.StartsWith("/"))
                {
                    pathname = pathname.Substring(1).Replace('/', '\\');
                    newDir = Path.Combine(connection.root, pathname);
                }
                else
                {
                    pathname = pathname.Replace('/', '\\');
                    newDir = Path.Combine(connection.currentDirectory, pathname);
                }

                if (Directory.Exists(newDir))
                {
                    connection.currentDirectory = new DirectoryInfo(newDir).FullName;

                    if (!connection.IsPathValid(connection.currentDirectory))
                    {
                        connection.currentDirectory = connection.root;
                    }
                }
                else
                {
                    connection.currentDirectory = connection.root;
                }
            }

            return "250 Changed to new directory";
        }
    }
}
