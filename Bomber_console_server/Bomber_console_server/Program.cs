using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MySql.Data.MySqlClient;
using MySql.Data;
using Bomber_console_server.Model;
using System.Threading;
using Newtonsoft.Json;
using ClassLibrary_CGC;


namespace Bomber_console_server
{
    class Program
    {
        static MySQL mysql;
        static string gameType;

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
            Helper.LOG("log.txt", "Поиск ожидающих игровых сессий...");

            while (true)
            {

                gameType = "sandbox";
                WorkWithWaitGames();

                gameType = "rating";
                WorkWithWaitGames();
                


                Thread.Sleep(5000);
            }
        }


        static void WorkWithWaitGames()
        {           
            List<Game> waitGames = mysql.GetWaitGame(gameType);

            if (waitGames.Count > 0)
            {
                Helper.LOG("log.txt", $"Ожидающих сессий ({gameType}) - {waitGames.Count}");

                for (int i = 0; i < waitGames.Count; i++)
                {
                    try
                    {
                        Helper.LOG("log.txt", $"Работа с сессией №{waitGames[i].id}...");
                        mysql.SetGameWorkStatus(waitGames[i].id, gameType);
                        string[] temp_compiled_exe_path = new string[waitGames[i].usergroup.users.Count];

                        for (int j = 0; j < waitGames[i].usergroup.users.Count; j++)
                        {
                            dbUser tempUser = waitGames[i].usergroup.users[j];
                            tempUser.user_exe_phppath = $"{MyPath.binDir}\\{gameType}\\{waitGames[i].id}\\{tempUser.id}";
                        }
                        Session session = new Session(waitGames[i], $"{gameType}");

                        string json = OpenGameResultFile(waitGames[i].id);

                        if (gameType == "rating")
                        {
                            List<Player> users = JsonConvert.DeserializeObject<List<Player>>(json);

                            for (int k = 0; k < waitGames[i].usergroup.users.Count; k++)
                            {
                                var tempUser = waitGames[i].usergroup.users[k];

                                if (mysql.CheckUserIsBot(tempUser.id))
                                {
                                    continue;
                                }
                             
                                for (int j = 0; j < users.Count; j++)
                                {
                                    if (int.Parse(users[j].ID) == tempUser.id)
                                    {
                                        mysql.AddUserRatingPoints(tempUser.id, users[j].Points);
                                        break;
                                    }
                                }
                            }
                        }


                        mysql.SetGameCompiledStatus(waitGames[i].id, gameType, json);
                        Helper.LOG("log.txt", $"Игровая сессия ({gameType}) № {waitGames[i].id} успешно завершена!");
                    }
                    catch (Exception e)
                    {
                        Helper.LOG("log.txt", $"При работе игровой сессии ({gameType}) №{waitGames[i].id} возникла Ошибка: {e.Message}");
                        mysql.SetGameErrorStatus(waitGames[i].id, gameType, "Ошибка при работе сессии");
                    }
                }
            }

        }




        /// <summary>
        /// Считать в строку из файла итоги игры
        /// </summary>
        /// <param name="game_id">id игры</param>
        /// <returns></returns>
        static string OpenGameResultFile(int game_id)
        {
            string json = "{}";

            using (StreamReader sr = new StreamReader($"{MyPath.binDir}\\{gameType}\\{game_id}\\{MyPath.gameResultsFileName}"))
            {
                json = sr.ReadToEnd();
            }
            return json;
        }
    }
}
