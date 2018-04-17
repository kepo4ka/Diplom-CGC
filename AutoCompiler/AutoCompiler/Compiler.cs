using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace AutoCompiler
{
    public class Compiler
    {
        static string main_Path;
        static string assets_Path;
        static string CscEXE_Path;
        public static string HostUserPath;
        public static string LogPath;
      

        string userClass_Path;
        string userClient_Path;
        

        string userClass_sourceName;
        string userClass_dllName;
        string ClassLibrary_CGC;
        string newtonjson;      
        string userClient_sourceName;
       public static string userClientexe_Name;
        string output;
        string errorput;
        public string containerName;


        public static List<string> compileDirectories = new List<string>();


        public Compiler(string _userClass_sourceName)
        {
            if (String.IsNullOrWhiteSpace(_userClass_sourceName))
            {
                throw new Exception("Неверное имя файла исходного кода");
            }
           

            main_Path = Directory.GetCurrentDirectory();          
            LogPath = $"{HostUserPath}\\log.txt";

            //if (main_Path.Contains("\\bin\\"))
            //{
            //    main_Path = Path.GetFullPath(Path.Combine(main_Path, @"..\..\.."));
            //}
            assets_Path = Path.GetFullPath(Path.Combine(main_Path, @"..\..\..\.."));
            assets_Path += "\\assets";


            CscEXE_Path = RuntimeEnvironment.GetRuntimeDirectory() + "csc.exe";
            userClass_Path = Helper.SpliteEndPath(_userClass_sourceName, true);
            userClient_Path = main_Path + "\\compiler";

            userClass_sourceName = Helper.SpliteEndPath(_userClass_sourceName) + ".cs";
            userClass_dllName = "User_class.dll";           

            userClient_sourceName = "Program.cs";
            userClientexe_Name = "Program.exe";

            ClassLibrary_CGC = "ClassLibrary_CGC.dll";
            newtonjson = "Newtonsoft.Json.dll";
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
                throw new Exception(e.Message);
            }
        }



        public void BackFile(string to)
        {
            Helper.DeleteFile($"{to}\\{userClientexe_Name}");
            Helper.FileMove($"{userClient_Path}\\{userClientexe_Name}", $"{to}\\{userClientexe_Name}");
        }



        public static void EndProccess()
        {
            try
            {

                DeleteComppiledFiles();
            }
            catch
            {
                Console.WriteLine("DeleteComppiledFiles Error");
            }          
        }




        /// <summary>
        /// Создать папки для компиляции
        /// </summary>
        void CreateUserDirectory()
        {
            compileDirectories.Add(userClient_Path);

            foreach (FileInfo tfile in Directory.CreateDirectory(userClient_Path).GetFiles())
            {
                tfile.Delete();
            }
            File.Copy($"{assets_Path}\\{newtonjson}", $"{userClient_Path}\\{newtonjson}");

            File.Copy($"{assets_Path}\\{ClassLibrary_CGC}", $"{userClient_Path}\\{ClassLibrary_CGC}");
            File.Copy($"{assets_Path}\\{userClient_sourceName}", $"{userClient_Path}\\{userClient_sourceName}");

            if (File.Exists($"{userClass_Path}{userClass_sourceName}"))
            {
                File.Copy($"{userClass_Path}{userClass_sourceName}", $"{userClient_Path}\\{userClass_sourceName}");
            }
            else
            {
                throw new Exception("Не удалось скопировать файл исходного кода стратегии");
            }
        }

        /// <summary>
        /// Компиляция пользовательского класса в dll и перещение его в папку программы Tcp-клиента
        /// </summary>
        void UserClassDLLCompile()
        {
            Helper.DeleteFile(userClass_Path + userClass_dllName);
            output = "";
            errorput = "";

            string code = $"cd {userClient_Path} && " +
                $"{CscEXE_Path} " +
                $"/r:{ClassLibrary_CGC} " +
                $"/target:library " +
                $"/out:{userClass_dllName} {userClass_sourceName}";

            Helper.startProccess(code, out output, out errorput);

            if (errorput != "")
            {
                throw new Exception($"Ошибка при компиляции пользовательской стратегии в DLL: {errorput}");
            }
        }

        /// <summary>
        /// Скомпилировать exe tcp-клиента пользователя
        /// </summary>
        void UserClientExeCompile()
        {
            output = "";
            errorput = "";
            Helper.startProccess($"cd {userClient_Path} && " +
               $"{CscEXE_Path} /r:{ClassLibrary_CGC};{userClass_dllName};{newtonjson} {userClient_sourceName}", out output, out errorput);

            if (errorput != "")
            {
                throw new Exception($"Ошибка при компиляции exe tcp-клиента: {errorput}");
            }
        }    

        /// <summary>
        /// Удалить папки, содержащие скомпилированные коды стратегий пользователей
        /// </summary>
        static void DeleteComppiledFiles()
        {
            for (int i = 0; i < compileDirectories.Count; i++)
            {
                Helper.DeleteDirectory(compileDirectories[i]);
            }
            compileDirectories.Clear();
        }
    }
}
