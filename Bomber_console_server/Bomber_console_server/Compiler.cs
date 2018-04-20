using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Bomber_console_server
{
    public class Compiler
    {
      static  string main_Path;
       static string CscEXE_Path;
        static string assets_Path;

        static string dockerImage;
      public static string HostUserPath;
        public static string LogPath;       
        static string gameboardjsonpath;
        static string userjsonpath;   

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

        string user_exe_php_path;



        public static List<string> compileDirectories = new List<string>();


        public Compiler(string user_exe_path, int i)
        {           
            if (user_exe_path == "" || user_exe_path == null)
            {
                throw new Exception("Неверное имя exe файла");
            }

            dockerImage = "kepo4ka/ubuntu_mono";

            main_Path = Directory.GetCurrentDirectory();
            HostUserPath = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            HostUserPath += $"\\docker_temp\\{Session.gameID}";
            LogPath = $"{HostUserPath}\\log.txt";

            gameboardjsonpath = "gameboard.json";
            userjsonpath = "user.json";
            
            assets_Path = Path.GetFullPath(Path.Combine(main_Path, @"..\") + "\\assets");

            if (main_Path.Contains("\\bin\\"))
            {
                assets_Path = Path.GetFullPath(Path.Combine(main_Path, @"..\..\..\..") + "\\assets");
                main_Path = Path.GetFullPath(Path.Combine(main_Path, @"..\..\.."));
            }         
  
            CscEXE_Path = RuntimeEnvironment.GetRuntimeDirectory() + "csc.exe";
           // userClass_Path = Helper.SpliteEndPath(user_exe_path,true);
            userClient_Path = main_Path + "\\" + "User_client\\User_client\\";
            
           // userClass_sourceName = Helper.SpliteEndPath(user_exe_path) + ".cs";
            userClass_dllName = "User_class.dll";

            user_directory_name = "User_" + i;

        //    userClient_sourceName = "Program.cs";
            userClientexe_Name = "Program.exe";

            user_exe_php_path = user_exe_path;

            ClassLibrary_CGC = "ClassLibrary_CGC.dll";
            newtonjson = "Newtonsoft.Json.dll";
            containerName = Helper.CalculateMD5Hash(DateTime.Now.Millisecond * Helper.rn.NextDouble() + "JOPA");           
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


        /// <summary>
        /// Работа с Docker
        /// </summary>
        public void StartProccess(int serverPort)
        {
            DockerPreparation();
            DockerStart();
            Docker.Exec(containerName, $"mono /cgc/{userClientexe_Name} {serverPort}");
        }


        static void DeleteDockerTempDirectory()
        {
            DirectoryInfo di = new DirectoryInfo(HostUserPath);
            DirectoryInfo[] diA = di.GetDirectories();
            foreach (DirectoryInfo df in diA)
            {
                Helper.DeleteDirectory(df.FullName);
                df.Delete();
            }
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
            try
            {
                DeleteDockerTempDirectory();
            }
            catch
            {
                Console.WriteLine("DeleteDockerTempDirectory Error");
            }
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
            Docker.Run(dockerImage, hostPath, containerName);
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
            File.Copy($"{assets_Path}\\{ClassLibrary_CGC}", $"{HostUserPath}\\{containerName}\\{ClassLibrary_CGC}");
            File.Copy($"{assets_Path}\\{userClass_dllName}", $"{HostUserPath}\\{containerName}\\{userClass_dllName}");
            File.Copy($"{assets_Path}\\{newtonjson}", $"{HostUserPath}\\{containerName}\\{newtonjson}");
          
            File.Copy($"{user_exe_php_path}", $"{HostUserPath}\\{containerName}\\{userClientexe_Name}");

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
        public void SaveTempGameInfo(string gameboardinfo, string userinfo, bool k = false)
        {
            //   string gbpath = $"{userClient_Path}{user_directory_name}\\{gameboardjsonpath}";
            //   string uspath = $"{userClient_Path}{user_directory_name}\\{userjsonpath}";

            string gbpath = $"{HostUserPath}\\{containerName}\\{gameboardjsonpath}";
            string uspath = $"{HostUserPath}\\{containerName}\\{userjsonpath}";           
            
            Helper.WriteDataJson(gameboardinfo, gbpath, k);
            Helper.WriteDataJson(userinfo, uspath, k);           
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
        public void UserClientStart(int port)
        {
            if (!File.Exists($"{userClient_Path}{user_directory_name}\\{userClientexe_Name}"))
            {               
                throw new Exception($"Не удалось запустить exe tcp-клиента");
            }


            StringBuilder sb = new StringBuilder();
          
            sb.AppendFormat($"/C cd {userClient_Path}{user_directory_name} && {userClientexe_Name} {port}");

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
