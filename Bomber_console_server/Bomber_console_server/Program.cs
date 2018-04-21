﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MySql.Data.MySqlClient;
using MySql.Data;
using Bomber_console_server.Model;
using System.Threading;


namespace Bomber_console_server
{
    class Program
    {  
        static MySQL mysql;

        static void Main(string[] args)
        {
            try
            {
                Helper.LOG("log.txt", "Начало работы");
                MonitoringGames();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                mysql.myConnection.Close();
                MonitoringGames();
            }   
        }

        static void MonitoringGames()
        {
            mysql = new MySQL();

            while (true)
            {
                List<SandboxGame> waitGames = mysql.GetWaitGameSandBox();
                Helper.LOG("log.txt", "Поиск ожидающих игровых сессий...");
                Helper.LOG("log.txt", $"Ожидающий сессий - {waitGames.Count}");

                for (int i = 0; i < waitGames.Count; i++)
                {
                    try
                    {
                        Helper.LOG("log.txt", $"Работа с сессией №{waitGames[i].id}");
                        mysql.SetSandboxGameWorkStatus(waitGames[i].id);
                        string[] temp_compiled_exe_path = new string[waitGames[i].usergroup.users.Count];

                        for (int j = 0; j < waitGames[i].usergroup.users.Count; j++)
                        {
                            dbUser tempUser = waitGames[i].usergroup.users[j];
                            tempUser.user_exe_phppath = $"{MyPath.binDir}\\{waitGames[i].id}\\{tempUser.id}\\{MyPath.exe_file_name}";
                        }
                        Session session = new Session(waitGames[i]);
                        session.InitGame();

                        mysql.SetSandboxGameCompiledStatus(waitGames[i].id, OpenGameResultFile(waitGames[i].id));
                        Helper.LOG("log.txt", $"Игровая сессия № {waitGames[i].id} успешно завершена");
                        
                    }
                    catch (Exception e)
                    {
                        Helper.LOG("log.txt", $"При работе игровой сессии №{waitGames[i].id} возникла Ошибка: {e.Message}");

                        mysql.SetSandboxGameErrorStatus(waitGames[i].id, e.Message);
                    }
                }

                Helper.LOG("log.txt", "Ждём...");
                Thread.Sleep(5000);
            }
        }


        /// <summary>
        /// Считать в строку из файла итоги игры
        /// </summary>
        /// <param name="sandbox_id">id игры</param>
        /// <returns></returns>
        static string OpenGameResultFile(int sandbox_id)
        {
            string json = "{}";

            using (StreamReader sr = new StreamReader($"{MyPath.binDir}\\{sandbox_id}\\{MyPath.gameResultsFileName}"))
            {
                json = sr.ReadToEnd();
            }
            return json;
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
