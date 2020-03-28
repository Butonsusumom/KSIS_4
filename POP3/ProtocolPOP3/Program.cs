using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;

namespace ProtocolPOP3 {
    internal class Program {
        public static void Main(string[] args) {
            Console.Write("POP3\n");
            TcpClient tcpClient = new TcpClient();               
            tcpClient.Connect("pop.mail.ru", 995);
            SslStream sslStream = new SslStream(tcpClient.GetStream());
            sslStream.AuthenticateAsClient("pop.mail.ru");

            StreamWriter streamWriter = new StreamWriter(sslStream);
            StreamReader streamReader = new StreamReader(sslStream);

            string receivedInfo = "";
            string line = "";
            streamWriter.WriteLine("USER k.tsybulkoo@mail.ru");
            streamWriter.Flush();
            receivedInfo = streamReader.ReadLine();
            Console.WriteLine(receivedInfo);
            streamWriter.WriteLine("PASS Blumblum1@");
            streamWriter.Flush();
            receivedInfo = streamReader.ReadLine();
            Console.WriteLine(receivedInfo);
            while ((receivedInfo = streamReader.ReadLine()) != null)
            {
                Console.WriteLine(receivedInfo);
                line = Console.ReadLine();
                streamWriter.WriteLine(line);
                streamWriter.Flush();
            }
            Console.Write("END");
            Console.Read();
        }
    }
}
