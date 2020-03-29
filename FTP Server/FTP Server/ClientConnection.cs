using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace FTP_Server
{
    class ClientConnection
    {
        public class DataConnectionOperation
        {
            public Func<NetworkStream, string, string> Operation { get; set; }
            public string Arguments { get; set; }
        }

        public enum TransferType
        {
            Ascii,
            Ebcdic,
            Image,
            Local,
        }

        public enum DataConnectionType
        {
            Passive,
            Active,
        }

        public TcpListener passiveListener;

        public TcpClient connectedControlClient;
        private TcpClient dataClient;

        private NetworkStream stream;
        private StreamReader reader;
        private StreamWriter writer;
        private CommandDictionary commandDictionary = new CommandDictionary();

        public string connectedClientusername;
        public string root = "C:\\Store";
        public string currentDirectory;
        public string renameFrom = null;
        public TransferType connectionType = TransferType.Ascii;

        //public bool authorized = false;
        public User currentUser;
        private List<string> validCommands;

        public DataConnectionType dataConnectionType = DataConnectionType.Active;
        public IPEndPoint dataEndpoint;

        public ClientConnection(TcpClient client)
        {
            connectedControlClient = client;
            stream = connectedControlClient.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            validCommands = new List<string>();
        }

        private string CheckUser()
        {
            if (currentUser == null)
            {
                return "530 Not logged in";
            }

            return null;
        }

        public bool IsPathValid(string path)
        {
            return path.StartsWith(root);
        }

        public string NormalizeFilename(string path)
        {
            if (path == null)
            {
                path = string.Empty;
            }

            if (path == "/")
            {
                return root;
            }
            else if (path.StartsWith("/"))
            {
                path = new FileInfo(Path.Combine(root, path.Substring(1))).FullName;
            }
            else
            {
                path = new FileInfo(Path.Combine(currentDirectory, path)).FullName;
            }

            return IsPathValid(path) ? path : null;
        }

        private static long CopyStream(Stream input, Stream output, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int count = 0;
            long total = 0;

            while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, count);
                total += count;
            }

            return total;
        }

        private static long CopyStreamAscii(Stream input, Stream output, int bufferSize)
        {
            char[] buffer = new char[bufferSize];
            int count = 0;
            long total = 0;

            using (StreamReader rdr = new StreamReader(input, Encoding.Default))
            {
                using (StreamWriter wtr = new StreamWriter(output, Encoding.Default))
                {
                    while ((count = rdr.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        wtr.Write(buffer, 0, count);
                        total += count;
                    }
                }
            }

            return total;
        }

        private long CopyStream(Stream input, Stream output)
        {
            Stream limitedStream = output;

            if (connectionType == TransferType.Image)
            {
                return CopyStream(input, limitedStream, 4096);
            }
            else
            {
                return CopyStreamAscii(input, limitedStream, 4096);
            }
        }

        public string ListOperation(NetworkStream dataStream, string pathname)
        {
            StreamWriter dataWriter = new StreamWriter(dataStream, Encoding.Default);

            IEnumerable<string> directories = Directory.EnumerateDirectories(pathname);

            foreach (string dir in directories)
            {
                DirectoryInfo d = new DirectoryInfo(dir);

                string date = d.LastWriteTime < DateTime.Now - TimeSpan.FromDays(180) ?
                    d.LastWriteTime.ToString("M dd yyyy"):
                    d.LastWriteTime.ToString("M dd HH:mm");

                string line = string.Format("drwxr-xr-x 1 owner group {0,8} {1} {2}", "4096", date, d.Name);

                dataWriter.WriteLine(line);
                dataWriter.Flush();
            }

            IEnumerable<string> files = Directory.EnumerateFiles(pathname);

            foreach (string file in files)
            {
                FileInfo f = new FileInfo(file);

                string date = f.LastWriteTime < DateTime.Now - TimeSpan.FromDays(180) ?
                    f.LastWriteTime.ToString("M dd yyyy") :
                    f.LastWriteTime.ToString("M dd HH:mm");

                string line = string.Format("-rw-r--r-- 1 owner group {0,8} {1} {2}", f.Length, date, f.Name); ;

                dataWriter.WriteLine(line);
                dataWriter.Flush();
            }

            return "226 Transfer complete";
        }

        public string RetrieveOperation(NetworkStream dataStream, string pathname)
        {
            long bytes = 0;

            using (FileStream fs = new FileStream(pathname, FileMode.Open, FileAccess.Read))
            {
                bytes = CopyStream(fs, dataStream);
            }

            return "226 Closing data connection, file transfer successful";
        }

        public string StoreOperation(NetworkStream dataStream, string pathname)
        {
            long bytes = 0;

            using (FileStream fs = new FileStream(pathname, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan))
            {
                bytes = CopyStream(dataStream, fs);
            }

            return "226 Closing data connection, file transfer successful";
        }

        private void HandleAsyncResult(IAsyncResult result)
        {
            if (dataConnectionType == DataConnectionType.Active)
            {
                dataClient.EndConnect(result);
            }
            else
            {
                dataClient = passiveListener.EndAcceptTcpClient(result);
            }
        }

        public void SetupDataConnectionOperation(DataConnectionOperation state)
        {
            if (dataConnectionType == DataConnectionType.Active)
            {
                dataClient = new TcpClient(dataEndpoint.AddressFamily);
                dataClient.BeginConnect(dataEndpoint.Address, dataEndpoint.Port, DoDataConnectionOperation, state);
            }
            else
            {
                passiveListener.BeginAcceptTcpClient(DoDataConnectionOperation, state);
            }
        }

        private void DoDataConnectionOperation(IAsyncResult result)
        {
            HandleAsyncResult(result);

            DataConnectionOperation op = result.AsyncState as DataConnectionOperation;

            string response;

            using (NetworkStream dataStream = dataClient.GetStream())
            {
                response = op.Operation(dataStream, op.Arguments);
            }

            dataClient.Close();
            dataClient = null;

            writer.WriteLine(response);
            writer.Flush();
        }

        public void HandleClient(object obj)
        {
            writer.WriteLine("220 SERVICE READY.");
            writer.Flush();

            validCommands.AddRange(new string[] {"USER", "PASS", "QUIT", "CWD", "PORT", "PASV", "TYPE", "RETR", "LIST", "PWD", "NOOP"});

            string line;

            try
            {
                while(!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    string response = null;
                    string[] command = line.Split(' ');
                    string cmd = command[0].ToUpper();
                    string arguments = command.Length > 1 ? line.Substring(command[0].Length + 1) : null;
                    Command currentCommand;

                    if (!validCommands.Contains(cmd))
                    {
                        response = CheckUser();
                    } 

                    if (response == null)
                    {
                        if (commandDictionary.ContainsKey(cmd))
                        {
                            commandDictionary.TryGetValue(cmd, out currentCommand);
                            response = currentCommand.Execute(arguments, this);
                        }
                        else
                        {
                            response = "502 COMMAND NOT SUPPORTED";
                        }
                    }
                    
                    if (connectedControlClient == null || !connectedControlClient.Connected)
                    {
                        break;
                    }
                    else
                    {
                        writer.WriteLine(response);
                        writer.Flush();
                        if (response.StartsWith("221"))
                        {
                            break;
                        }
                    } 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }


        public void Dispose()
        {
            connectedControlClient.Close();
            stream.Close();
            reader.Close();
            writer.Close();
        }

    }
}
