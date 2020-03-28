using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("HTTP CLIENT\n");
            string url, value;
           
            Console.Write("Enetr Link: ");
            url = Console.ReadLine();
            Console.Write("POST: ");
            value = Console.ReadLine();
            Console.WriteLine();

            Task.Run(() => SendRequests(url, value));
            Console.ReadLine();
        }


        /// https://journal.bsuir.by/api/v1/studentGroup/schedule?studentGroup=851002
        /// http://httpbin.org/post
        /// 
        static async Task SendRequests(string url, string value)
        {
            using (HttpClient client = new HttpClient())
            {
                //Sends GET request and prints server's response
                var result = await client.GetAsync(url);
                Console.WriteLine("GET");
                Console.WriteLine("{0} {1} HTTP/{2}\n", result.RequestMessage.Method, result.RequestMessage.RequestUri, result.RequestMessage.Version);
                string resultContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine(result + "\n");
                Console.WriteLine(resultContent + "\n");

                //Sends HEAD request and prints the response
                result = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                Console.WriteLine("HEAD");
                Console.WriteLine("{0} {1} HTTP/{2}\n", result.RequestMessage.Method, result.RequestMessage.RequestUri, result.RequestMessage.Version);
                Console.WriteLine(result + "\n");

                //Sends POST request with variable, passed to this method, and print the response
                FormUrlEncodedContent content = new FormUrlEncodedContent(new[] {new KeyValuePair<string, string>("Variable", value)});
                result = await client.PostAsync(url, content);
                Console.WriteLine("POST");
                Console.WriteLine("{0} {1} HTTP/{2}\n", result.RequestMessage.Method, result.RequestMessage.RequestUri, result.RequestMessage.Version);
                resultContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine(result + "\n");
                Console.WriteLine(resultContent);
            }
        }
    }
}
