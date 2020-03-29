using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTP_Server
{
    class CommandDictionary : Dictionary<string, Command>
    {
        public CommandDictionary()
        {
            Add("USER", new USERcommand());
            Add("PASS", new PASScommand());
            Add("SYST", new SYSTcommand());
            Add("PWD", new PWDcommand());
            Add("TYPE", new TYPEcommand());
            Add("PORT", new PORTcommand());
            Add("LIST", new LISTcommand());
            Add("PASV", new PASVcommand());
            Add("CWD", new CWDcommand());
            Add("RETR", new RETRcommand());
            Add("STOR", new STORcommand());
            Add("DELE", new DELEcommand());
            Add("RNFR", new RNFRcommand());
            Add("RNTO", new RNTOcommand());
            Add("RMD", new RMDcommand());
            Add("MKD", new MKDcommand());
            Add("QUIT", new QUITcommand());
            Add("NOOP", new NOOPcommand());
        }
    }
}
