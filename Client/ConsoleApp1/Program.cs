using System;

namespace FTPClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Client ftp = new Client(@"ftp://169.254.27.55/", "root", "root");   //создаем нового клиента
            while (true)
            {
                string command = Console.ReadLine();  //считываем с консоли команду
                string[] words = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);  //рразбиваем ее на слова
                if (String.Compare(words[0], "DELE") == 0)  //удаление файла
                    Console.WriteLine(ftp.DeleteFile(words[1]));
                else
                    if (String.Compare(words[0], "STOR") == 0)  //загрузка на сервер
                    Console.WriteLine(ftp.UploadFile(words[1], words[2]));
                else
                    if (String.Compare(words[0], "RETR") == 0)  //загрузка с сервера
                    Console.WriteLine(ftp.DownloadFile(words[1], words[2]));
                else
                    if (String.Compare(words[0], "LIST") == 0)  //вывод полного содержимого директории
                {
                    string[] dir;
                    dir = ftp.ListDirectoryDetails();
                    if (dir != null)
                    {
                        foreach (string file in dir)
                        {
                            Console.WriteLine(file);
                        }
                    }
                }
                else
                    if (String.Compare(words[0], "END") == 0)  //загрузка с сервера
                    break;
                else
                    Console.WriteLine("Incorrect Command");  //неверная комманда

            }
        }
    }
}
