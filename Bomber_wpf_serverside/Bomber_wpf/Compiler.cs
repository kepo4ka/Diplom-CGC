using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using ClassLibrary_CGC;
using User_class;
using Newtonsoft.Json;

namespace Bomber_wpf
{
    public class Compiler
    {
      static  string main_Path;
       static string CscEXE_Path;
       static string HostUserPath;
        static string gameboardjsonpath = "gameboard.json";
        static string actionfile = "action.txt";
        static string userjsonpath = "user.json";

        string userClass_Path;
        string userClient_Path;

        string userClass_sourceName;        
        string userClass_dllName;
        string ClassLibrary_CGC;
        string newtonjson;
        string user_directory_name;
        string userClient_sourceName;
        string userClientexe_Name;
        string output;
        string errorput;
        public string containerName;    
            

        public static List<string> compileDirectories = new List<string>();


        public Compiler(string _userClass_sourceName, int i)
        {           
            if (_userClass_sourceName == "" || _userClass_sourceName == null)
            {
                throw new Exception("Неверное имя файла исходного кода");
            }           

            main_Path = Directory.GetCurrentDirectory();
            HostUserPath = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            HostUserPath += $"\\docker_temp\\{Form1.gameID}";

            if (main_Path.Contains("\\bin\\"))
            {
                main_Path = Path.GetFullPath(Path.Combine(main_Path, @"..\..\.."));
            }         
  
            CscEXE_Path = RuntimeEnvironment.GetRuntimeDirectory() + "csc.exe";
            userClass_Path = Helper.SpliteEndPath(_userClass_sourceName,true);
            userClient_Path = main_Path + "\\" + "User_client\\User_client\\";
            
            userClass_sourceName = Helper.SpliteEndPath(_userClass_sourceName) + ".cs";
            userClass_dllName = "User_class.dll";

            user_directory_name = "User_" + i;

            userClient_sourceName = "Program.cs";
            userClientexe_Name = "Program.exe";

            ClassLibrary_CGC = "ClassLibrary_CGC.dll";
            newtonjson = "Newtonsoft.Json.dll";
            containerName = Helper.CalculateMD5Hash(DateTime.Now.Millisecond * Helper.rn.NextDouble() + "JOPA");           
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
                throw new Exception(e.Message);
            }
        }


        /// <summary>
        /// Работа с Docker
        /// </summary>
        public void StartProccess()
        {
            DockerPreparation();
            DockerStart();
            Docker.Exec(containerName, "mono /cgc/Program.exe");
        }


        static void DeleteDockerTempDirectory()
        {
            Helper.DeleteDirectory(HostUserPath);
        }


        public static void EndProccess()
        {
            DeleteComppiledFiles();
            DeleteDockerTempDirectory();
        }


        public void StopContainer()
        {
            Docker.StopContainer(containerName);
        }


        /// <summary>
        /// Подготовка к запуску Docker контейнера
        /// </summary>
        void DockerPreparation()
        {
           // CreateDirectoryInAppData();
            CreateDockerTempDirectory(containerName);
            CopyDependenciesToTempDirectory();           
        }


        /// <summary>
        /// Запустить Docker контейнер
        /// </summary>
        void DockerStart()
        {
            string hostPath = $"{HostUserPath}\\{containerName}";

            hostPath = Docker.VolumeFormat(hostPath);

            Docker.Run("kepo4ka/ubuntu_mono", hostPath, containerName);
        }      





        /// <summary>
        /// Создать временную папку, необходимую для Docker
        /// </summary>
        public static void CreateDirectoryInAppData()
        {
            Helper.CreateEmptyDirectory(HostUserPath);
        }

        /// <summary>
        /// Создать папку tcp-клиента внутри временной папки Docker
        /// </summary>
        /// <param name="dirName">Имя создаваемой папки</param>
        void CreateDockerTempDirectory(string dirName)
        {
            Helper.CreateEmptyDirectory($"{HostUserPath}\\{dirName}");          
        }

