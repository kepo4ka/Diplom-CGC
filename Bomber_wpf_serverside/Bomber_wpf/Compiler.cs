﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Bomber_wpf
{
    public class Compiler
    {
      public static  string main_Path;
        static string CscEXE_Path = CscEXE_Path = RuntimeEnvironment.GetRuntimeDirectory() + "csc.exe";
        public static string assets_Path;


        public static string LogPath = "log.txt";
        public static string mapsPath;

        string userClass_Path;
       public static string TempGamePath;

        string userClass_sourceName;
        public string userClass_Default_sourceName = "user.cs";
        public static string userClientexe_Name = "Program.exe";
        string userClientLocal_sourceName = "ProgramLocal.cs";

        string userClass_dllName = "User_class.dll";
        string ClassLibrary_CGC = "ClassLibrary_CGC.dll";
        string newtonjson = "Newtonsoft.Json.dll";
        string user_directory_name;
      

        string output;
        string errorput;
            

        public static List<string> compileDirectories = new List<string>();

        public Compiler()
        {
            
            DirectoryInfo main_di = new DirectoryInfo(Directory.GetCurrentDirectory());
            main_Path = main_di.FullName;

            if (main_di.Name == "Debug" || main_di.Name == "Release")
            {
                assets_Path = Path.GetFullPath(Path.Combine(main_Path, @"..\..\..\..") + "\\assets");
            }
            else
            {
                assets_Path = main_Path + "\\assets";
            }

            mapsPath = assets_Path + "\\" + "maps";

            TempGamePath = main_Path + "\\" + "compiler";  
        }

        public Compiler(string _userClass_sourceName, int i)
        {           
            if (_userClass_sourceName == "" || _userClass_sourceName == null)
            {
                throw new Exception("Неверное имя файла исходного кода");
            }
            DirectoryInfo main_di = new DirectoryInfo(Directory.GetCurrentDirectory());
            main_Path = main_di.FullName;

            if (main_di.Name == "Debug" || main_di.Name == "Release")
            {
                assets_Path = Path.GetFullPath(Path.Combine(main_Path, @"..\..\..\..") + "\\assets");
            }
            else
            {
                assets_Path = Path.GetFullPath(main_Path + "\\assets");
            }
            
            userClass_Path = Helper.SpliteEndPath(_userClass_sourceName,true);
            TempGamePath = main_Path + "\\" + "compiler";         
            userClass_sourceName = Helper.SpliteEndPath(_userClass_sourceName) + ".cs";
            mapsPath = assets_Path + "\\" + "maps";

            user_directory_name = "User_" + i;
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


        public static void EndProccess()
        {
            DeleteComppiledFiles();            
        }



   

        /// <summary>
        /// Создать папки для компиляции
        /// </summary>
        void CreateUserDirectory()
        {
            compileDirectories.Add($"{TempGamePath}\\{user_directory_name}");

            Helper.DeleteDirectory($"{TempGamePath}\\{user_directory_name}");

            Directory.CreateDirectory($"{TempGamePath}\\{user_directory_name}");
          
            File.Copy($"{assets_Path}\\{newtonjson}", $"{TempGamePath}\\{user_directory_name}\\{newtonjson}");

            File.Copy($"{assets_Path}\\{ClassLibrary_CGC}", $"{TempGamePath}\\{user_directory_name}\\{ClassLibrary_CGC}");
            File.Copy($"{assets_Path}\\{userClientLocal_sourceName}", $"{TempGamePath}\\{user_directory_name}\\{userClientLocal_sourceName}");



            if (File.Exists($"{userClass_Path}\\{userClass_sourceName}"))
            {
                File.Copy($"{userClass_Path}\\{userClass_sourceName}", $"{TempGamePath}\\{user_directory_name}\\{userClass_Default_sourceName}");
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
            output = "";
            errorput = "";

            string code = $"cd {TempGamePath}\\{user_directory_name} && " +
                $"{CscEXE_Path} " +
                $"/r:{ClassLibrary_CGC} " +
                $"/target:library " +
                $"/out:{userClass_dllName} {userClass_Default_sourceName}";

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
            Helper.startProccess($"cd {TempGamePath}\\{user_directory_name} && " +
               $"{CscEXE_Path} /r:{ClassLibrary_CGC};{userClass_dllName};{newtonjson} /out:{userClientexe_Name} {userClientLocal_sourceName}", out output, out errorput);

            if (errorput!="")
            {               
                throw new Exception($"Ошибка при компиляции exe tcp-клиента: {errorput}");
            }            
        }

 

        /// <summary>
        /// Запустить tcp-клиент пользователя
        /// </summary>
        public void UserClientStart()
        {
            if (!File.Exists($"{TempGamePath}\\{user_directory_name}\\{userClientexe_Name}"))
            {               
                throw new Exception($"Не удалось запустить exe tcp-клиента");
            }

           
            //StringBuilder sb = new StringBuilder();
          
            //sb.AppendFormat($"/C cd {TempGamePath}\\{user_directory_name} && {userClientexe_Name}");

            //Process.Start("cmd.exe", sb.ToString());



          
            Helper.startProccess($"{TempGamePath}\\{user_directory_name}\\{userClientexe_Name}");

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
