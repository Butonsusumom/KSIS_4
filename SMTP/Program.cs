using System;
using System.Linq;
using SmtpProtocol.Properties.client;
using SmtpProtocol.Properties.server;

namespace SmtpProtocol
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.Write("SMTP\n");
            var server = new Server(25, new AuthorizedClient("ksis.test@mail.ru", "HaHajatutzhivu"));
            const string command = "/changer";
            const string exit = "/exit";

            Console.Write("Enter Adress: ");
            Client receiver = new Client(Console.ReadLine());
            Console.WriteLine(String.Join(" ","Enter command to change reiver",command));
            Console.WriteLine(String.Join(" ","Enter command to exit",exit));
            for (;;)
            {

                Console.Write("Message : ");
                var text = Console.ReadLine();
                
                switch (text)
                {
                    case command:
                    {
                        Console.Write("Enter Adress: ");
                        receiver = new Client(Console.ReadLine());
                        break;
                    }
                    case exit: return;
                    default:
                    {
                        server.SendMessage(receiver, text);
                        break;
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
