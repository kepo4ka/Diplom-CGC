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

namespace AutoCompiler
{
    class Helper
    {

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
        /// Получить полный путь до папки, в которой находится файл
        /// </summary>
        /// <param name="filePath">Файл</param>
        /// <returns>Папка</returns>
        public static string GetFileDirectory(string filePath)
        {
            string[] temp = filePath.Split('\\');

            string directory = filePath.Replace($"\\{temp[temp.Length - 1]}", "");
            return directory;
        }



        /// <summary>
        /// Добавить информацию в лог на форме
        /// </summary>
        /// <param name="message"></param>
        public static void LOG(string message, string filename = "log.txt")
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
                    if (errorput=="")
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
                Helper.LOG("errors.txt", "DeleteDirectory ERROR: " + e.Message);
            }
        } 
        public static void FileMove(string from, string to)
        {           
                File.Move(from, to); 
        }  
    }
}
