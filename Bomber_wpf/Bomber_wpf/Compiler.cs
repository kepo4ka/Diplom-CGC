using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Threading;

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

        string userClient_sourceName;
        string userClientexe_Name;

        public Compiler()
        {
            main_Path = "D:\\Cloudmail\\Исходники\\C#\\Diplom-CGC\\";
            CscEXE_Path = "C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\csc.exe";
            userClass_Path = main_Path + "User_class\\User_class\\";
            userClient_Path = main_Path + "User_client\\User_client\\";

            userClass_sourceName = "User.cs";
            userClass_dllName = "User_class.dll";

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

        public void ComplineAndStart()
        {
            try
            {
                DeleteFile("compile_log.txt");       

                UserClassDLLCompile();
                UserClientExeCompile();               
            }
            catch (Exception e)
            {
                MessageBox.Show($"Ошибка при компиляции: {e.Message}");
            }
        }

        public void UserClientStart()
        {
            StringBuilder sb = new StringBuilder();
            if (!File.Exists(userClient_Path+userClientexe_Name))
            {
                throw new Exception("Не найден файл exe, запускающий tcp клиент");
            }
            sb.AppendFormat($"/K cd {userClient_Path} && {userClientexe_Name}");
           
            Process.Start("cmd.exe", sb.ToString());           
        }



        /// <summary>
        /// Компиляция пользовательского класса в dll и перещение его в папку программы Tcp-клиента
        /// </summary>
        private void UserClassDLLCompile()
        {
            DeleteFile(userClass_Path + userClass_dllName);
            DeleteFile(userClient_Path + userClass_dllName);

            if (!File.Exists(userClass_Path + userClass_sourceName))
            {
                throw new Exception("Файл исходного кода класса Юзера не найден");
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat($"/C cd {userClass_Path} && " +
                $"{CscEXE_Path} /r:{ClassLibrary_CGC} /target:library /out:{userClass_dllName} {userClass_Path}{userClass_sourceName}");
            Process.Start("cmd.exe", sb.ToString());
            Thread.Sleep(1000);
            if (File.Exists(userClass_Path + userClass_dllName))
            {
                File.Move(userClass_Path + userClass_dllName, userClient_Path + userClass_dllName);
            }
            else
            {
                throw new Exception("Библиотека с кодом юзера не была скомпилирована");
            }
        }


        private void UserClientExeCompile()
        {
            DeleteFile(userClass_Path + userClientexe_Name);
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($"/C cd {userClient_Path} && " +
                $"{CscEXE_Path} /r:{ClassLibrary_CGC};{userClass_dllName} {userClient_sourceName}");
            Process.Start("cmd.exe", sb.ToString());
        }



        private void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }



    }
}
