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
            if (main_Path.Contains("\\bin\\"))
            {
                main_Path = Path.GetFullPath(Path.Combine(main_Path, @"..\..\.."));
            }
           
            main_Path += "\\";            
           
            CscEXE_Path = RuntimeEnvironment.GetRuntimeDirectory() + "csc.exe";
            userClass_Path = Form1.SpliteEndPath(_userClass_sourceName,true);
            userClient_Path = main_Path + "User_client\\User_client\\";
            
            userClass_sourceName = Form1.SpliteEndPath(_userClass_sourceName) + ".cs";
            userClass_dllName = "User_class.dll";

            user_directory_name = "User_" + i;

            userClient_sourceName = "Program.cs";
            userClientexe_Name = "Program.exe";

            ClassLibrary_CGC = "ClassLibrary_CGC.dll";
        }


        //public Compiler(string main_path, string cscExe_path, string source_name)
        //{
        //    main_Path = "D:\\Cloudmail\\Исходники\\C#\\Diplom-CGC";
        //    CscEXE_Path = "C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\csc.exe";
        //    userClass_Path = main_Path + "\\User_class\\User_class";
        //    userClient_Path = main_Path + "\\User_client\\User_client";
        //    userClass_sourceName = source_name + ".cs";
        //    userClientexe_Name = source_name + ".exe";
        //    userClass_dllName = source_name + ".dll";
        //}


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

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat($"/C cd {userClient_Path}{user_directory_name} && " +
                $"{CscEXE_Path} " +
                $"/r:{ClassLibrary_CGC} " +
                $"/target:library " +
                $"/out:{userClass_dllName} {userClass_sourceName}");

            Process.Start("cmd.exe", sb.ToString());
            Thread.Sleep(3000);

            if (!File.Exists(userClient_Path + user_directory_name + "\\" + userClass_dllName))
            {
                throw new Exception("Исходный код стратегии не удалось скомпилировать, возможны ошибки в коде");
            }
        }

        /// <summary>
        /// Скомпилировать exe tcp-клиента пользователя
        /// </summary>
        private void UserClientExeCompile()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($"/C cd {userClient_Path}{user_directory_name} && " +
                $"{CscEXE_Path} /r:{ClassLibrary_CGC};{userClass_dllName} {userClient_sourceName}");
            Process.Start("cmd.exe", sb.ToString());
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
