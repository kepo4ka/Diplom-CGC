﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;

using Newtonsoft.Json;
using ClassLibrary_CGC;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;

namespace Bomber_wpf
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
            if (actionInt >= 0 && actionInt < 6)
            {
                pa = (PlayerAction)actionInt;
            }
            else
            {
                pa = PlayerAction.Wait;
            }
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
            string serverMessage = Encoding.ASCII.GetString(serverData, 0, bytes);
            return serverMessage;
        }

        /// <summary>
        /// Отправить строку в сетевой поток
        /// </summary>
        /// <param name="message">Отправляемая строка</param>
        public static void writeStream(NetworkStream strm, string message)
        {
            Byte[] data = Encoding.ASCII.GetBytes(message);
            strm.Write(data, 0, data.Length);
        }


        /// <summary>
        /// Добавить информацию в лог на форме
        /// </summary>
        /// <param name="message"></param>
        public static void LOG(string filename, string message)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filename, true))
                {
                    string time = DateTime.Now.ToString("dd-MM-yyyy H-mm-ss");
                    time = "[" + time + "] ";
                    sw.WriteLine($"{time}: {message} \n");
                }
            }
            catch (Exception IO)
            {
                return;
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
            // Получение текста в виде кодировки 866 win
            procStartInfo.StandardOutputEncoding = Encoding.GetEncoding(866);
            procStartInfo.StandardErrorEncoding = Encoding.GetEncoding(866);
            // не создавать окно CMD
            procStartInfo.CreateNoWindow = false;

            Process proc = new Process();


            //запуск CMD
            proc.StartInfo = procStartInfo;
            proc.Start();
            //чтение результата
            output = "";
            errorput = proc.StandardError.ReadToEnd();

            while (proc.StandardOutput.Peek() >= 0)
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
        /// Запустить процесс со специфичными настройками
        /// </summary>
        /// <param name="stroke">Команда и её параметры</param>
        /// <returns>Результат выполнения процесса</returns>
        public static void startProccess(string stroke)
        {
            ProcessStartInfo procStartInfo =
                new ProcessStartInfo(
                    "cmd", "/c " + stroke);

            procStartInfo.UseShellExecute = false;
            // не создавать окно CMD
            procStartInfo.CreateNoWindow = false;

            Process proc = new Process();
            // Получение текста в виде кодировки 866 win
            //  procStartInfo.StandardOutputEncoding = Encoding.GetEncoding();
            //запуск CMD
            proc.StartInfo = procStartInfo;
            proc.Start();


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

                    foreach (DirectoryInfo df in diA)
                    {
                        DeleteDirectory(df.FullName);
                        if (df.GetDirectories().Length == 0 && df.GetFiles().Length == 0)
                        {
                            df.Delete();
                        }
                    }
                    di.Delete();
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

        public static string Zip(string value)
        {
            //Transform string into byte[]
            byte[] byteArray = new byte[value.Length];
            int indexBA = 0;
            char[] charArray = value.ToCharArray();
            foreach (char item in charArray)
            {
                byteArray[indexBA++] = (byte)item;
            }

            //Prepare for compress
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.IO.Compression.GZipStream sw = new System.IO.Compression.GZipStream(ms,
                System.IO.Compression.CompressionMode.Compress);

            //Compress
            sw.Write(byteArray, 0, byteArray.Length);
            //Close, DO NOT FLUSH cause bytes will go missing...
            sw.Close();

            //Transform byte[] zip data to string
            byteArray = ms.ToArray();
            System.Text.StringBuilder sB = new System.Text.StringBuilder(byteArray.Length);
            foreach (byte item in byteArray)
            {
                sB.Append((char)item);
            }
            ms.Close();
            sw.Dispose();
            ms.Dispose();
            return sB.ToString();
        }



        public static string UnZip(string value)
        {
            //Transform string into byte[]
            byte[] byteArray = new byte[value.Length];
            int indexBA = 0;
            foreach (char item in value.ToCharArray())
            {
                byteArray[indexBA++] = (byte)item;
            }

            //Prepare for decompress
            System.IO.MemoryStream ms = new System.IO.MemoryStream(byteArray);
            System.IO.Compression.GZipStream sr = new System.IO.Compression.GZipStream(ms,
                System.IO.Compression.CompressionMode.Decompress);

            //Reset variable to collect uncompressed result
            byteArray = new byte[byteArray.Length];

            //Decompress
            int rByte = sr.Read(byteArray, 0, byteArray.Length);

            //Transform byte[] unzip data to string
            System.Text.StringBuilder sB = new System.Text.StringBuilder(rByte);
            //Read the number of bytes GZipStream red and do not a for each bytes in
            //resultByteArray;
            for (int i = 0; i < rByte; i++)
            {
                sB.Append((char)byteArray[i]);
            }
            sr.Close();
            ms.Close();
            sr.Dispose();
            ms.Dispose();
            return sB.ToString();
        }

        /// <summary>
        /// Compresses the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string CompressString(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return Convert.ToBase64String(gZipBuffer);
        }


        /// <summary>
        /// Decompresses the string.
        /// </summary>
        /// <param name="compressedText">The compressed text.</param>
        /// <returns></returns>
        public static string DecompressString(string compressedText)
        {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }



        public static int[,] LoadMap(string MapPath)
        {
            int[,] gameboardpseudo = null;
            try
            {
                if (String.IsNullOrWhiteSpace(MapPath))
                {
                    throw new Exception("Неверное имя файла");
                }

                gameboardpseudo = Helper.GetGameboardFromFile(MapPath);
                return gameboardpseudo;
            }
            catch (Exception er)
            {
                Helper.LOG(Compiler.LogPath, $"Не удалось загрузить Пользовальскую карту: {er.Message}");
            }

            try
            {
                DirectoryInfo di = new DirectoryInfo(Compiler.mapsPath);
                var files = di.GetFiles("*.txt");
                if (files.Length < 1)
                {
                    throw new Exception($"Не удалось найти ни одной карты в папке {Compiler.mapsPath}");
                }
                MapPath = files[rn.Next(0, files.Length)].FullName;


                gameboardpseudo = Helper.GetGameboardFromFile(MapPath);
                return gameboardpseudo;
            }
            catch (Exception er)
            {
                Helper.LOG(Compiler.LogPath, $"Не удалось загрузить карту из стандартных карт: {er.Message}");
                return null;
            }
        }


        public static int[,] GetGameboardFromFile(string psource)
        {
            int[,] gameboardpseudo = new int[15, 15];
            string[] splitedFile = psource.Split('.');
            string[] splitPath = psource.Split('\\');
            string fileExtension = splitedFile[splitedFile.Length - 1];
            string filePath = splitPath[splitPath.Length - 1];

            //  MessageBox.Show("psource " + psource);

            using (StreamReader sr = new StreamReader(psource))
            {
                for (int i = 0; i < gameboardpseudo.GetLength(0); i++)
                {
                    string line = "";
                    if ((line = sr.ReadLine().Trim()) != "")
                    {
                        string[] linesplit = line.Split();

                        if (linesplit.Length != gameboardpseudo.GetLength(1))
                        {
                            throw new Exception("Ошибка при парсинге карты: неверное количество столбцов");
                        }

                        for (int j = 0; j < linesplit.Length; j++)
                        {
                            int t = 0;
                            if (!int.TryParse(linesplit[j], out t))
                            {
                                throw new Exception($"Ошибка при парсинге карты: нечисловое значение [{i},{j}");
                            }
                            gameboardpseudo[i, j] = t;
                        }
                    }
                }
            }



            //gameboardpseudo[0, 0] = gameboardpseudo[0, 0] == 5 ? 5 : 0;
            //gameboardpseudo[0, gameboardpseudo.GetLength(1) - 1] = gameboardpseudo[0, gameboardpseudo.GetLength(1) - 1] == 5 ? 5 : 0;
            //gameboardpseudo[gameboardpseudo.GetLength(1) - 1, 0] = gameboardpseudo[gameboardpseudo.GetLength(1) - 1, 0] == 5 ? 5 : 0;
            //gameboardpseudo[gameboardpseudo.GetLength(0) - 1, gameboardpseudo.GetLength(1) - 1] = gameboardpseudo[gameboardpseudo.GetLength(0) - 1, gameboardpseudo.GetLength(1) - 1] == 5 ? 5 : 0;

            return gameboardpseudo;

        }







        public static string ActionToSymbol(PlayerAction pa)
        {
            string symbol = "";
            switch (pa)
            {
                case PlayerAction.Wait:
                    symbol = "W";
                    break;
                case PlayerAction.Bomb:
                    symbol = "B";
                    break;
                case PlayerAction.Left:
                    symbol = "L";
                    break;
                case PlayerAction.Up:
                    symbol = "U";
                    break;
                case PlayerAction.Right:
                    symbol = "R";
                    break;
                case PlayerAction.Down:
                    symbol = "D";
                    break;
            }
            return symbol;
        }



        public static string[] GetLastVersion()
        {
            string tag = "";
            string [] content;

            string site = "http://195.133.48.168";
            site = "http://195.133.48.168/core/getgithubuserfileslast.php?JOPA=mercyme";
                // Создаём объект WebClient
                using (var webClient = new WebClient())
                {
                    // Выполняем запрос по адресу и получаем ответ в виде строки

                    var response = webClient.DownloadString(site);
                    
                    content = response.Trim().Split();
                    if (content.Length!=2)
                    {
                        throw new Exception();
                    }
                }   
            return content;
        }





    }
}