        /// <summary>
        /// Скопировать необходимые файлы в папку tcp-клиента
        /// </summary>
        void CopyDependenciesToTempDirectory()
        {
            File.Copy($"{userClient_Path}{user_directory_name}\\{ClassLibrary_CGC}",$"{HostUserPath}\\{containerName}\\{ClassLibrary_CGC}");
            File.Copy($"{userClient_Path}{user_directory_name}\\{userClass_dllName}", $"{HostUserPath}\\{containerName}\\{userClass_dllName}");
            File.Copy($"{userClient_Path}{user_directory_name}\\{userClientexe_Name}", $"{HostUserPath}\\{containerName}\\{userClientexe_Name}");
            
            File.Copy($"{userClient_Path}{user_directory_name}\\{newtonjson}", $"{HostUserPath}\\{containerName}\\{newtonjson}");

        }

        /// <summary>
        /// Создать папки для компиляции
        /// </summary>
        void CreateUserDirectory()
        {
            compileDirectories.Add(userClient_Path + user_directory_name);

            foreach(FileInfo tfile in Directory.CreateDirectory(userClient_Path + user_directory_name).GetFiles())
            {
                tfile.Delete();
            }
            File.Copy(userClient_Path + newtonjson, userClient_Path + user_directory_name + "\\" + newtonjson);

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
        /// Записать инфjрмацию о GameBoard в файл, внутри временной папки польвателя
        /// </summary>
        /// <param name="data">Слепок Gamboard, сериализованный в json</param>
        public void SaveTempGameInfo(string gameboardinfo, string userinfo,  bool k=false)
        {
            
            string gbpath = $"{userClient_Path}{user_directory_name}\\{gameboardjsonpath}";
            string uspath = $"{userClient_Path}{user_directory_name}\\{userjsonpath}";

            try
            {
                Helper.WriteDataJson(gameboardinfo, gbpath, k);
                Helper.WriteDataJson(userinfo, uspath, k);
            }
            catch (IOException)
            {
                Thread.Sleep(100);
                SaveTempGameInfo(gameboardinfo, userinfo);
            }
        }

        
        /// <summary>
        /// Получить информацию от пользователя
        /// </summary>
        /// <returns>Информация от клиента</returns>
        public string[] ReadClientDataFile()
        {           
            string path = $"{userClient_Path}{user_directory_name}\\{actionfile}";
            try
            {
                return Helper.ReadFile(path); 
            }
            catch (IOException)
            {
                Thread.Sleep(100);
                return ReadClientDataFile();
            }
            
        }

        /// <summary>
        /// Проверить готов ли клиент к началу игры
        /// </summary>
        /// <returns></returns>
        public void WaitClientStart()
        {
            string file = "log.txt";
            string path = $"{userClient_Path}{user_directory_name}\\{file}";

            if (!File.Exists(file))
            {
                Thread.Sleep(100);
                WaitClientStart();
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

            string code = $"cd {userClient_Path}{user_directory_name} && " +
                $"{CscEXE_Path} " +
                $"/r:{ClassLibrary_CGC} " +
                $"/target:library " +
                $"/out:{userClass_dllName} {userClass_sourceName}";

            ////code = $"cd {userClient_Path}{user_directory_name} && " +
            ////    "dir &&" +
            ////    "ping 127.0.0.1 -n 6 > nul";

            //Process.Start("cmd.exe", code);

           Helper.startProccess(code, out output, out errorput);

            if (errorput!= "")
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
            Helper.startProccess($"cd {userClient_Path}{user_directory_name} && " +
               $"{CscEXE_Path} /r:{ClassLibrary_CGC};{userClass_dllName};{newtonjson} {userClient_sourceName}", out output, out errorput);

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
            if (!File.Exists($"{userClient_Path}{user_directory_name}\\{userClientexe_Name}"))
            {               
                throw new Exception($"Не удалось запустить exe tcp-клиента");
            }


            StringBuilder sb = new StringBuilder();
          
            sb.AppendFormat($"/C cd {userClient_Path}{user_directory_name} && {userClientexe_Name}");

            Process.Start("cmd.exe", sb.ToString());
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
