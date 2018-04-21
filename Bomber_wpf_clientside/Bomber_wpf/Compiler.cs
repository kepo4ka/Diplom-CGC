using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

namespace Bomber_wpf
{
    public class Compiler
    {
        string main_Path;
        string CscEXE_Path;

        string userClass_Path;
        string userClient_Path;

        string userClass_sourceName;        
        string userClass_dllName;
        string ClassLibrary_CGC;
        string user_directory_name;
        string userClient_sourceName;
        string userClientexe_Name;

        public static List<string> compileDirectories = new List<string>();

        public Compiler(string _userClass_sourceName, int i)
        {           
            if (_userClass_sourceName == "" || _userClass_sourceName == null)
            {
                throw new Exception("Неверное имя файла исходного кода");
            }

            main_Path = Directory.GetCurrentDirectory();
            //if (main_Path.Contains("\\bin\\"))
            //{
            //    main_Path = Path.GetFullPath(Path.Combine(main_Path, @"..\..\.."));
            //}
           
            main_Path += "\\";            
           
            CscEXE_Path = RuntimeEnvironment.GetRuntimeDirectory() + "csc.exe";
            userClass_Path = Form1.SpliteEndPath(_userClass_sourceName,true);
            userClient_Path = main_Path;
            
            userClass_sourceName = Form1.SpliteEndPath(_userClass_sourceName) + ".cs";
            userClass_dllName = "User_class.dll";

            user_directory_name = "User_" + i;

            userClient_sourceName = "Program.cs";
            userClientexe_Name = "Program.exe";

            ClassLibrary_CGC = "ClassLibrary_CGC.dll";
        }



        /// <summary>
        /// Запустить процесс со специфичными настройками
        /// </summary>
        /// <param name="stroke">Команда и её параметры</param>
        /// <returns>Результат выполнения процесса</returns>
        public static void startProccess(string stroke, out string output, out string errorlog)
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
            procStartInfo.StandardErrorEncoding = Encoding.GetEncoding(866);
            //запуск CMD
            proc.StartInfo = procStartInfo;
            proc.Start();
            //чтение результата
            output = proc.StandardOutput.ReadToEnd();
            errorlog = proc.StandardError.ReadToEnd();
        }


        /// <summary>
        /// Скомпилировать файлы, необходимые для tcp-клиента пользователя
        /// </summary>
        public void Compile()
        {
            try
            {
                CreateUserDirectory();
                UserClassDLLCompile();
                UserClientExeCompile();
            }
            catch (Exception e)
            {
                throw new Exception("compile_error: " + e.Message);
            }
        }

        public void CreateUserDirectory()
        {
            compileDirectories.Add(userClient_Path + user_directory_name);

            foreach(FileInfo tfile in Directory.CreateDirectory(userClient_Path + user_directory_name).GetFiles())
            {
                tfile.Delete();
            }

            File.Copy(userClient_Path + ClassLibrary_CGC, userClient_Path + user_directory_name+"\\" + ClassLibrary_CGC);
            File.Copy(userClient_Path + userClient_sourceName, userClient_Path + user_directory_name +"\\" + userClient_sourceName);

            if (File.Exists(userClass_Path + userClass_sourceName))
            {
                File.Copy(userClass_Path + userClass_sourceName, userClient_Path + user_directory_name + "\\" + userClass_sourceName);
            }
            else
            {
                throw new Exception("Не удалось скопировать файл исходного кода стратегии");
            }
        }



        /// <summary>
        /// Компиляция пользовательского класса в dll и перещение его в папку программы Tcp-клиента
        /// </summary>
        private void UserClassDLLCompile()
        {
            DeleteFile(userClass_Path + userClass_dllName);
            //  DeleteFile(userClient_Path + userClass_dllName);       
            string output = "";
            string error = "";

            startProccess($"cd {userClient_Path}{user_directory_name} && " +
                $"{CscEXE_Path} " +
                $"/r:{ClassLibrary_CGC} " +
                $"/target:library " +
                $"/out:{userClass_dllName} {userClass_sourceName}", out output, out error);

            if (error.Length > 1)
            {
                // Form1.LogUpdate(error);
                throw new Exception("Ошибка, не удалось скомпилировать пользовательский код");                
            }
           
        }

        /// <summary>
        /// Скомпилировать exe tcp-клиента пользователя
        /// </summary>
        private void UserClientExeCompile()
        {
            string output = "";
            string error = "";
            startProccess($"cd {userClient_Path}{user_directory_name} && " +
                $"{CscEXE_Path} /r:{ClassLibrary_CGC};{userClass_dllName} {userClient_sourceName}", out output, out error);

            if (error != "")
            {
                throw new Exception("Ошибка, не удалось скомпилировать tcp-client");
            }
        }


        /// <summary>
        /// Запустить tcp-клиент пользователя
        /// </summary>
        public void UserClientStart()
        {
            StringBuilder sb = new StringBuilder();
            if (!File.Exists(userClient_Path + user_directory_name + "\\" + userClientexe_Name))
            {
                throw new Exception("Не найден файл exe, запускающий tcp клиент");
            }
            sb.AppendFormat($"/C cd {userClient_Path}{user_directory_name} && {userClientexe_Name}");

            Process.Start("cmd.exe", sb.ToString());
        }


        /// <summary>
        /// Удалить папки, содержащие скомпилированные коды стратегий пользователей
        /// </summary>
        public static void DeleteComppiledFiles()
        {
            for (int i = 0; i < compileDirectories.Count; i++)
            {
                DeleteDirectory(compileDirectories[i]);
            }
            compileDirectories.Clear();
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
            if (Directory.Exists(DirectoryPath))
            {
                DirectoryInfo di = new DirectoryInfo(DirectoryPath);
                foreach (FileInfo tfile in di.GetFiles())
                {
                    tfile.Delete();
                }
               
                Directory.Delete(DirectoryPath);
            }
        }
    }
}
