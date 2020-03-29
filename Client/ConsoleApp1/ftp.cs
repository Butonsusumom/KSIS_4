using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

//тут представленая собственная реализация класса FtpWebRequest, тк в шарпах есть очень много функций для работы с FTP 

namespace FTPClient
{
	public class Client
	{
		private string password;
		private string userName;
		private string uri;
		private int bufferSize = 1024;

		public bool Passive = true; //передача в пассивном режиме (клиент только слушает, а не открывает соединение сам)
		public bool Binary = true;  //файлы передаются в бинарном виде
		public bool EnableSsl = false; //используется "защищенная" передача файлов, т.е. использвуется шифрование
		public bool Hash = false; 

		public Client(string uri, string userName, string password)       //конструктор нашего класса, uri - идентификатор ресурса (у нас это локальный адрес сервера)
		{																  //username и password в нашем случае root,
			this.uri = uri;												  //они используются для идентификации клиента
			this.userName = userName;
			this.password = password;
		}

		public string ChangeWorkingDirectory(string path)        //изменение рабочей директории
		{
			uri = combine(uri, path);

			return PrintWorkingDirectory();
		}

		public string DeleteFile(string fileName)  //DELE - удаление файла
		{
			var request = createRequest(combine(uri, fileName), WebRequestMethods.Ftp.DeleteFile);

			return getStatusDescription(request);
		}
		public string DownloadFile(string source, string dest)  //RETR - загрузка файлов с сервера
		{
			var request = createRequest(combine(uri, source), WebRequestMethods.Ftp.DownloadFile);  //создаем запрос
			string exep;
			byte[] buffer = new byte[bufferSize];
			try
			{
				using (var response = (FtpWebResponse)request.GetResponse())  //получаем ответ сервера
				{
					using (var stream = response.GetResponseStream())  //получаем поток, используемый для чтения текста ответа сервера
					{
						using (var fs = new FileStream(dest, FileMode.OpenOrCreate)) //создаем поток для передаваемого файла
						{
							int readCount = stream.Read(buffer, 0, bufferSize);  //считываем последовательность байт из текущего потока

							while (readCount > 0)
							{
								if (Hash)
									Console.Write("#");

								fs.Write(buffer, 0, readCount);
								readCount = stream.Read(buffer, 0, bufferSize);  //считываем последовательность байт из текущего потока
							}
						}

					}
					return response.StatusDescription;
				}
			}
			catch (Exception ex)
			{
				exep = ex.Message;
			}
			return exep;
		}

		public DateTime GetDateTimestamp(string fileName)  //MDTM - получение даты и времени последнего измеения файла
		{
			var request = createRequest(combine(uri, fileName), WebRequestMethods.Ftp.GetDateTimestamp); 

			using (var response = (FtpWebResponse)request.GetResponse())
			{
				return response.LastModified;
			}
		}

		public string GetFileSize(string fileName)  //SIZE - получаем размер файла
		{
			string exep;
			var request = createRequest(combine(uri, fileName), WebRequestMethods.Ftp.GetFileSize);  //создаем запрос
			try
			{
				using (var response = (FtpWebResponse)request.GetResponse())  //получаем ответ сервера
				{
					return response.ContentLength.ToString();  //получаем размер файла и преобразуем в строку
				}
			}
			catch (Exception ex)
			{
				exep = ex.Message;
			}
			return exep;
		}

