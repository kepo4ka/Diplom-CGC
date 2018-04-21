using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MySql.Data.MySqlClient;
using MySql.Data;
using Bomber_console_server.Model;


namespace Bomber_console_server
{
    class Program
    {
        static string binDir = "C:\\xampp\\htdocs\\sandbox";
        static string source_file_name = "strategy.cs";
        static string exe_file_name = "Program.exe";
        static MySQL mysql;

        static void Main(string[] args)
        {

            //Session session = new Session(GetUserSourcePathsFromFile());
            //session.InitGame();
            //Console.WriteLine("ССЕсСИЯ игры завершена");
            //Console.ReadKey();

            mysql = new MySQL();

            MonitoringGames();
            

        }

        static void MonitoringGames()
        {
            //while (true)
            //{
                List<SandboxGame> waitGames = mysql.GetWaitGameSandBox();
                for (int i = 0; i < waitGames.Count; i++)
                {
                    string[] temp_compiled_exe_path = new string[waitGames[i].usergroup.users.Count];

                    for (int j = 0; j < waitGames[i].usergroup.users.Count; j++)
                    {
                        User tempUser = waitGames[i].usergroup.users[j];
                        tempUser.phpPath = $"{binDir}\\{waitGames[i].id}\\{tempUser.id}";
                        temp_compiled_exe_path[j] = tempUser.phpPath;
                    }                          
                    Session session = new Session(temp_compiled_exe_path, waitGames[i].id, $"{binDir}\\{waitGames[i].id}");
                    session.InitGame();
                }    
            //}
        }




        static string[] GetUserSourcePathsFromFile()
        {
            string[] usersPath = new string[4];
            //using (StreamReader sr = new StreamReader("users_source.txt"))
            //{
            //    for (int i = 0; i < usersPath.Length; i++)
            //    {
            //        usersPath[i] = sr.ReadLine();
            //    }
            //}

            int i = 0;
            DirectoryInfo di = new DirectoryInfo(Path.GetFullPath($"{Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..")}\\User_sources"));
            foreach (FileInfo fi in di.GetFiles())
            {
                usersPath[i] = fi.FullName;
                i++;
            }

            return usersPath;
        }
    }
}
