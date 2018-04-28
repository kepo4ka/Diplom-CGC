﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using ClassLibrary_CGC;
using User_class;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Bomber_wpf
{

    public partial class Form1 : Form
    {
        StartPage startPage;

        public int cw = 30;
        Graphics g;
        Pen p;
        SolidBrush sb;
        GameBoard gb;
        GameBoard tempGB;
        int GameTimer;
        int TimeLimit = Config.one_tick_client_wait_time;
        int globalTimeLimit = Config.all_game_client_max_wait_timeout;

        Color[] player_colors = new Color[4]
        {
            Color.PaleVioletRed, Color.Green, Color.DodgerBlue, Color.Aqua
        };


        bool isGameOver = false;

        IPAddress serverIp = IPAddress.Parse("127.0.0.1");
        int serverPort;
        TcpListener server;
        List<UserInfo> usersInfo = new List<UserInfo>();
        List<GameBoard> gameBoardStates = new List<GameBoard>();
        List<GameBoard> savedGameBoardStates = new List<GameBoard>();
        int visualizeGameCadrNumber = 0;
        public static string gameID;

        public string gameboardjson;
        List<int> Prioritets = new List<int>();

        List<Cell> DestroyedCells;
        List<Player> DestroyedPlayers;
        int[,] gbpseudo = new int[15, 15];
        List<Player> pseudoplayers;



        Random rn = new Random();


        /// <summary>
        /// Игра в реальном времени
        /// </summary>
        /// <param name="pstartPage"></param>
        public Form1(StartPage pstartPage, int[,] gbpseudo = null)
        {
            InitializeComponent();
            g = panel1.CreateGraphics();
            p = new Pen(Color.Black);
            sb = new SolidBrush(Color.DimGray);

            serverPort = 9595;

            serverStart();

            startPage = pstartPage;

            InitGame(gbpseudo);
        }

        /// <summary>
        /// Воспроизведение сохранённой игры
        /// </summary>
        /// <param name="pstartPage"></param>
        /// <param name="pgameBoardStatates"></param>
        public Form1(StartPage pstartPage, List<GameBoard> pgameBoardStatates)
        {
            InitializeComponent();
            g = panel1.CreateGraphics();
            p = new Pen(Color.Black);
            sb = new SolidBrush(Color.DimGray);
            startPage = pstartPage;

            savedGameBoardStates = pgameBoardStatates;
            InitVisualizingGame();
        }


        void CheckUserCodeSourcesPath()
        {
            int k = 0;
            for (int i = 0; i < startPage.paths.Length; i++)
            {
                switch (startPage.paths[i])
                {
                    case null:
                        continue;
                    case "":
                        gb.Players.Add(CreatePlayer(false, k));
                        k++;
                        break;
                    default:
                        Compiler cmp = CompileAndStartUserFiles(startPage.paths[i], i);
                        if (cmp == null)
                        {
                            continue;
                        }

                        TcpClient tcp = server.AcceptTcpClient();
                        if (tcp == null)
                        {
                            continue;
                        }


                        User tuser = (User)CreatePlayer(true, k);
                        usersInfo.Add(new UserInfo(tuser, tcp, cmp));

                        gb.Players.Add(tuser);
                        k++;
                        break;
                }
                //   Prioritets.Add(rn.Next(0, 100));
            }
        }


        /// <summary>
        /// Создание игрока на основе данных из формы
        /// </summary>
        /// <param name="i"></param>
        Player CreatePlayer(bool k, int i)
        {
            string[] ppaths = startPage.paths;
            Player pplayer;

            if (!k)
            {
                pplayer = new Bot();
                pplayer.Name = "Bot_" + (i + 1);
            }
            else
            {
                pplayer = new User();
                pplayer.Name = "User_" + (i + 1);
            }

            pplayer.ID = Helper.CalculateMD5Hash(DateTime.Now.Millisecond * rn.NextDouble() + "JOPA" + pplayer.Name);
            pplayer.Points = 0;

            //   pplayer.Health = 5;
            pplayer.BangRadius = Config.bang_start_radius;
            pplayer.BombsCount = Config.player_bombs_count_start;

            //switch (i)
            //{
            //    case 0:
            //        pplayer.X = 0;
            //        pplayer.Y = 0;
            //        break;
            //    case 1:
            //        //  pplayer.X = gb.W - 1;
            //        pplayer.X = 1;
            //        pplayer.Y = 0;
            //        break;
            //    case 2:
            //        pplayer.X = 0;
            //        pplayer.Y = gb.H - 1;
            //        break;
            //    case 3:
            //        pplayer.X = gb.W - 1;
            //        pplayer.Y = gb.H - 1;
            //        break;
            //}

            pplayer.X = pseudoplayers[0].X;
            pplayer.Y = pseudoplayers[0].Y;
            pseudoplayers.RemoveAt(0);

            return pplayer;
        }



        /// <summary>
        /// Запуск визуализации сохраннённой игры
        /// </summary>
        public void InitVisualizingGame()
        {
            gb = savedGameBoardStates[0];

            initListView();

            game_timer.Tick += visualizing_game_Tick;
            game_timer.Interval = 1000;
            game_timer.Start();
            GameTimer = 0;

        }

        public void visualizing_game_Tick(object sender, EventArgs e)
        {
            NextTickInVisualizingGame();
        }

        /// <summary>
        /// Следующий "Кадр" воспроизведения
        /// </summary>
        public void NextTickInVisualizingGame()
        {
            gb = savedGameBoardStates[GameTimer];
            GameTimer++;
            DrawAll();

            CheckVisualizingGameOver();

            UpdateListView();
        }

        /// <summary>
        /// Проверка условия окончания воспроизведения игры
        /// </summary>
        public void CheckVisualizingGameOver()
        {
            if (GameTimer >= savedGameBoardStates.Count)
            {
                VisualizingGameOver();
            }
        }


        /// <summary>
        /// Окончание воспроизведения игры
        /// </summary>
        public void VisualizingGameOver()
        {
            game_timer.Stop();
            var result = DialogResult.No;
            string message = "Воспроизведение завершено!\n Начать заново?";
            result = MessageBox.Show(message, "GAME OVER",
                                 MessageBoxButtons.YesNo,
                                 MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                InitVisualizingGame();
            }
            else
            {
                this.Hide();
                game_timer.Stop();
                startPage.Show();
            }
        }


        /// <summary>
        /// Начать сеанс игры
        /// </summary>
        public void InitGame(int[,] gbpseudo = null)
        {
            isGameOver = false;

            GameTimer = Config.gameTicksMax;
            pseudoplayers = new List<Player>();
            Prioritets = new List<int>();
            DestroyedPlayers = new List<Player>();
            DestroyedCells = new List<Cell>();

            if (gbpseudo == null)
            {
                gb = new GameBoard();
            }
            else
            {
                gb = new GameBoard(gbpseudo);
                for (int i = 0; i < gb.Players.Count; i++)
                {
                    Player tplayer = new Player();
                    tplayer.X = gb.Players[i].X;
                    tplayer.Y = gb.Players[i].Y;
                    pseudoplayers.Add(tplayer);
                }

                gb.Players.Clear();
            }
            SetPseudoPlayers();

            formLog(gb.Bonuses.Count + "");
            // gameID = Helper.CalculateMD5Hash(DateTime.Now.Millisecond * Helper.rn.NextDouble() + "JOPAJOPA");

            CheckUserCodeSourcesPath();

            SetPrioritets();
            List<int> tt = Prioritets;

            gameBoardStates.Add((GameBoard)gb.Clone());

            game_timer.Tick += game_timer_Tick;
            game_timer.Interval = 800;
            game_timer.Start();

            initListView();

        }

        public void SetPrioritets()
        {
       
            while (Prioritets.Count < gb.Players.Count)
            {
                int prioritet = rn.Next(0, gb.Players.Count);
                bool isExist = false;
                for (int i = 0; i < Prioritets.Count; i++)
                {
                    if (Prioritets[i]==prioritet)
                    {
                        isExist = true;
                        break;
                    }
                }

                if (isExist ==false)
                {
                    Prioritets.Add(prioritet);
                }
            }
        }


        /// <summary>
        /// Отправить сообщение по tcp
        /// </summary>
        /// <param name="stream">Сетевой Поток</param>
        /// <param name="message">Сообщение</param>
        static void SendMessage(NetworkStream stream, string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Получить сообщение по tcp
        /// </summary>
        /// <param name="stream">Сетевой поток</param>
        /// <returns>Полученное сообщение</returns>
        static string ReceiveMessage(NetworkStream stream)
        {
            string message = "";

            byte[] data = new byte[256]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();

            int bytes = 0;
            do
            {
                bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);

            message = builder.ToString();
            return message;
        }


        public void streamWrite(Stream strm, string message)
        {
            Helper.LOG(Compiler.LogPath, "send length " + message.Length);
            using (StreamWriter sw = new StreamWriter(strm))
            {
                sw.Write(message);
                sw.Flush();
            }
        }

        public string streamRead(Stream strm)
        {
            string message = "";
            using (StreamReader sr = new StreamReader(strm))
            {
                message = sr.ReadToEnd();
            }
            Helper.LOG(Compiler.LogPath, "read length " + message.Length);
            return message;
        }




        /// <summary>
        /// Отправить Клиентам инофрмацию об Игровом мире (объект класса GameBoard)
        /// </summary>
        public void CommunicateWithClients()
        {
            for (int i = 0; i < usersInfo.Count; i++)
            {
                try
                {
                    if (usersInfo[i].client == null || usersInfo[i].player.Health < 1)
                    {
                        continue;
                    }
                    UserInfo tempUserInfo = usersInfo[i];
                    NetworkStream strm = tempUserInfo.client.GetStream();
                    string userjson = JsonConvert.SerializeObject(tempUserInfo.player);

                    SendMessage(tempUserInfo.client.GetStream(), gameboardjson);
                    ReceiveMessage(strm);


                    SendMessage(tempUserInfo.client.GetStream(), userjson);


                    string name = ReceiveMessage(strm);


                    Stopwatch a = new Stopwatch();

                    a.Start();
                    SendMessage(strm, "p");
                    string action = "";

                    string sleeptime = ReceiveMessage(strm);
                    formLog(sleeptime + " ms - " + name);

                    SendMessage(strm, "p");

                    action = ReceiveMessage(strm);





                    tempUserInfo.player.ACTION = Helper.DecryptAction(action);

                    //Helper.LOG(Compiler.LogPath, "gamebouard length " + gameboardjson.Length);
                    //streamWrite(strm, gameboardjson);

                    //Helper.LOG(Compiler.LogPath, "userjson length " + userjson.Length);

                    //streamWrite(strm, userjson);


                    //string action = streamRead(strm);


                    //tempUserInfo.player.ACTION = Helper.DecryptAction(action);




                }
                catch (Exception er)
                {
                    Helper.LOG(Compiler.LogPath, "CommunicateWithClients ERROR: " + er.Message);
                    PlayerDisconnect(usersInfo[i].player);
                }
            }
        }




        public void serverStart()
        {
            try
            {
                server = new TcpListener(serverIp, serverPort);
                server.Start();
            }
            catch (SocketException er)
            {
                Helper.LOG(Compiler.LogPath, "serverStart SocketException: " + er.Message);
                Environment.Exit(0);
            }
        }





        /// <summary>
        /// Обновить информацию о живых игроках
        /// </summary>
        public void UpdateListView()
        {
            players_listView.Items.Clear();

            for (int i = 0; i < gb.Players.Count; i++)
            {
                var tplayer = gb.Players[i];

                var item = new ListViewItem(new[] {
                    tplayer.Name,
                    tplayer.ID,
                    tplayer.Health.ToString(),
                    tplayer.Points.ToString(),
                    tplayer.ACTION.ToString(),
                    tplayer.BombsCount.ToString(),
                    tplayer.BangRadius.ToString(),
                    tplayer.X.ToString(),
                    tplayer.Y.ToString()
                });
                item.BackColor = player_colors[i];
                players_listView.Items.Add(item);
            }
        }


        /// <summary>
        /// Задать списки игроков
        /// </summary>
        public void initListView()
        {
            players_listView.Clear();

            players_listView.View = View.Details;

            players_listView.Columns.Add("Name");
            players_listView.Columns.Add("ID");
            players_listView.Columns.Add("Health");
            players_listView.Columns.Add("Points");
            players_listView.Columns.Add("PlayerAction");
            players_listView.Columns.Add("BombsCount");
            players_listView.Columns.Add("BangRadius");
            players_listView.Columns.Add("X");
            players_listView.Columns.Add("Y");

            for (int i = 0; i < gb.Players.Count; i++)
            {
                var tplayer = gb.Players[i];

                var item = new ListViewItem(new[] {
                    tplayer.Name,
                    tplayer.ID,
                    tplayer.Health.ToString(),
                    tplayer.Points.ToString(),
                    tplayer.ACTION.ToString(),
                    tplayer.BombsCount.ToString(),
                    tplayer.BangRadius.ToString(),
                    tplayer.X.ToString(),
                    tplayer.Y.ToString()
                });
                players_listView.Items.Add(item);
            }
        }

        private void game_timer_Tick(object sender, EventArgs e)
        {
            NextTick();
        }

        /// <summary>
        /// Отрисовать все элементы
        /// </summary>
        public void DrawAll()
        {
            panel1.Refresh();
            this.Text = "Тик - " + GameTimer;
            DrawCells();
            DrawLavas();
            DrawBonuses();
            DrawPlayers();
            DrawBombs();
            DrawGrid();
        }

        /// <summary>
        /// Нарисовать видимые бонусы
        /// </summary>
        public void DrawBonuses()
        {
            for (int i = 0; i < gb.Bonuses.Count; i++)
            {
                var tbonus = gb.Bonuses[i];

                if (tbonus.Visible == false)
                {
                    continue;
                }
                Color tcolor;
                if (tbonus.Type == BonusType.Ammunition)
                {
                    tcolor = Config.bonus_big;
                }
                else
                {
                    tcolor = Config.bonus_fast;
                }
                PaintEllipse(tbonus.X, tbonus.Y, tcolor);
            }
        }

        /// <summary>
        /// Отрисовать живых Игроков на поле
        /// </summary>
        public void DrawPlayers()
        {
            for (int i = 0; i < gb.Players.Count; i++)
            {
                var tplayer = gb.Players[i];
                if (tplayer.Health > 0)
                {
                    PaintPlayer(tplayer.X, tplayer.Y, player_colors[i]);
                }
            }
        }

        /// <summary>
        /// Отрисовать Бомбы
        /// </summary>
        public void DrawBombs()
        {
            for (int i = 0; i < gb.Bombs.Count; i++)
            {
                var tbomb = gb.Bombs[i];
                PaintBomb(tbomb.X, tbomb.Y, Config.bomb_color);
            }
        }

        /// <summary>
        /// Отрисовать лаву
        /// </summary>
        public void DrawLavas()
        {
            for (int k = 0; k < gb.Lavas.Count; k++)
            {
                var tlava = gb.Lavas[k];
                PaintRect(tlava.X, tlava.Y, Config.lava_color);
            }
        }

        /// <summary>
        /// Обновить состояние игрового мира
        /// </summary>
        public void GameProccess()
        {
            GameTimer--;

            //    PlayerProcess();
            PlayerProccessTest();
            PlayerBonusCollision();
            BombsProccess();
            LavasProccess();

            DestroyedObjectProccess();

            ChangePrioritet();
        }

        /// <summary>
        /// Удалить объекты, которые участвовали в коллизиях игроков
        /// </summary>
        public void DestroyedObjectProccess()
        {
            for (int i = 0; i < DestroyedPlayers.Count; i++)
            {
                PlayerDeath(DestroyedPlayers[i]);
            }
            DestroyedPlayers.Clear();

            for (int i = 0; i < DestroyedCells.Count; i++)
            {
                DestroyedCells[i].Type = CellType.Free;
            }
            DestroyedCells.Clear();
        }

        /// <summary>
        /// Обработка передаваемой и сохраняемой информации об игре в текущий Тик
        /// </summary>
        public void GameboardInfoProccess()
        {
            tempGB = null;
            tempGB = (GameBoard)gb.Clone();
            SetXYInfo();
            gameBoardStates.Add(tempGB);
            gameboardjson = JsonConvert.SerializeObject(tempGB);
        }


        /// <summary>
        /// Следующий ход игры
        /// </summary>
        public void NextTick()
        {
            CheckGameOver();

            if (isGameOver == false)
            {
                GameProccess();
                DrawAll();
                GameboardInfoProccess();
                CommunicateWithClients();
                UpdateListView();
            }
        }


        public void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        /// <summary>
        /// Изменить приоритет ходов игроков
        /// </summary>
        public void ChangePrioritet()
        {
            int last = Prioritets[Prioritets.Count - 1];

            for (int i = Prioritets.Count - 1; i > 0; i--)
            {
                Prioritets[i] = Prioritets[i - 1];
            }
            Prioritets[0] = last;
        }



        public void SetPseudoPlayers()
        {
            int[] tx = new int[]
             {
               0, 14, 0, 14
             };
            int[] ty = new int[]
            {
              0, 0, 14, 14
            };


            if (pseudoplayers.Count == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    Player tplayer = new Player();
                    tplayer.X = tx[i];
                    tplayer.Y = ty[i];

                    pseudoplayers.Add(tplayer);
                }
            }
            else
            {
                while (pseudoplayers.Count < 4)
                {
                    Player tplayer = new Player();

                    for (int i = 0; i < 4; i++)
                    {
                        bool inarray = false;
                        for (int j = 0; j < pseudoplayers.Count; j++)
                        {
                            if (pseudoplayers[j].X == tx[i] && pseudoplayers[j].Y == ty[i])
                            {
                                inarray = true;
                                break;
                            }
                        }
                        if (!inarray)
                        {
                            tplayer.X = tx[i];
                            tplayer.Y = ty[i];
                            pseudoplayers.Add(tplayer);
                            break;
                        }
                    }


                }

            }
        }


        /// <summary>
        /// Получить информацию о каждой клетке: какие объекты в ней сейчас расположены (Игроки, Бомбы, Видимые Бонусы, Лава (с наибольшим LiveTime)
        /// </summary>
        public void SetXYInfo()
        {
            for (int i = 0; i < tempGB.XYinfo.GetLength(0); i++)
            {
                for (int j = 0; j < tempGB.XYinfo.GetLength(1); j++)
                {
                    var tXYInfo = tempGB.XYinfo[i, j];

                    for (int ii = 0; ii < tempGB.Players.Count; ii++)
                    {
                        if (tempGB.Players[ii].X == i && tempGB.Players[ii].Y == j)
                        {
                            tXYInfo.Player = tempGB.Players[ii];
                            break;
                        }
                    }


                    int tmaxLiveTime = 0;

                    for (int ii = 0; ii < tempGB.Lavas.Count; ii++)
                    {
                        if (tempGB.Lavas[ii].X == i && tempGB.Lavas[ii].Y == j)
                        {
                            if (gb.Lavas[ii].LiveTime >= tmaxLiveTime)
                            {
                                tmaxLiveTime = tempGB.Lavas[ii].LiveTime;

                                tXYInfo.Lava = tempGB.Lavas[ii];
                            }
                        }
                    }


                    for (int ii = 0; ii < tempGB.Bonuses.Count; ii++)
                    {
                        if (tempGB.Bonuses[ii].X == i && tempGB.Bonuses[ii].Y == j)
                        {
                            tXYInfo.Bonus = tempGB.Bonuses[ii];
                            break;
                        }
                    }

                    for (int ii = 0; ii < tempGB.Bombs.Count; ii++)
                    {
                        if (gb.Bombs[ii].X == i && tempGB.Bombs[ii].Y == j)
                        {
                            tXYInfo.Bomb = tempGB.Bombs[ii];
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Окончание игры: очищение, разрыв соединения, отображение и сохранение информации
        /// </summary>
        public void GameOver(List<Player> winners)
        {
            isGameOver = true;

            if (winners.Count == 1)
            {
                winners[0].Points += Config.player_win_points;
            }
            else
            {
                for (int i = 0; i < winners.Count; i++)
                {
                    winners[i].Points += Config.player_survive_points;
                }
            }

            gameBoardStates.Add(gb);
            SaveGameInfo();
            StopClearTempFiles();

            EndGameMessage(winners);
        }


        void StopClearTempFiles()
        {
            try
            {
                game_timer.Stop();
                gameBoardStates.Clear();

                Disconnect();
                if (server != null)
                {
                    server.Stop();
                }
                Compiler.EndProccess();

                for (int i = 0; i < usersInfo.Count; i++)
                {
                    try
                    {
                        if (usersInfo[i].client != null)
                        {
                            usersInfo[i].client.Close();
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                usersInfo.Clear();

            }
            catch (Exception e)
            {
                Helper.LOG(Compiler.LogPath, "StopClearTempFiles ERROR: " + e.Message);
            }
        }


        /// <summary>
        /// Диалоговое окно с информацией о результатах игровой сессии
        /// </summary>
        /// <param name="winners"></param>
        public void EndGameMessage(List<Player> winners)
        {
            var result = DialogResult.No;
            string message = "";

            winners.Sort((a, b) => a.Health.CompareTo(b.Health));

            if (GameTimer < 1)
            {
                message += "ВРЕМЯ И СТЕКЛО \n";
            }

            message += "Игроки: \n";

            for (int i = 0; i < gb.Players.Count; i++)
            {
                message += gb.Players[i].Name + ": " + gb.Players[i].Points + " (баллы) \n";
            }

            message += "Начать заново?";

            result = MessageBox.Show(message, "GAME OVER",
                              MessageBoxButtons.YesNo,
                              MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.Close();

                this.startPage.OpenRealGameForm();
            }
            else
            {
                this.Hide();
                startPage.Show();
            }
        }


        /// <summary>
        /// Проверить наступили ли условия для наступления Конца игры
        /// </summary>
        public void CheckGameOver()
        {
            List<Player> winners = gb.Players.FindAll(c => c.Health > 0);

            if (GameTimer < 1 || winners.Count < 2)
            {
                GameOver(winners);
            }
        }

        /// <summary>
        /// Сохранение "слепков" игры в виде списка, где индекс - это номер Тика игры
        /// </summary>
        public void SaveGameInfo()
        {
            //try
            //{
            string gameResultDirectoryName = Directory.GetCurrentDirectory() + "\\" + GetInfoAboutThisGame() + "\\";

            Helper.DeleteDirectory(gameResultDirectoryName);
            Directory.CreateDirectory(gameResultDirectoryName);


            string gameStaterVisualizerFileName = "Visualizer.dat";
            string gameStaterVisualizerJSONFileName = "Visualizer.json";
            string userComandsFileName = "UserCommands.json";

            string gameResultsFileName = "gameResults.json";

            BinaryFormatter form = new BinaryFormatter();
            using (FileStream fs = new FileStream($"{gameResultDirectoryName}\\{gameStaterVisualizerFileName}", FileMode.OpenOrCreate))
            {
                form.Serialize(fs, gameBoardStates);
            }

            using (StreamWriter sw = new StreamWriter($"{gameResultDirectoryName}\\{gameStaterVisualizerJSONFileName}", false))
            {
                string visualizer = JsonConvert.SerializeObject(gameBoardStates);
                sw.Write(visualizer);
            }

            using (StreamWriter sw = new StreamWriter($"{gameResultDirectoryName}\\{gameResultsFileName}", false))
            {
                string GameResultsJson = JsonConvert.SerializeObject(GetPlayerResult());
                sw.Write(GameResultsJson);
            }

            using (StreamWriter sw = new StreamWriter($"{gameResultDirectoryName}\\{userComandsFileName}", false))
            {
                string allTicksPlayersStats = JsonConvert.SerializeObject(GetPlayersInfoAllTicks());
                sw.Write(allTicksPlayersStats);
            }

            //}
            //catch (Exception e)
            //{
            //    MessageBox.Show("Ошибка при сохранении информации об игре в файл: " + e.Message);
            //}
        }




        /// <summary>
        /// Получить информацию о результатах игроков
        /// </summary>
        /// <returns></returns>
        private List<Player> GetPlayerResult()
        {
            if (gameBoardStates.Count < 1)
            {
                return null;
            }

            List<Player> players = new List<Player>();

            GameBoard lastGameBoardState = gameBoardStates[gameBoardStates.Count - 1];

            for (int i = 0; i < lastGameBoardState.Players.Count; i++)
            {
                var tplayer = lastGameBoardState.Players[i];
                players.Add(tplayer);
            }

            return players;
        }


        /// <summary>
        /// Получить информацию о текущей игры для последующего создания сохранений и т.п.
        /// </summary>
        /// <returns></returns>
        public string GetInfoAboutThisGame()
        {
            DateTime time = DateTime.Now;
            string time_str = time.ToString("dd-MM-yyyy H-mm-ss");

            string gameStatesFileName = "Game - (";
            if (gameBoardStates[0].Players.Count > 1)
            {
                for (int i = 0; i < gameBoardStates[0].Players.Count - 1; i++)
                {
                    var tplayer = gameBoardStates[0].Players[i];
                    gameStatesFileName += tplayer.Name + " vs ";
                }
            }
            gameStatesFileName += gameBoardStates[0].Players[gameBoardStates[0].Players.Count - 1].Name + ")";


            gameStatesFileName += " " + time_str;

            return gameStatesFileName;
        }

        public List<List<Player>> GetPlayersInfoAllTicks()
        {
            List<List<Player>> AllPlayersStates = new List<List<Player>>();

            for (int i = 0; i < gameBoardStates.Count; i++)
            {
                AllPlayersStates.Add(gameBoardStates[i].Players);
            }
            return AllPlayersStates;
        }



        public void PlayerProccessTest()
        {
            for (int i = 0; i < Prioritets.Count; i++)
            {
                Player tplayer = gb.Players[Prioritets[i]];

                if (tplayer is Bot)
                {
                    tplayer.ACTION = tplayer.Play();
                }     

                PlayerMove(gb.Players[Prioritets[i]]);
            }
        }


        /// <summary>
        /// обработка действий игроков
        /// </summary>
        public void PlayerProcess()
        {
            List<Player> tempplayers = new List<Player>();
            for (int i = 0; i < gb.Players.Count; i++)
            {
                var tvitya = gb.Players[i];

                if (tvitya.Health == 0)
                {
                    continue;
                }

                if (tvitya is Bot)
                {
                    tvitya.ACTION = tvitya.Play();
                }

                PlayerFire(tvitya);

                Player tempplayer = new Player()
                {
                    ACTION = tvitya.ACTION,
                    X = tvitya.X,
                    Y = tvitya.Y
                };
                PlayerMove(tempplayer);
                tempplayers.Add(tempplayer);
            }

            //for (int i = 0; i < tempplayers.Count; i++)
            //{
            //    var tvitya1 = tempplayers[i];
            //    for (int j = i + 1; j < tempplayers.Count; j++)
            //    {
            //        var tvitya2 = tempplayers[j];
            //        if (tvitya1.X == tvitya2.X && tvitya1.Y == tvitya2.Y)
            //        {
            //            gb.Players[i].ACTION = PlayerAction.Wait;
            //            gb.Players[j].ACTION = PlayerAction.Wait;
            //        }
            //        if (tvitya2.X == gb.Players[i].X && tvitya2.Y == gb.Players[i].Y && tvitya1.X == gb.Players[i].X && tvitya1.Y == gb.Players[i].Y)
            //        {
            //            gb.Players[i].ACTION = PlayerAction.Wait;
            //            gb.Players[j].ACTION = PlayerAction.Wait;
            //        }
            //    }
            //}

            //for (int i = 0; i < tempplayers.Count; i++)
            //{
            //    var tvitya1 = tempplayers[i];
            //    for (int j = i + 1; j < tempplayers.Count; j++)
            //    {
            //        var tvitya2 = tempplayers[j];
            //        if (tvitya1.X == tvitya2.X && tvitya1.Y == tvitya2.Y)
            //        {
            //            if (usersInfo.Find(c => c.player == tvitya1).prioritet > usersInfo.Find(c => c.player == tvitya2).prioritet)
            //            {
            //              //  gb.Players[i].ACTION = PlayerAction.Wait;
            //                gb.Players[j].ACTION = PlayerAction.Wait;
            //            }
            //            else
            //            {
            //                gb.Players[i].ACTION = PlayerAction.Wait;
            //            }
            //        }
            //        //if (tvitya2.X == gb.Players[i].X && tvitya2.Y == gb.Players[i].Y && tvitya1.X == gb.Players[i].X && tvitya1.Y == gb.Players[i].Y)
            //        //{
            //        //    gb.Players[i].ACTION = PlayerAction.Wait;
            //        //    gb.Players[j].ACTION = PlayerAction.Wait;
            //        //}
            //    }
            //}


            for (int i = 0; i < tempplayers.Count; i++)
            {
                var tvitya1 = tempplayers[i];
                for (int j = i + 1; j < tempplayers.Count; j++)
                {
                    var tvitya2 = tempplayers[j];
                    if (tvitya1.X == tvitya2.X && tvitya1.Y == tvitya2.Y)
                    {
                        if (Prioritets[i] > Prioritets[j])
                        {
                            // gb.Players[i].ACTION = PlayerAction.Wait;
                            gb.Players[j].ACTION = PlayerAction.Wait;
                        }
                        else
                        {
                            gb.Players[i].ACTION = PlayerAction.Wait;
                        }
                    }
                    //if (tvitya2.X == gb.Players[i].X && tvitya2.Y == gb.Players[i].Y && tvitya1.X == gb.Players[i].X && tvitya1.Y == gb.Players[i].Y)
                    //{                       
                    //    gb.Players[i].ACTION = PlayerAction.Wait;
                    //    gb.Players[j].ACTION = PlayerAction.Wait;
                    //}
                }
            }



            for (int i = 0; i < gb.Players.Count; i++)
            {
                if (gb.Players[i].Health > 0)
                {
                    PlayerMove(gb.Players[i]);
                }
            }
        }


        /// <summary>
        /// Подбор игроками Бонусов
        /// </summary>
        public void PlayerBonusCollision()
        {
            Bonus[,] tempbonus = ListToMass(gb.Bonuses);

            for (int k = 0; k < gb.Players.Count; k++)
            {
                var tvitya = gb.Players[k];

                if (tvitya.Health < 1)
                {
                    continue;
                }

                int i = tvitya.X;
                int j = tvitya.Y;

                if (tempbonus[i, j] != null && tempbonus[i, j].Visible == true)
                {
                    tvitya.Points += Config.player_bonus_find_points;

                    if (tempbonus[i, j].Type == BonusType.Radius)
                    {
                        tvitya.BangRadius++;
                    }
                    else
                    {
                        tvitya.BombsCount++;
                    }

                    //    gb.Bonuses.Remove(tempbonus[i, j]);
                    tempbonus[i, j] = null;
                }
            }
        }



        /// <summary>
        /// Перевести список в матрицу координат бомб
        /// </summary>
        /// <param name="pbombs"></param>
        /// <returns></returns>
        public Bomb[,] ListToMass(List<Bomb> pbombs)
        {
            Bomb[,] tbombs_mass = new Bomb[gb.W, gb.H];

            for (int i = 0; i < pbombs.Count; i++)
            {
                tbombs_mass[pbombs[i].X, pbombs[i].Y] = pbombs[i];
            }
            return tbombs_mass;
        }



        /// <summary>
        /// Перевести список в матрицу координат бонусов 
        /// </summary>
        /// <param name="pbonuses"></param>
        /// <returns></returns>
        public Bonus[,] ListToMass(List<Bonus> pbonuses)
        {
            Bonus[,] tbonus_mass = new Bonus[gb.W, gb.H];

            for (int i = 0; i < pbonuses.Count; i++)
            {
                tbonus_mass[pbonuses[i].X, pbonuses[i].Y] = pbonuses[i];
            }
            return tbonus_mass;
        }

        /// <summary>
        /// Перевести список в матрицу координат бонусов 
        /// </summary>
        /// <param name="pbonuses"></param>
        /// <returns></returns>
        public Player[,] ListToMass(List<Player> pplayers)
        {
            Player[,] tplayer_mass = new Player[gb.W, gb.H];

            for (int i = 0; i < pplayers.Count; i++)
            {
                tplayer_mass[pplayers[i].X, pplayers[i].Y] = pplayers[i];
            }
            return tplayer_mass;
        }



        /// <summary>
        /// Совершить действие, планируемое игроком (свойство ACTION), если оно возможно
        /// </summary>
        /// <param name="pplayer">Игрок, действие которого планируется выполнить</param>
        public void PlayerMove(Player pplayer)
        {
            Bomb[,] tbombs_mass = ListToMass(gb.Bombs);
            Player[,] tplayer_mass = ListToMass(gb.Players);

            switch (pplayer.ACTION)
            {
                case PlayerAction.Right:
                    if (pplayer.X + 1 > gb.W - 1)
                    {
                        break;
                    }
                    if (gb.Cells[pplayer.X + 1, pplayer.Y].Type == CellType.Destructible || gb.Cells[pplayer.X + 1, pplayer.Y].Type == CellType.Indestructible)
                    {
                        break;
                    }

                    if (tbombs_mass[pplayer.X + 1, pplayer.Y] != null)
                    {
                        break;
                    }
                    if (tplayer_mass[pplayer.X+1, pplayer.Y] != null)
                    {
                        break;
                    }


                    pplayer.X++;
                    break;

                case PlayerAction.Left:
                    if (pplayer.X - 1 < 0)
                    {
                        break;
                    }
                    if (gb.Cells[pplayer.X - 1, pplayer.Y].Type == CellType.Destructible || gb.Cells[pplayer.X - 1, pplayer.Y].Type == CellType.Indestructible)
                    {
                        break;
                    }

                    if (tbombs_mass[pplayer.X - 1, pplayer.Y] != null)
                    {
                        break;
                    }
                    if (tplayer_mass[pplayer.X - 1, pplayer.Y] != null)
                    {
                        break;
                    }

                    pplayer.X--;
                    break;

                case PlayerAction.Down:
                    if (pplayer.Y + 1 > gb.H - 1)
                    {
                        break;
                    }
                    if (gb.Cells[pplayer.X, pplayer.Y + 1].Type == CellType.Destructible || gb.Cells[pplayer.X, pplayer.Y + 1].Type == CellType.Indestructible)
                    {
                        break;
                    }

                    if (tbombs_mass[pplayer.X, pplayer.Y + 1] != null)
                    {
                        break;
                    }
                    if (tplayer_mass[pplayer.X, pplayer.Y + 1] != null)
                    {
                        break;
                    }

                    pplayer.Y++;
                    break;

                case PlayerAction.Up:
                    if (pplayer.Y - 1 < 0)
                    {
                        break;
                    }
                    if (gb.Cells[pplayer.X, pplayer.Y - 1].Type == CellType.Destructible || gb.Cells[pplayer.X, pplayer.Y - 1].Type == CellType.Indestructible)
                    {
                        break;
                    }

                    if (tbombs_mass[pplayer.X, pplayer.Y - 1] != null)
                    {
                        break;
                    }

                    if (tplayer_mass[pplayer.X, pplayer.Y - 1] != null)
                    {
                        break;
                    }

                    pplayer.Y--;
                    break;

                case PlayerAction.Bomb:
                    if (pplayer.BombsCount > 0)
                    {
                        CreateBomb(pplayer);
                        pplayer.BombsCount--;
                    }
                    break;

                case PlayerAction.Wait:
                    break;
            }
        }

        /// <summary>
        /// Выстрел игрока по возможности
        /// </summary>
        /// <param name="pplayer"></param>
        public void PlayerFire(Player pplayer)
        {
            if (pplayer.ACTION == PlayerAction.Bomb)
            {
                if (pplayer.BombsCount > 0)
                {
                    CreateBomb(pplayer);
                    pplayer.BombsCount--;
                }
            }
        }




        /// <summary>
        /// Создать бомбу на месте игрока
        /// </summary>
        /// <param name="_player">Игрок, создающий бомбу</param>
        public void CreateBomb(Player _player)
        {
            Bomb tbomb = new Bomb();

            tbomb.LiveTime = Config.bomb_live_time;
            tbomb.PlayerID = _player.ID;
            tbomb.X = _player.X;
            tbomb.Y = _player.Y;
            tbomb.Bang_radius = _player.BangRadius;

            gb.Bombs.Add(tbomb);
        }



        /// <summary>
        /// Обработать состояния бомб
        /// </summary>
        public void BombsProccess()
        {
            for (int i = 0; i < gb.Bombs.Count; i++)
            {
                var tbomb = gb.Bombs[i];
                if (tbomb.LiveTime < 1)
                {
                    // gb.Players.Find(c => c.ID == tbomb.PlayerID).BombsCount++;

                    for (int j = 0; j < gb.Players.Count; j++)
                    {
                        if (gb.Players[i].Health < 1)
                        {
                            continue;
                        }

                        if (gb.Players[j].ID == tbomb.PlayerID)
                        {
                            gb.Players[j].BombsCount++;
                            break;
                        }
                    }

                    GenerateLava(tbomb);
                    gb.Bombs.Remove(tbomb);
                    continue;
                }

                tbomb.LiveTime--;
            }
        }


        /// <summary>
        /// Зачислить поинты за убийство другого игрока
        /// </summary>
        /// <param name="plava">Лава, убившая другого игрока</param>
        public void PlayerAddPointsKill(Lava plava)
        {
            for (int i = 0; i < gb.Players.Count; i++)
            {
                var tplayer = gb.Players[i];
                if (tplayer.ID != plava.PlayerID)
                {
                    //  tplayer.Points += Config.player_kill_points;
                    break;
                }
            }
        }


        /// <summary>
        /// Зачислить поинты за разрушение стены
        /// </summary>
        /// <param name="plava">Лава, разрушившая стену</param>
        public void PlayerAddPointsCellDestroy(Lava plava)
        {
            for (int i = 0; i < gb.Players.Count; i++)
            {
                var tplayer = gb.Players[i];
                if (tplayer.ID == plava.PlayerID)
                {
                    tplayer.Points += Config.player_cell_destroy_points;
                    break;
                }
            }
        }

        /// <summary>
        /// Игрок умер
        /// </summary>
        /// <param name="pplayer"></param>
        /// <param name="plava"></param>
        public void PlayerDeath(Player pplayer)
        {
            //if (plava.PlayerID != pplayer.ID)
            //{
            //    PlayerAddPointsKill(plava);
            //}
            pplayer.Health--;

            if (pplayer is User && pplayer.Health < 1)
            {
                PlayerDisconnect(pplayer);
            }
        }


        /// <summary>
        /// Отключение игрока-клиента
        /// </summary>
        /// <param name="pclient"></param>
        public void PlayerDisconnect(Player pplayer)
        {
            try
            {
                TcpClient tempclient = usersInfo.Find(c => c.player == pplayer).client;
                if (tempclient != null)
                {
                    tempclient.Close();
                    tempclient = null;
                }
                usersInfo.Find(c => c.player == pplayer).client = null;

            }
            catch (Exception e)
            {
                Helper.LOG(Compiler.LogPath, "Ошибка в функции PlayerDisconnect: " + e.Message);
            }
        }

        /// <summary>
        /// Отключение всех игроков-клиентов
        /// </summary>
        /// <param name="pclient"></param>
        public void Disconnect()
        {
            for (int i = 0; i < usersInfo.Count; i++)
            {
                if (usersInfo[i].client != null)
                {
                    usersInfo[i].client.Close();
                }
            }
        }



        /// <summary>
        /// Взаимодействие лавы и игроков
        /// </summary>
        /// <param name="plava"></param>
        /// <param name="i"></param>
        public void LavaPlayersCollision(Lava plava)
        {
            for (int k = 0; k < gb.Players.Count; k++)
            {
                var tplayer = gb.Players[k];

                if (tplayer.Health > 0 && tplayer.X == plava.X && tplayer.Y == plava.Y)
                {
                    DestroyedPlayers.Add(tplayer);

                    for (int i = 0; i < gb.Players.Count; i++)
                    {
                        if (plava.PlayerID == gb.Players[i].ID && tplayer.ID != gb.Players[i].ID)
                        {
                            gb.Players[i].Points += Config.player_kill_points;
                        }
                    }
                }
                //  PlayerDeath(tplayer);
            }
        }






        /// <summary>
        /// Обработка состояния лавы
        /// </summary>
        public void LavasProccess()
        {
            for (int k = 0; k < gb.Lavas.Count; k++)
            {
                var tlava = gb.Lavas[k];

                LavaPlayersCollision(tlava);
                LavaCellsBonusesCollision(tlava);

                if (tlava.LiveTime < 1)
                {
                    gb.Lavas.Remove(tlava);
                    k--;
                }
                tlava.LiveTime--;
            }
        }




        /// <summary>
        /// Взаимодействие лавы со стенами и бонусами под стенами
        /// </summary>
        /// <param name="plava">Объект Лава</param>
        public void LavaCellsBonusesCollision(Lava plava)
        {
            Bonus[,] tbonuses_mass = ListToMass(gb.Bonuses);

            if (gb.Cells[plava.X, plava.Y].Type == CellType.Destructible)
            {
                // gb.Cells[plava.X, plava.Y].Type = CellType.Free;
                DestroyedCells.Add(gb.Cells[plava.X, plava.Y]);
                // gb.Players.Find(c => c.ID == plava.PlayerID).Points += Config.player_cell_destroy_points;

                for (int i = 0; i < gb.Players.Count; i++)
                {
                    if (gb.Players[i].ID == plava.PlayerID)
                    {
                        gb.Players[i].Points += Config.player_cell_destroy_points;
                    }
                }

                if (tbonuses_mass[plava.X, plava.Y] != null)
                {
                    tbonuses_mass[plava.X, plava.Y].Visible = true;
                }

                //   PlayerAddPointsCellDestroy(plava);
            }
        }




        /// <summary>
        /// Нарисовать стены
        /// </summary>
        public void DrawCells()
        {
            for (int i = 0; i < gb.Cells.GetLength(0); i++)
            {
                for (int j = 0; j < gb.Cells.GetLength(1); j++)
                {
                    var tcell = gb.Cells[i, j];

                    if (tcell.Type == CellType.Destructible)
                    {
                        PaintRect(tcell.X, tcell.Y, Config.cell_destructible_color);
                    }
                    else if (tcell.Type == CellType.Indestructible)
                    {
                        PaintRect(tcell.X, tcell.Y, Config.cell_indestructible_color);
                    }
                }
            }
        }


        /// <summary>
        /// Создать лаву на основе информации о бомбе, её породившую
        /// </summary>
        /// <param name="_bomb"></param>
        public void GenerateLava(Bomb _bomb)
        {
            int tradius = _bomb.Bang_radius;

            for (int i = _bomb.X + 1; i <= _bomb.X + tradius; i++)
            {
                if (i < 0 || i > gb.W - 1 || gb.Cells[i, _bomb.Y].Type == CellType.Indestructible)
                {
                    break;
                }

                Lava tlava = new Lava()
                {
                    X = i,
                    Y = _bomb.Y,
                    LiveTime = Config.lava_livetime,
                    PlayerID = _bomb.PlayerID
                };
                gb.Lavas.Add(tlava);
            }


            for (int i = _bomb.X; i >= _bomb.X - tradius; i--)
            {
                if (i < 0 || i > gb.W - 1 || gb.Cells[i, _bomb.Y].Type == CellType.Indestructible)
                {
                    break;
                }

                Lava tlava = new Lava()
                {
                    X = i,
                    Y = _bomb.Y,
                    LiveTime = Config.lava_livetime,
                    PlayerID = _bomb.PlayerID
                };
                gb.Lavas.Add(tlava);
            }


            for (int j = _bomb.Y + 1; j <= _bomb.Y + tradius; j++)
            {
                if (j < 0 || j > gb.H - 1 || gb.Cells[_bomb.X, j].Type == CellType.Indestructible)
                {
                    break;
                }

                Lava tlava = new Lava()
                {
                    X = _bomb.X,
                    Y = j,
                    LiveTime = Config.lava_livetime,
                    PlayerID = _bomb.PlayerID
                };
                gb.Lavas.Add(tlava);
            }


            for (int j = _bomb.Y - 1; j >= _bomb.Y - tradius; j--)
            {
                if (j < 0 || j > gb.H - 1 || gb.Cells[_bomb.X, j].Type == CellType.Indestructible)
                {
                    break;
                }

                Lava tlava = new Lava()
                {
                    X = _bomb.X,
                    Y = j,
                    LiveTime = Config.lava_livetime,
                    PlayerID = _bomb.PlayerID
                };
                gb.Lavas.Add(tlava);
            }
        }


        /// <summary>
        /// Нарисовать бомбу
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="cl"></param>
        public void PaintBomb(int x, int y, Color cl)
        {
            sb = new SolidBrush(cl);
            g.FillEllipse(new SolidBrush(cl), x * cw + cw / 4, y * cw + cw / 4, cw - cw / 2, cw - cw / 2);
        }

        /// <summary>
        /// Нариросовать эллипс
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="cl"></param>
        public void PaintEllipse(int x, int y, Color cl)
        {
            sb = new SolidBrush(cl);
            g.FillEllipse(new SolidBrush(cl), x * cw, y * cw, cw, cw);
        }


        /// <summary>
        /// Нариросовать эллипс
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="cl"></param>
        public void PaintPlayer(int x, int y, Color cl)
        {
            sb = new SolidBrush(cl);
            float zoomed = Convert.ToSingle(0.7);
            g.FillEllipse(new SolidBrush(Color.Black), x * cw, y * cw, cw, cw);

            g.FillEllipse(new SolidBrush(cl), x * cw + cw / 10, y * cw + cw / 10, cw - cw / 5, cw - cw / 5);
        }


        /// <summary>
        /// нарисовать квадрат
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="cl"></param>
        public void PaintRect(int x, int y, Color cl)
        {
            sb = new SolidBrush(cl);
            g.DrawRectangle(p, x * cw, y * cw, cw, cw);
            g.FillRectangle(sb, x * cw, y * cw, cw, cw);
        }


        /// <summary>
        /// Нарисовать сетку
        /// </summary>
        public void DrawGrid()
        {
            int w = gb.Cells.GetLength(0);
            int h = gb.Cells.GetLength(0);

            for (int i = 0; i < w; i++)
            {
                g.DrawLine(p, i * cw, 0, i * cw, w * cw);
            }
            for (int j = 0; j < h; j++)
            {
                g.DrawLine(p, 0, j * cw, h * cw, j * cw);
            }
        }








        private void log_box_TextChanged(object sender, EventArgs e)
        {
            log_box.SelectionStart = log_box.Text.Length;
            log_box.ScrollToCaret();
        }


        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            StopClearTempFiles();

            startPage.Show();
        }





        /// <summary>
        /// Попытаться скомпилировать и запустить стратегию пользователя
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Compiler CompileAndStartUserFiles(string path, int i)
        {
            try
            {
                Compiler compiler = new Compiler(path, i);
                compiler.Compile();
                Thread.Sleep(100);
                compiler.UserClientStart();

                return compiler;
            }
            catch (Exception e)
            {
                Helper.LOG(Compiler.LogPath, "ERROR in CompileAndStartUserFiles: " + e.Message + " " + e.StackTrace);
                return null;
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }



        /// <summary>
        /// Пауза
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void control_btn_Click(object sender, EventArgs e)
        {
            game_timer.Enabled = !game_timer.Enabled;
            if (game_timer.Enabled)
            {
                control_btn.Text = "Пауза";
            }
            else
            {
                control_btn.Text = "Продолжить";
            }

        }

        /// <summary>
        /// Увеличение скорости игры 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fast_btn_Click(object sender, EventArgs e)
        {
            game_timer.Interval = game_timer.Interval / 2;
            slow_btn.Enabled = true;

            if (game_timer.Interval < 110)
            {
                fast_btn.Enabled = false;
                game_timer.Interval = 110;
            }
            else
            {
                fast_btn.Enabled = true;
            }
        }


        /// <summary>
        /// Замедление скорости игры
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slow_btn_Click(object sender, EventArgs e)
        {
            game_timer.Interval = game_timer.Interval * 2;

            fast_btn.Enabled = true;

            if (game_timer.Interval > 6000)
            {
                slow_btn.Enabled = false;
                game_timer.Interval = 6000;
            }
            else
            {
                slow_btn.Enabled = true;
            }
        }



        private void log_box_DoubleClick(object sender, EventArgs e)
        {
            log_box.Clear();
        }


        public void formLog(string message)
        {
            log_box.AppendText(message + "\n");
        }

    }
}
