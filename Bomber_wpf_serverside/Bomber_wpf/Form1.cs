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
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

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
        int GameTimer;

        string serverIp = "127.0.0.1";
        TcpListener server;
        Dictionary<Player, TcpClient> clients = new Dictionary<Player, TcpClient>();
        List<PlayerTimeout> players_timeout = new List<PlayerTimeout>();
        List<GameBoard> gameBoardStates = new List<GameBoard>();
        List<GameBoard> savedGameBoardStates = new List<GameBoard>();
        int visualizeGameCadrNumber = 0;
        int TimeOut = 5000;

        Color[] player_colors = new Color[4]
        {
            Color.PaleVioletRed, Color.Green, Color.HotPink, Color.Aqua
        };

        bool isGameOver = false;

        Random rn = new Random();




        /// <summary>
        /// Игра в реальном времени
        /// </summary>
        /// <param name="pstartPage"></param>
        public Form1(StartPage pstartPage)
        {
            InitializeComponent();
            g = panel1.CreateGraphics();
            p = new Pen(Color.Black);
            sb = new SolidBrush(Color.DimGray);

            IPAddress ip = IPAddress.Parse(serverIp);
            server = new TcpListener(ip, 9595);
            server.Start();
            startPage = pstartPage;

            InitGame();
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


        void CheckUserCodeSourcesPath(out int clients_count)
        {
            clients_count = 0;


            for (int i = 0; i < startPage.paths.Length; i++)
            {
                InitPlayersInfo(i);
                if (startPage.paths[i] != null && startPage.paths[i] != "")
                {
                    string tfilename = SpliteEndPath(startPage.paths[i]);
                    CompileAndStartUserFiles(tfilename);
                    clients_count++;
                }
            }
        }

        /// <summary>
        /// Выделить из Пути файла имя этого Файла
        /// </summary>
        /// <param name="ppath">Полный путь до файла</param>
        /// <returns>Имя файла</returns>
        public string SpliteEndPath(string ppath)
        {
            Stack<char> tsymbols = new Stack<char>();
            string nfileName = "";

            int tindex = ppath.Length - 1;

            for (; tindex >= 0; tindex--)
            {
                if (ppath[tindex] == '.')
                {
                    break;
                }
            }

            tindex--;

            for (; tindex >= 0; tindex--)
            {

                if (ppath[tindex] == '\\')
                {
                    break;
                }
                tsymbols.Push(ppath[tindex]);
            }

            while (tsymbols.Count > 0)
            {
                nfileName += tsymbols.Pop();
            }
            return nfileName;
        }


        void InitPlayersInfo(int i)
        {
            string[] ppaths = startPage.paths;
            Player pplayer;

            if (ppaths[i] == null)
            {
                return;
            }

            if (ppaths[i] == "")
            {
                pplayer = new Bot();
                pplayer.Name = "Bot_" + (i + 1);
            }
            else
            {
                pplayer = new User();
                pplayer.Name = "User_" + (i + 1);
            }                        

            pplayer.ID = CalculateMD5Hash(DateTime.Now.Millisecond * rn.NextDouble() + "JOPA" + pplayer.Name);
            pplayer.Color = player_colors[i];
            pplayer.Points = 0;
            //    pplayer.Health = Config.player_health;
            pplayer.Health = 1;
            pplayer.Bang_radius = Config.bang_start_radius;
            pplayer.BombsCount = Config.player_bombs_count_start;
            pplayer.Bang_radius = 3;

            switch (i)
            {
                case 0:
                    pplayer.X = 0;
                    pplayer.Y = 0;
                    break;
                case 1:
                    pplayer.X = gb.W - 1;
                    pplayer.Y = 0;
                    break;
                case 2:
                    pplayer.X = 0;
                    pplayer.Y = gb.H - 1;
                    break;
                case 3:
                    pplayer.X = gb.W - 1;
                    pplayer.Y = gb.H - 1;
                    break;
            }
            gb.Players.Add(pplayer);
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
            GameTimer = savedGameBoardStates.Count;
            visualizeGameCadrNumber = 0;
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
            visualizeGameCadrNumber = savedGameBoardStates.Count - GameTimer;
            gb = savedGameBoardStates[visualizeGameCadrNumber];

            panel1.Refresh();
            UpdateListView();

            this.Text = "Тик - " + GameTimer;
            DrawAll();

            CheckVisualizingGameOver();
            GameTimer--;
        }

        /// <summary>
        /// Проверка условия окончания воспроизведения игры
        /// </summary>
        public void CheckVisualizingGameOver()
        {
            if (GameTimer <= 1)
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
        public void InitGame()
        {
            clients.Clear();
            players_timeout.Clear();
            gameBoardStates.Clear();

            isGameOver = false;

            GameTimer = Config.gameTicksMax;
            gb = new GameBoard();
            int tconnected_clients_count;

            CheckUserCodeSourcesPath(out tconnected_clients_count);

            //Bomb test = new Bomb_big
            //{
            //    X = 0,
            //    Y = 1,                
            //    Color = Color.Red,
            //    LiveTime = 3,
            //    PlayerID = 1
            //};
            //gb.Bombs.Add(test);

            Bonus_big test = new Bonus_big(0, 1);
            test.Visible = true;
            Bonus_big test1 = new Bonus_big(1, 0);
            test1.Visible = true;



            gb.Bonuses.Add(test);
            gb.Bonuses.Add(test1);

            LogUpdate(gb.Bonuses.Count + "");








            int tplayers_index = 0;

            while (clients.Count < tconnected_clients_count)
            {
                //  MessageBox.Show("Ожидаем клиентов в фоновом режиме");
                if (gb.Players[tplayers_index] is User)
                {
                    clients.Add(gb.Players[tplayers_index], server.AcceptTcpClient());
                    players_timeout.Add( new PlayerTimeout(gb.Players[tplayers_index]));
                }
                tplayers_index++;
                //  MessageBox.Show("Клиент подключился");
            }


          //  SendGameInfo();

            gameBoardStates.Add((GameBoard)gb.Clone());

            game_timer.Tick += game_timer_Tick;
            game_timer.Interval = 800;
            game_timer.Start();

            initListView();
        }


        /// <summary>
        /// Отправить Клиентам инофрмацию об Игровом мире (объект класса GameBoard)
        /// </summary>
        public void SendGameInfo()
        {
            try
            {

                foreach (var tclient in clients)
                {
                    try
                    {
                        if (tclient.Value != null)
                        {
                            NetworkStream strm = tclient.Value.GetStream();
                            IFormatter formatter = new BinaryFormatter();
                            formatter.Serialize(strm, gb);
                            formatter.Serialize(strm, tclient.Key);                            
                        }
                    }
                    catch
                    {
                        PlayerDisconnect(tclient.Value);
                    }
                }
            }
            catch (Exception e)
            {
                LogUpdate("SendGameInfo Error " + e.Message);
            }
        }


        /// <summary>
        /// Получить информацию об Игроках (класс Player) от Клиентов
        /// </summary>
        public void RecieveUserInfo()
        {
            try
            {
                foreach (var tclient in clients)
                {
                    try
                    {
                        if (tclient.Value != null)
                        {
                            NetworkStream strm = tclient.Value.GetStream();
                            IFormatter formatter = new BinaryFormatter();

                            PlayerTimeout tplayer = players_timeout.Find(c => c.Player.ID == tclient.Key.ID);
                            if (tplayer.Timeout > 120)
                            {
                                throw new TimeoutException(tclient.Key.Name + " превысил общее допустимое время на все ходы");
                            }

                            byte[] sdata = new byte[4];
                            strm.Read(sdata, 0, sdata.Length);
                            string start = Encoding.ASCII.GetString(sdata);

                            if (start != "s")
                            {
                                throw new Exception("Неверное начально сообщения");
                            }

                            byte[] data = new byte[4];

                            IAsyncResult ar = strm.BeginRead(data, 0, data.Length, null, null);

                            WaitHandle wh = ar.AsyncWaitHandle;

                            try
                            {
                                if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(TimeOut), false))
                                {
                                    tplayer.Timeout += TimeOut;
                                    throw new TimeoutException(tclient.Key.Name + " превысил допустимое время на один ход");
                                }

                                strm.EndRead(ar);
                            }
                            catch (TimeoutException e)
                            {                               
                                throw new TimeoutException(e.Message);
                            }

                            finally
                            {
                                wh.Close();
                            }

                            string message = Encoding.ASCII.GetString(data);

                            if (message.Length<1 || message=="")
                            {
                                throw new Exception();
                            }

                            switch (message)
                            {
                                case "1":
                                    tclient.Key.ACTION = PlayerAction.Bomb;
                                    break;
                                case "2":
                                    tclient.Key.ACTION = PlayerAction.Down;
                                    break;
                                case "3":
                                    tclient.Key.ACTION = PlayerAction.Left;
                                    break;
                                case "4":
                                    tclient.Key.ACTION = PlayerAction.Right;
                                    break;
                                case "5":
                                    tclient.Key.ACTION = PlayerAction.Up;
                                    break;
                                default:
                                    tclient.Key.ACTION = PlayerAction.Wait;
                                    break;
                            }

                            //Thread thr = new Thread(() =>
                            //{
                            //    Player nplayer = (Player)formatter.Deserialize(strm);
                            //    tclient.Key.ACTION = nplayer.ACTION;
                            //});

                            //thr.Start();

                            //Thread.Sleep(2000);

                            //if (thr.ThreadState==ThreadState.Running)
                            //{

                            //}

                            //   gb.Players.Find(c => c.ID == nplayer.ID).ACTION = nplayer.ACTION;

                        }
                    }

                    catch (TimeoutException e)
                    {
                        LogUpdate(e.Message);
                        tclient.Key.ACTION = PlayerAction.Wait;
                    }
                    catch (Exception)
                    {
                        // PlayerDisconnect(tclient.Value);
                      //  LogUpdate("Игрок " + tclient.Key.Name + " слишком долго думал");
                        tclient.Key.ACTION = PlayerAction.Wait;
                    }
                }
            }
            catch
            {
                LogUpdate("ОШИБКА при перечислении списка Юзеров");
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
                    tplayer.Bang_radius.ToString(),
                    tplayer.X.ToString(),
                    tplayer.Y.ToString()
                });
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
                    tplayer.Bang_radius.ToString(),
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
                PaintEllipse(tbonus.X, tbonus.Y, tbonus.Color);
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
                    PaintPlayer(tplayer.X, tplayer.Y, tplayer.Color);
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
                PaintBomb(tbomb.X, tbomb.Y, tbomb.Color);
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
            PlayerProcess();
            PlayerBonusCollision();

            BombsProccess();
            LavasProccess();
            LavaCollision();
        }



        /// <summary>
        /// Следующий ход игры
        /// </summary>
        public void NextTick()
        {
            if (isGameOver == false)
            {
                

                panel1.Refresh();
                UpdateListView();

                this.Text = "Тик - " + GameTimer;

                GameProccess();
                DrawAll();

                gameBoardStates.Add((GameBoard)gb.Clone());

                CheckGameOver();

                SetXYInfo();

                SendGameInfo();
                RecieveUserInfo();
            }
        }


        public void panel1_Paint(object sender, PaintEventArgs e)
        {

        }



        /// <summary>
        /// Получить информацию о каждой клетке: какие объекты в ней сейчас расположены (Игроки, Бомбы, Видимые Бонусы, Лава (с наибольшим LiveTime)
        /// </summary>
        public void SetXYInfo()
        {
            for (int i = 0; i < gb.XYinfo.GetLength(0); i++)
            {
                for (int j = 0; j < gb.XYinfo.GetLength(1); j++)
                {
                    var tXYInfo = gb.XYinfo[i, j];

                    for (int ii = 0; ii < gb.Players.Count; ii++)
                    {
                        if (gb.Players[ii].X == i && gb.Players[ii].Y == j)
                        {
                            tXYInfo.Player = gb.Players[ii];
                            break;
                        }
                    }


                    int tmaxLiveTime = 0;

                    for (int ii = 0; ii < gb.Lavas.Count; ii++)
                    {
                        if (gb.Lavas[ii].X == i && gb.Lavas[ii].Y == j)
                        {
                            if (gb.Lavas[ii].LiveTime >= tmaxLiveTime)
                            {
                                tmaxLiveTime = gb.Lavas[ii].LiveTime;
                                tXYInfo.Lava = gb.Lavas[ii];
                            }
                        }
                    }


                    for (int ii = 0; ii < gb.Bonuses.Count; ii++)
                    {
                        if (gb.Bonuses[ii].X == i && gb.Bonuses[ii].Y == j)
                        {
                            tXYInfo.Bonus = gb.Bonuses[ii];
                            break;
                        }
                    }

                    for (int ii = 0; ii < gb.Bombs.Count; ii++)
                    {
                        if (gb.Bombs[ii].X == i && gb.Bombs[ii].Y == j)
                        {
                            tXYInfo.Bomb = gb.Bombs[ii];
                            break;
                        }
                    }


                }
            }
        }

        /// <summary>
        /// Окончание игры: вывод и сохранение информации
        /// </summary>
        public void GameOver(List<Player> winners)
        {
            isGameOver = true;

            Disconnect();
            server.Stop();

            game_timer.Stop();

            Thread.Sleep(100);
            Compiler.DeleteComppiledFiles();

            var result = DialogResult.No;
            string message = "";

           winners.Sort((a, b) => a.Health.CompareTo(b.Health));

            if (GameTimer<1)
            {
                message += "ВРЕМЯ И СТЕКЛО \n";
            }

            message += "Игроки: \n";

            for (int i = 0; i < gb.Players.Count; i++)
            {
                message += gb.Players[i].Name + ": " + gb.Players[i].Points + " (баллы) \n";
            }

            gameBoardStates.Add(gb);

            SaveGameInfoFile();

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
            if (isGameOver == false)
            {
                List<Player> winners = gb.Players.FindAll(c => c.Health > 0);

                if (GameTimer < 1 || winners.Count<2)
                {
                    GameOver(winners);
                }
            }
        }

        /// <summary>
        /// Сохранение "слепков" игры в виде списка, где индекс - это номер Тика игры
        /// </summary>
        public void SaveGameInfoFile()
        {
            //try
            //{
            string gameResultDirectoryName = Directory.GetCurrentDirectory() + "\\" + GetInfoAboutThisGame() + "\\";

            Compiler.DeleteDirectory(gameResultDirectoryName);

            Directory.CreateDirectory(gameResultDirectoryName);


            string gameStaterVisualizerFileName = "Visualizer.dat";
            string gameResultsFileName = "gameResults.json";

            BinaryFormatter form = new BinaryFormatter();
            using (FileStream fs = new FileStream(gameResultDirectoryName + gameStaterVisualizerFileName, FileMode.OpenOrCreate))
            {
                form.Serialize(fs, gameBoardStates);
            }

            StreamWriter sw = new StreamWriter(gameResultDirectoryName + gameResultsFileName, false);
            string GameResultsJson = JsonConvert.SerializeObject(GetPlayerResult());
            sw.Write(GameResultsJson);
            sw.Close();

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

            for (int i = 0; i < tempplayers.Count; i++)
            {
                var tvitya1 = tempplayers[i];
                for (int j = i + 1; j < tempplayers.Count; j++)
                {
                    var tvitya2 = tempplayers[j];
                    if (tvitya1.X == tvitya2.X && tvitya1.Y == tvitya2.Y)
                    {
                        gb.Players[i].ACTION = PlayerAction.Wait;
                        gb.Players[j].ACTION = PlayerAction.Wait;
                    }
                    if (tvitya2.X == gb.Players[i].X && tvitya2.Y == gb.Players[i].Y && tvitya1.X == gb.Players[i].X && tvitya1.Y == gb.Players[i].Y)
                    {
                        gb.Players[i].ACTION = PlayerAction.Wait;
                        gb.Players[j].ACTION = PlayerAction.Wait;
                    }
                }
            }

            for (int i = 0; i < gb.Players.Count; i++)
            {
                if (gb.Players[i].Health > 0)
                {
                    PlayerMove(gb.Players[i]);
                }

                gb.Players[i].ACTION = PlayerAction.Wait;
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

                    if (tempbonus[i, j] is Bonus_big)
                    {
                        tvitya.Bang_radius++;
                    }
                    else if (tempbonus[i, j] is Bonus_fast)
                    {
                        tvitya.BombsCount++;
                    }
                    gb.Bonuses.Remove(tempbonus[i, j]);
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
        /// Перевести список в матрицу координат игроков
        /// </summary>
        /// <param name="pplayers"></param>
        /// <returns></returns>
        public Player[,] ListToMass(List<Player> pplayers)
        {
            Player[,] tplayers_mass = new Player[gb.W, gb.H];

            for (int i = 0; i < pplayers.Count; i++)
            {

                tplayers_mass[pplayers[i].X, pplayers[i].Y] = pplayers[i];
            }
            return tplayers_mass;
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
        /// Совершить действие, планируемое игроком (свойство ACTION), если оно возможно
        /// </summary>
        /// <param name="pplayer">Игрок, действие которого планируется выполнить</param>
        public void PlayerMove(Player pplayer)
        {
            Bomb[,] tbombs_mass = ListToMass(gb.Bombs);
            switch (pplayer.ACTION)
            {
                case PlayerAction.Right:
                    if (pplayer.X + 1 > gb.W - 1)
                    {
                        break;
                    }
                    if (gb.Cells[pplayer.X + 1, pplayer.Y] is Cell_destructible || gb.Cells[pplayer.X + 1, pplayer.Y] is Cell_indestructible)
                    {
                        break;
                    }

                    if (tbombs_mass[pplayer.X + 1, pplayer.Y] != null)
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
                    if (gb.Cells[pplayer.X - 1, pplayer.Y] is Cell_destructible || gb.Cells[pplayer.X - 1, pplayer.Y] is Cell_indestructible)
                    {
                        break;
                    }

                    if (tbombs_mass[pplayer.X - 1, pplayer.Y] != null)
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
                    if (gb.Cells[pplayer.X, pplayer.Y + 1] is Cell_destructible || gb.Cells[pplayer.X, pplayer.Y + 1] is Cell_indestructible)
                    {
                        break;
                    }

                    if (tbombs_mass[pplayer.X, pplayer.Y + 1] != null)
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
                    if (gb.Cells[pplayer.X, pplayer.Y - 1] is Cell_destructible || gb.Cells[pplayer.X, pplayer.Y - 1] is Cell_indestructible)
                    {
                        break;
                    }

                    if (tbombs_mass[pplayer.X, pplayer.Y - 1] != null)
                    {
                        break;
                    }

                    pplayer.Y--;
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




        ///// <summary>
        ///// Перезарядка игроков
        ///// </summary>
        ///// <param name="_player"></param>
        //public void PlayerReload(Player _player)
        //{
        //    if (_player.BonusType == BonusType.Ammunition || _player.BonusType == BonusType.All)
        //    {
        //        _player.BombsCount = Config.player_bombs_count_max;
        //    }
        //    else
        //    {
        //        _player.BombsCount = Config.player_bombs_count_start;
        //    }
        //}



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
            tbomb.Bang_radius = _player.Bang_radius;

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
                        if (gb.Players[i].Health<1)
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
                 //   tplayer.Points += Config.player_kill_points;
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
        /// Игрок умер в лаве
        /// </summary>
        /// <param name="pplayer"></param>
        /// <param name="plava"></param>
        public void PlayerKill(Player pplayer)
        {
            //if (plava.PlayerID != pplayer.ID)
            //{
            //    PlayerAddPointsKill(plava);
            //}
            pplayer.Health = 0;

            if (pplayer is User)
            {
                PlayerDisconnect(clients[pplayer]);
            }

            clients[pplayer] = null;

            //   PlayerDeath(pplayer);
        }

        /// <summary>
        /// Похороны игрока
        /// </summary>
        /// <param name="pplayer"></param>
        public void PlayerDeath(Player pplayer)
        {
            gb.Players.Remove(pplayer);
        }


        /// <summary>
        /// Отключение игрока-клиента
        /// </summary>
        /// <param name="pclient"></param>
        public void PlayerDisconnect(TcpClient pclient)
        {
            if (pclient != null)
            {
                pclient.Close();
            }
            try
            {
                foreach (var tclient in clients)
                {
                    if (tclient.Value == pclient)
                    {
                        clients[tclient.Key] = null;
                    }
                }
            }
            catch (Exception e)
            {
                LogUpdate("Ошибка в функции PlayerDisconnect: " + e.Message);
            }
        }

        /// <summary>
        /// Отключение всех игроков-клиентов
        /// </summary>
        /// <param name="pclient"></param>
        public void Disconnect()
        {
            foreach (var tclient in clients)
            {
                if (tclient.Value != null)
                {
                    tclient.Value.Close();
                }
            }
            clients.Clear();
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

                if (tplayer.Health>0 && tplayer.X == plava.X && tplayer.Y == plava.Y)
                {
                    PlayerKill(tplayer);
                }
            }
        }


        /// <summary>
        /// Взаимодействие лавы с остальными объектами
        /// </summary>
        /// <param name="plava">Лава</param>
        public void LavaCollision()
        {
            for (int i = 0; i < gb.Lavas.Count; i++)
            {
                var tlava = gb.Lavas[i];

                LavaPlayersCollision(tlava);
                LavaCellsBonusesCollision(tlava);
            }
        }


        /// <summary>
        /// Взаимодействие лавы со стенами и бонусами под стенами
        /// </summary>
        /// <param name="plava">Объект Лава</param>
        public void LavaCellsBonusesCollision(Lava plava)
        {
            Bonus[,] tbonuses_mass = ListToMass(gb.Bonuses);

            if (gb.Cells[plava.X, plava.Y] is Cell_destructible)
            {
                gb.Cells[plava.X, plava.Y] = new Cell_free()
                {
                    X = plava.X,
                    Y = plava.Y
                };

                if (tbonuses_mass[plava.X, plava.Y] != null)
                {
                    tbonuses_mass[plava.X, plava.Y].Visible = true;
                }

             //   PlayerAddPointsCellDestroy(plava);
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

                if (tlava.LiveTime < 1)
                {
                    gb.Lavas.Remove(tlava);
                    k--;
                }
                tlava.LiveTime--;
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

                    if (tcell is Cell_indestructible)
                    {
                        PaintRect(tcell.X, tcell.Y, tcell.Color);
                    }
                    else if (tcell is Cell_destructible)
                    {
                        PaintRect(tcell.X, tcell.Y, tcell.Color);
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
                if (i < 0 || i > gb.W - 1 || gb.Cells[i, _bomb.Y] is Cell_indestructible)
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
                if (i < 0 || i > gb.W - 1 || gb.Cells[i, _bomb.Y] is Cell_indestructible)
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
                if (j < 0 || j > gb.H - 1 || gb.Cells[_bomb.X, j] is Cell_indestructible)
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
                if (j < 0 || j > gb.H - 1 || gb.Cells[_bomb.X, j] is Cell_indestructible)
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


        ///// <summary>
        ///// Создать лаву, на месте взрыва бомбы
        ///// </summary>
        ///// <param name="x">Координата бомбы</param>
        ///// <param name="y">Координата бомбы</param>
        //public void GenerateLava(int x, int y)
        //{
        //    Lava tlava = new Lava()
        //    {
        //        X = x,
        //        Y = y,
        //        Radius = Config.lava_radius_big,
        //        LiveTime = Config.lava_livetime
        //    };
        //    gb.Lavas.Add(tlava);
        //}


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





        /// <summary>
        /// Добавить информацию в лог
        /// </summary>
        /// <param name="message"></param>
        public void LogUpdate(string message)
        {
            string time = DateTime.Now.ToString("dd-MM-yyyy H-mm-ss");
            time = "[" + time + "] ";

            log_box.Text += time + message + Environment.NewLine;
        }


        private void log_box_TextChanged(object sender, EventArgs e)
        {
            log_box.SelectionStart = log_box.Text.Length;
            log_box.ScrollToCaret();
        }


        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Disconnect();
            Thread.Sleep(100);
            Compiler.DeleteComppiledFiles();

            if (server != null)
            {
                server.Stop();
            }

            startPage.Show();
        }


        public string CalculateMD5Hash(string input)
        {

            MD5 md5 = MD5.Create();

            byte[] inputBytes = Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)

            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }



        void CompileAndStartUserFiles(string path)
        {
            Compiler compiler = new Compiler(path);
            compiler.Compile();

            Thread.Sleep(1000);
            compiler.UserClientStart();
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

            if (game_timer.Interval < 100)
            {
                fast_btn.Enabled = false;
                game_timer.Interval = 100;
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
    }
}