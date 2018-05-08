using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using ClassLibrary_CGC;

namespace Bomber_console_server
{
    public class Compiler
    {
        static string main_Path;
        static string CscEXE_Path;
        static string assets_Path;
        public static string HostUserPath;
        public static string LogPath;
        public static string mapsPath;

        string output;
        string errorput;
        public string containerName;

        static string php_dir_path;
        static string main_php_path;

        public Compiler()
        {   
            CscEXE_Path = RuntimeEnvironment.GetRuntimeDirectory() + "csc.exe";

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

            mapsPath = assets_Path + "\\" + "maps";
            LogPath = $"{HostUserPath}\\log.txt";           
        }

        public Compiler(string _php_dir_path, int i, int gameId, string type)
        {
            if (_php_dir_path == "" || _php_dir_path == null)
            {
                throw new Exception("Неверное имя exe файла");
            }

            main_php_path = $"{MyPath.binDir}\\{type}\\{gameId}";
            php_dir_path = _php_dir_path;

            CscEXE_Path = RuntimeEnvironment.GetRuntimeDirectory() + "csc.exe";

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
            mapsPath = assets_Path + "\\" + "maps";

            HostUserPath = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            HostUserPath += $"\\docker_temp\\{type}";
            LogPath = $"{HostUserPath}\\log.txt";
            containerName = Helper.CalculateMD5Hash(DateTime.Now.Millisecond * Helper.rn.NextDouble() + "JOPA");           
           
        }

        /// <summary>
        /// Работа с Docker
        /// </summary>
        public void StartProccess(int serverPort)
        {
            DockerPreparation();
            DockerStart();
            Docker.Exec(containerName, $"mono /cgc/{MyPath.exe_file_name} {serverPort}");
        }


        static void CopyFileDockerToPHP()
        {
            DirectoryInfo di = new DirectoryInfo(HostUserPath);
            FileInfo[] fiA = di.GetFiles();
            foreach (FileInfo fi in fiA)
            {
                File.Copy(fi.FullName, $"{main_php_path}\\{fi.Name}");
            }
        }



        static void DeleteOnlyDirectories(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            DirectoryInfo[] diA = di.GetDirectories();
            foreach (DirectoryInfo df in diA)
            {
                Helper.DeleteDirectory(df.FullName);
              //  df.Delete();
            }
        }


        public static void EndProccess()
        {
            try
            {
                CopyFileDockerToPHP();
            }
            catch
            {
                Helper.LOG("log.txt", "CopyFileDockerToPHP ERROR");
            }

            try
            {
                DeleteOnlyDirectories(HostUserPath);
                DeleteOnlyDirectories($"{main_php_path}");
            }
            catch (Exception er)
            {
                Helper.LOG("log.txt", $"DeleteOnlyDirectories Error: {er.Message}");
            }
        }



        void MoveGameResultstoPHP()
        {
            //    if (!File.Exists($"{HostUserPath}\\{gameboardjsonpath}"))
        }


        public static void SaveGameStatesForVisualizer(List<GameBoard> gameBoardStates)
        {

            //BinaryFormatter form = new BinaryFormatter();
            //using (FileStream fs = new FileStream($"{HostUserPath}\\{MyPath.gameStaterVisualizerFileName}", FileMode.OpenOrCreate))
            //{
            //    form.Serialize(fs, gameBoardStates);
            //}

            using (StreamWriter sw = new StreamWriter($"{HostUserPath}\\{MyPath.gameStaterVisualizerJSONFileName}", false))
            {
                string visualizer = JsonConvert.SerializeObject(gameBoardStates);
                sw.Write(visualizer);
            }
        }

        public static void SaveGameResult(List<Player> players)
        {
            if (players != null)
            {
                using (StreamWriter sw = new StreamWriter($"{HostUserPath}\\{MyPath.gameResultsFileName}", false))
                {
                    string GameResultsJson = JsonConvert.SerializeObject(players);
                    sw.Write(GameResultsJson);
                }
            }
        }


        public static void SavePlayersAllCommands(List<List<Player>> players)
        {
            if (players != null)
            {
                using (StreamWriter sw = new StreamWriter($"{HostUserPath}\\{MyPath.userComandsFileName}", false))
                {
                    sw.AutoFlush = true;
                    string allTicksPlayersStats = JsonConvert.SerializeObject(players);
                    sw.Write(allTicksPlayersStats);
                }
            }
        }


        public static void SavePlayersAllCommandsUnity(string commands)
        {
            using (StreamWriter sw = new StreamWriter($"{HostUserPath}\\{MyPath.userComandsUnityFileName}", false))
            {
                sw.AutoFlush = true;
                sw.WriteLine(commands);
            }

        }

        


        public static void Compress()
        {
        //    Helper.Compress($"{HostUserPath}\\{MyPath.gameStaterVisualizerFileName}", $"{HostUserPath}\\{MyPath.gameStaterVisualizeFileNamerDATtoGZ}");
            Helper.Compress($"{HostUserPath}\\{MyPath.gameStaterVisualizerJSONFileName}", $"{HostUserPath}\\{MyPath.gameStaterVisualizeFileNamerJSONtoGZ}");
            if (File.Exists($"{HostUserPath}\\{MyPath.gameStaterVisualizerFileName}"))
            {
                try
                {
                    File.Delete($"{HostUserPath}\\{MyPath.gameStaterVisualizerJSONFileName}");
                }
                catch
                {

                }
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
            CreateDockerTempDirectory(containerName);
            CopyDependenciesToTempDirectory();
        }


        /// <summary>
        /// Запустить Docker контейнер
        /// </summary>
        void DockerStart()
        {
            string hostPath = $"{HostUserPath}\\{containerName}";
            Docker.Run(MyPath.dockerImage, hostPath, containerName);
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
            File.Copy($"{assets_Path}\\{MyPath.ClassLibrary_CGC}", $"{HostUserPath}\\{containerName}\\{MyPath.ClassLibrary_CGC}");
            File.Copy($"{assets_Path}\\{MyPath.NewtonJsonLibraryName}", $"{HostUserPath}\\{containerName}\\{MyPath.NewtonJsonLibraryName}");

            File.Copy($"{php_dir_path}\\{MyPath.exe_file_name}", $"{HostUserPath}\\{containerName}\\{MyPath.exe_file_name}");
            File.Copy($"{php_dir_path}\\{MyPath.userClass_dllName}", $"{HostUserPath}\\{containerName}\\{MyPath.userClass_dllName}");
        }
    }
}
