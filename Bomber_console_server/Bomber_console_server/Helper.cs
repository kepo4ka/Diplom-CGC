using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;

using ClassLibrary_CGC;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace Bomber_console_server
{
    class Helper
    {

       public static Random rn = new Random();

        /// <summary>
        /// Распознать Команду из строки
        /// </summary>
        /// <param name="message">Данная строка</param>
        /// <param name="usersInfo">Список, в элемент которого необходимо передать команду</param>
        /// <param name="i">Индекс элемента в Списке, который получает команду</param>
        public static PlayerAction DecryptAction(string message)
        {
            PlayerAction pa = new PlayerAction();
            int actionInt = int.Parse(message);
            pa = (PlayerAction)actionInt;
            return pa;     
        }

        /// <summary>
        /// Распознать Команду из строки
        /// </summary>
        /// <param name="message">Данная строка</param>
        /// <param name="usersInfo">Список, в элемент которого необходимо передать команду</param>
        /// <param name="i">Индекс элемента в Списке, который получает команду</param>
        public static string EncryptAction(PlayerAction action)
        {
            int actionInt = (int)action;
            string actionString = actionInt.ToString();
            return actionString;
        }


        /// <summary>
        /// Случайный md5 хеш
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CalculateMD5Hash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }
        

        /// <summary>
        /// Выделить из Пути файла имя этого Файла
        /// </summary>
        /// <param name="ppath">Полный путь до файла</param>
        /// <returns>Имя файла</returns>
        public static string SpliteEndPath(string ppath, bool k = false)
        {
            Stack<char> tsymbols = new Stack<char>();
            string nfileName = "";

            int tindex = ppath.Length - 1;

            for (; tindex >= 0; tindex--)
            {
                if (ppath[tindex] == '.')
                {
                    break;
                }
            }
            tindex--;

            for (; tindex >= 0; tindex--)
            {

                if (ppath[tindex] == '\\')
                {
                    break;
                }
                tsymbols.Push(ppath[tindex]);
            }

            while (tsymbols.Count > 0)
            {
                nfileName += tsymbols.Pop();
            }

            if (k)
            {
                nfileName = ppath.Substring(0, ppath.IndexOf(nfileName + ".cs"));
            }
            return nfileName;
        }


        /// <summary>
        /// Cчитать строку из сетевого потока
        /// </summary>
        /// <returns></returns>
        public static string readStream(NetworkStream strm)
        {
            Byte[] serverData = new Byte[16];
            int bytes = strm.Read(serverData, 0, serverData.Length);
            string serverMessage = Encoding.Unicode.GetString(serverData, 0, bytes);
            return serverMessage;
        }

        /// <summary>
        /// Отправить строку в сетевой поток
        /// </summary>
        /// <param name="message">Отправляемая строка</param>
        public static void writeStream(NetworkStream strm, string message)
        {
            Byte[] data = Encoding.Unicode.GetBytes(message);
            strm.Write(data, 0, data.Length);
        }


        /// <summary>
        /// Добавить информацию в лог на форме
        /// </summary>
        /// <param name="message"></param>
        public static void LOG(string filename, string message)
        {
            using (StreamWriter sw = new StreamWriter(filename, true))
            {
                string time = DateTime.Now.ToString("dd-MM-yyyy H-mm-ss");
                time = "[" + time + "] ";
                sw.WriteLine($"{time}: {message}");
                Console.WriteLine($"{time}: {message}");
            }
        }

        /// <summary>
        /// Запустить процесс со специфичными настройками
        /// </summary>
        /// <param name="stroke">Команда и её параметры</param>
        /// <returns>Результат выполнения процесса</returns>
        public static void startProccess(string stroke, out string output, out string errorput)
        {
            ProcessStartInfo procStartInfo =
                new ProcessStartInfo(
                    "cmd", "/c " + stroke);
            // Следующая команды означает, что нужно перенаправить стандартынй вывод
            // на Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.RedirectStandardError = true;
            procStartInfo.UseShellExecute = false;
            // не создавать окно CMD
            procStartInfo.CreateNoWindow = true;

            Process proc = new Process();
            // Получение текста в виде кодировки 866 win
            procStartInfo.StandardOutputEncoding = Encoding.GetEncoding(866);
            //запуск CMD
            proc.StartInfo = procStartInfo;
            proc.Start();
            //чтение результата
            output = "";
            errorput = proc.StandardError.ReadToEnd();

          while (proc.StandardOutput.Peek()>=0)
            {
                output = proc.StandardOutput.ReadLine();
                if (output.Contains("error"))
                {
                    if (errorput == "")
                    {
                        errorput += Environment.NewLine;
                    }
                    errorput += output + Environment.NewLine + proc.StandardOutput.ReadToEnd();
                    break;
                }
            }
        }


        /// <summary>
        /// Удалить файл, если он существует
        /// </summary>
        /// <param name="filePath">Путь к удаляемому файлу</param>
        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }


        /// <summary>
        ///  Удалить папку, если она существует
        /// </summary>
        /// <param name="DirectoryPath"></param>
        public static void DeleteDirectory(string DirectoryPath)
        {
            try
            {
                if (Directory.Exists(DirectoryPath))
                {
                    DirectoryInfo di = new DirectoryInfo(DirectoryPath);
                    DirectoryInfo[] diA = di.GetDirectories();
                    FileInfo[] fi = di.GetFiles();

                    foreach (FileInfo f in fi)
                    {
                        f.Delete();
                    }

                    foreach (FileInfo tfile in di.GetFiles())
                    {
                        tfile.Delete();
                    }

                    foreach (DirectoryInfo df in diA)
                    {
                        DeleteDirectory(df.FullName);
                        if (df.GetDirectories().Length == 0 && df.GetFiles().Length == 0)
                        {
                            df.Delete();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Helper.LOG(Compiler.LogPath, "DeleteDirectory ERROR: " + e.Message);
            }
        }       


        /// <summary>
        /// Создать, либо пересоздать пустую папку
        /// </summary>
        /// <param name="ppath"></param>
        public static void CreateEmptyDirectory(string ppath)
        {
            DeleteDirectory(ppath);
            Directory.CreateDirectory(ppath);
        }

        /// <summary>
        /// Записать строку в файл
        /// </summary>
        /// <param name="data">Строка</param>
        /// <param name="path">Полный путь до файла, включая имя файла и расширение</param>
        /// <param name="k">Если true, то данные добавляются в конец файла, не перезаписывая файл</param>
        public static void WriteDataJson(string data, string path, bool k)
        {
            if (data != null)
            {
                using (StreamWriter sw = new StreamWriter(path, k))
                {
                    sw.AutoFlush = true;
                    sw.WriteLine(data);
                }
            }
        }

      
        /// <summary>
        /// Считать данные из файла
        /// </summary>
        /// <param name="path">Полный путь до файла, включая имя файла и расширение</param>
        /// <returns>Полученные данные</returns>
        public static string[] ReadFile(string path)
        {
            string[] data = new string[3];

            if (!File.Exists(path))
            {
                return null;
            }

            using (StreamReader sr = new StreamReader(path))
            {
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = sr.ReadLine();
                    if (String.IsNullOrWhiteSpace(data[i]))
                    {
                        return null;
                    }
                }
            }              
            return data;
        }


        public static void Compress(string sourceFile, string compressedFile)
        {
            // поток для чтения исходного файла
            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate))
            {
                // поток для записи сжатого файла
                using (FileStream targetStream = File.Create(compressedFile))
                {
                    // поток архивации
                    using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                    {
                        sourceStream.CopyTo(compressionStream); // копируем байты из одного потока в другой
                        Console.WriteLine("Сжатие файла {0} завершено. Исходный размер: {1}  сжатый размер: {2}.",
                            sourceFile, sourceStream.Length.ToString(), targetStream.Length.ToString());
                    }
                }
            }
        }

        public static void Decompress(string compressedFile, string targetFile)
        {
            // поток для чтения из сжатого файла
            using (FileStream sourceStream = new FileStream(compressedFile, FileMode.OpenOrCreate))
            {
                // поток для записи восстановленного файла
                using (FileStream targetStream = File.Create(targetFile))
                {
                    // поток разархивации
                    using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(targetStream);
                        Console.WriteLine("Восстановлен файл: {0}", targetFile);
                    }
                }
            }
        }
    }
}