		public string[] ListDirectory()  //NLIST - вывод краткого содержиого директории
		{
			var list = new List<string>();  //создаем список из строк
			string exep;
			var request = createRequest(WebRequestMethods.Ftp.ListDirectory);  //создаем запрос
			try
			{
				using (var response = (FtpWebResponse)request.GetResponse())  //получаем ответ от сервера
				{
					using (var stream = response.GetResponseStream())   //получаем поток, используемый для чтения текста ответа сервера
					{
						using (var reader = new StreamReader(stream, true)) //создаем объект, который считывает байты из потока в определенной кодировке
						{
							while (!reader.EndOfStream)
							{
								list.Add(reader.ReadLine()); //считываем построчно, пока не дойдем до конца потока
							}
						}
					}
				}
				return list.ToArray();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return null;
		}

		public string[] ListDirectoryDetails()   //LIST - вывод полного содержимого директории
		{
			var list = new List<string>();  //создаем список из строк

			var request = createRequest(WebRequestMethods.Ftp.ListDirectoryDetails);  //создаем запрос
			try
			{
				using (var response = (FtpWebResponse)request.GetResponse())  //получаем ответ от сервера
				{
					using (var stream = response.GetResponseStream())  //получаем поток, используемый для чтения текста ответа сервера
					{
						using (var reader = new StreamReader(stream, true))  //создаем объект, который считывает байты из потока в определенной кодировке
						{
							while (!reader.EndOfStream)
							{
								list.Add(reader.ReadLine());   //считываем построчно, пока не дойдем до конца потока
							}
						}
					}
					Console.WriteLine(response.StatusDescription);  
				}
				return list.ToArray();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return null;
		}

		public string MakeDirectory(string directoryName)   //MKD - создание директории
		{
			var request = createRequest(combine(uri, directoryName), WebRequestMethods.Ftp.MakeDirectory);

			return getStatusDescription(request);
		}

		public string PrintWorkingDirectory()  //PWD - вывод имени текущей директории
		{
			var request = createRequest(WebRequestMethods.Ftp.PrintWorkingDirectory);

			return getStatusDescription(request);
		}

		public string RemoveDirectory(string directoryName)  //RMD - удаление директории
		{
			var request = createRequest(combine(uri, directoryName), WebRequestMethods.Ftp.RemoveDirectory);

			return getStatusDescription(request);
		}

		public string Rename(string currentName, string newName)  //RENAME - изменение имени файла
		{
			var request = createRequest(combine(uri, currentName), WebRequestMethods.Ftp.Rename);  //создаем запрос

			request.RenameTo = newName;  //даем файлу новое имя

			return getStatusDescription(request);
		}

		public string UploadFile(string source, string destination)       //STOR - загрузка файла на сервер
		{
			string exep;
			var request = createRequest(combine(uri, destination), WebRequestMethods.Ftp.UploadFile);  //создаем запрос
			try
			{
				using (var stream = request.GetRequestStream()) //возвращает поток, используемый для выгрузки данных на сервер
				{
					using (var fileStream = System.IO.File.Open(source, FileMode.Open)) //создаем поток для передаваемого файла
					{
						int num;

						byte[] buffer = new byte[bufferSize];

						while ((num = fileStream.Read(buffer, 0, buffer.Length)) > 0) //считываем последовательность байт из текущего потока
						{
							if (Hash)
								Console.Write("#");

							stream.Write(buffer, 0, num);  //записываем последовательность байт в текущий поток
						}
					}
				}
				return getStatusDescription(request); //получаем состояние ответа от сервера
			}
			catch (Exception ex)
			{
				exep = ex.Message;
			}
			return exep;

		}

		public string UploadFileWithUniqueName(string source)    //STOU - загрузка файла на сервер с уникальным именем
		{
			var request = createRequest(WebRequestMethods.Ftp.UploadFileWithUniqueName);  //создаем запрос

			using (var stream = request.GetRequestStream())        //возвращает поток, используемый для выгрузки данных на сервер
			{
				using (var fileStream = System.IO.File.Open(source, FileMode.Open))  //создаем поток для передаваемого файла
				{
					int num;

					byte[] buffer = new byte[bufferSize];

					while ((num = fileStream.Read(buffer, 0, buffer.Length)) > 0)  //считываем последовательность байт из текущего потока
					{
						if (Hash)
							Console.Write("#");

						stream.Write(buffer, 0, num);   //записываем последовательность байт в текущий поток
					}
				}
			}

			using (var response = (FtpWebResponse)request.GetResponse())  
			{
				return Path.GetFileName(response.ResponseUri.ToString());   //возвращает имя файла с расширением
			}
		}

		private FtpWebRequest createRequest(string method)       //создаем запрос
		{
			return createRequest(uri, method);
		}

		private FtpWebRequest createRequest(string uri, string method)  //тут тоже создаем, только с другими параметрами
		{
			string exep;
			try
			{
				var r = (FtpWebRequest)WebRequest.Create(uri);          //инициализируем запрос к серверу
				r.Credentials = new NetworkCredential(userName, password);  //идентифицируем пользователя
				r.Method = method;       //задаем команду, которую отправим на сервер
				r.UseBinary = Binary;   
				r.EnableSsl = EnableSsl;     
				r.UsePassive = Passive;   
				return r;
			}
			catch (Exception ex)    //ловим исключения
			{
				exep = ex.Message;
			}
			return null;
		}

		private string getStatusDescription(FtpWebRequest request)  //строка содержит код состояния и сообщение
		{															//от сервера
			string exep;
			try
			{
				using (var response = (FtpWebResponse)request.GetResponse())       //здесь мы получаем ответ от сервера
				{
					return response.StatusDescription;
				}
			}
			catch (Exception ex)            //здесь мы ловим исключения
			{
				exep = ex.Message;
			}
			return exep;
		}

		private string combine(string path1, string path2)             //здесь две строки объединяются в один путь и меняются слеши
		{
			return Path.Combine(path1, path2).Replace("\\", "/");
		}
	}
}