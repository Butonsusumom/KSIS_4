using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("HTTP SERVER\n");
            SimpleHTTPServer server;
            string path;
            //C:\Users\agcl3\OneDrive\Рабочий стол\Новая папка\http.html
            Console.WriteLine("Enter Path");
            path = Console.ReadLine();
            Console.WriteLine("Enter end to exit\n");

            server = new SimpleHTTPServer(path);

            while (Console.ReadLine() != "end")
                Console.WriteLine("Enter end to exit\n");

            server.Stop();
        }
    }
}
