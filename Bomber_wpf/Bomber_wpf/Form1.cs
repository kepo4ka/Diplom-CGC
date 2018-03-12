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
        List<GameBoard> gameBoardStates = new List<GameBoard>();
        List<GameBoard> savedGameBoardStates = new List<GameBoard>();
        int visualizeGameCadrNumber = 0;

        Color[] player_colors = new Color[4]
        {
            Color.PaleVioletRed, Color.Green, Color.HotPink, Color.Aqua
        };

        bool isGameOver = false;

        bool testbool = false;

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

            for (; tindex >=0; tindex--)
            {
                if (ppath[tindex]=='.')
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
                pplayer.Name = "bot_" + (i + 1);
            }
            else
            {
                pplayer = new User();                
                pplayer.Name = "User_" + (i + 1);
            }
            

            pplayer.ID = i;
            pplayer.Color = player_colors[i];
            pplayer.Points = 0;
            //    pplayer.Health = Config.player_health;
            pplayer.Health = 15;
            pplayer.BonusType = BonusType.None;
            pplayer.BombsCount = Config.player_bombs_count_start;

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
            if (GameTimer<=1)
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

            int tplayers_index = 0;

            while (clients.Count < tconnected_clients_count)
            {
                //  MessageBox.Show("Ожидаем клиентов в фоновом режиме");
                if (gb.Players[tplayers_index] is User)
                {
                    clients.Add(gb.Players[tplayers_index], server.AcceptTcpClient());
                }
                tplayers_index++;
              //  MessageBox.Show("Клиент подключился");
            }

            SendGameInfo();

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
                Dictionary<Player, TcpClient> tclients = clients;

                foreach (var tclient in tclients)
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
                        PlayerDeath(tclient.Key);
                        PlayerDisconnect(tclient.Value);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("SendGameInfo Error " + e.Message);
            }
        }


        /// <summary>
        /// Получить информацию об Игроках (класс Player) от Клиентов
        /// </summary>
        public void ReceiveUserInfo()
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
                            Player nplayer = (Player)formatter.Deserialize(strm);
                            gb.Players.Find(c => c.ID == nplayer.ID).ACTION = nplayer.ACTION;
                        }
                    }
                    catch (Exception e)
                    {
                        PlayerDeath(tclient.Key);
                        PlayerDisconnect(tclient.Value);
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
            dead_players_listvView.Items.Clear();

            for (int i = 0; i < gb.Players.Count; i++)
            {
                var tplayer = gb.Players[i];

                var item = new ListViewItem(new[] {
                    tplayer.Name, tplayer.ID.ToString(),
                    tplayer.Health.ToString(),
                    tplayer.Points.ToString(),
                    tplayer.ACTION.ToString(),
                    tplayer.BombsCount.ToString(),
                    tplayer.BonusType.ToString(),
                    tplayer.X.ToString(),
                    tplayer.Y.ToString()
                });
                players_listView.Items.Add(item);
            }

            for (int i = 0; i < gb.DeadPlayers.Count; i++)
            {
                var tplayer = gb.DeadPlayers[i];

                var item = new ListViewItem(new[] {
                    tplayer.Name, tplayer.ID.ToString(),
                    tplayer.Health.ToString(),
                    tplayer.Points.ToString(),
                    tplayer.ACTION.ToString(),
                    tplayer.BombsCount.ToString(),
                    tplayer.BonusType.ToString(),
                    tplayer.X.ToString(),
                    tplayer.Y.ToString()
                });
                dead_players_listvView.Items.Add(item);
            }
        }
       

        /// <summary>
        /// Задать списки игроков
        /// </summary>
        public void initListView()
        {
            players_listView.Clear();
            dead_players_listvView.Clear();

            players_listView.View = View.Details;
            dead_players_listvView.View = View.Details;

            players_listView.Columns.Add("Name");
            players_listView.Columns.Add("ID");
            players_listView.Columns.Add("Health");
            players_listView.Columns.Add("Points");
            players_listView.Columns.Add("PlayerAction");
            players_listView.Columns.Add("BombsCount");
            players_listView.Columns.Add("Bonus");
            players_listView.Columns.Add("X");
            players_listView.Columns.Add("Y");


            dead_players_listvView.Columns.Add("Name");
            dead_players_listvView.Columns.Add("ID");
            dead_players_listvView.Columns.Add("Health");
            dead_players_listvView.Columns.Add("Points");
            dead_players_listvView.Columns.Add("PlayerAction");
            dead_players_listvView.Columns.Add("BombsCount");
            dead_players_listvView.Columns.Add("Bonus");
            dead_players_listvView.Columns.Add("X");
            dead_players_listvView.Columns.Add("Y");

            for (int i = 0; i < gb.Players.Count; i++)
            {
                var tplayer = gb.Players[i];

                var item = new ListViewItem(new[] {
                    tplayer.Name, tplayer.ID.ToString(),
                    tplayer.Health.ToString(),
                    tplayer.Points.ToString(),
                    tplayer.ACTION.ToString(),
                    tplayer.BombsCount.ToString(),
                    tplayer.BonusType.ToString(),
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
                PaintEllipse(tplayer.X, tplayer.Y, tplayer.Color);
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

                for (int i = tlava.X; i <= tlava.X + tlava.Radius; i++)
                {
                    if (i < 0 || i > gb.W - 1 || gb.Cells[i, tlava.Y] is Cell_indestructible)
                    {
                        break;
                    }
                    PaintRect(i, tlava.Y, Config.lava_color);
                }

                for (int i = tlava.X; i >= tlava.X - tlava.Radius; i--)
                {
                    if (i < 0 || i > gb.W - 1 || gb.Cells[i, tlava.Y] is Cell_indestructible)
                    {
                        break;
                    }
                    PaintRect(i, tlava.Y, Config.lava_color);
                }

                for (int j = tlava.Y; j <= tlava.Y + tlava.Radius; j++)
                {
                    if (j < 0 || j > gb.H - 1 || gb.Cells[tlava.X, j] is Cell_indestructible)
                    {
                        break;
                    }

                    PaintRect(tlava.X, j, Config.lava_color);
                }

                for (int j = tlava.Y; j >= tlava.Y - tlava.Radius; j--)
                {
                    if (j < 0 || j > gb.H - 1 || gb.Cells[tlava.X, j] is Cell_indestructible)
                    {
                        break;
                    }

                    PaintRect(tlava.X, j, Config.lava_color);
                }
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
                ReceiveUserInfo();

                panel1.Refresh();
                UpdateListView();

                this.Text = "Тик - " + GameTimer;

                GameProccess();                            
                DrawAll();

                gameBoardStates.Add((GameBoard)gb.Clone());

                CheckGameOver();
                SendGameInfo();
            }
        }


        public void panel1_Paint(object sender, PaintEventArgs e)
        {     

        }

        /// <summary>
        /// Окончание игры: вывод и сохранение информации
        /// </summary>
        public void GameOver()
        {
            isGameOver = true;

            Disconnect();
            server.Stop();

            game_timer.Stop();

            Thread.Sleep(100);
            Compiler.DeleteComppiledFiles();

            var result = DialogResult.No;
            string message = "";


            if (gb.Players.Count == 1)
            {
                gb.Players[0].Points += Config.player_win_points;

                message = "Всех порешал игрок - " + gb.Players[0].Name + ", количество Очков - " + gb.Players[0].Points + ". \n";
            }
            else if (gb.Players.Count < 1)
            {
                message = "Игроки погибли одновременно. \n";
            }
            else if (GameTimer < 1)
            {
                message = "Время истекло. \n";
                message += "Выжившие Игроки и их Очки: \n";

                for (int i = 0; i < gb.Players.Count; i++)
                {
                    var tplayer = gb.Players[i];
                    tplayer.Points += Config.player_survive_points;

                    message += tplayer.Name + ": " + tplayer.Points + " \n";
                }

                message += "Мёртвые Игроки и их Очки: \n";
                for (int i = 0; i < gb.DeadPlayers.Count; i++)
                {
                    var tplayer = gb.DeadPlayers[i];
                    message += tplayer.Name + ": " + tplayer.Points + " \n";
                }
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
                if (GameTimer < 1 || gb.Players.Count <= 1)
                {                
                    GameOver();
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
            if (gameBoardStates.Count<1)
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

            for (int i = 0; i < lastGameBoardState.DeadPlayers.Count; i++)
            {
                var tplayer = lastGameBoardState.DeadPlayers[i];
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
                for (int j = i+1; j < tempplayers.Count; j++)
                {
                    var tvitya2 = tempplayers[j];
                    if (tvitya1.X == tvitya2.X && tvitya1.Y == tvitya2.Y)
                    {
                        gb.Players[i].ACTION = PlayerAction.Wait;
                        gb.Players[j].ACTION = PlayerAction.Wait;
                    }
                    if (tvitya2.X == gb.Players[i].X  && tvitya2.Y == gb.Players[i].Y && tvitya1.X == gb.Players[i].X && tvitya1.Y == gb.Players[i].Y)
                    {
                        gb.Players[i].ACTION = PlayerAction.Wait;
                        gb.Players[j].ACTION = PlayerAction.Wait;
                    }
                }
            }

            for (int i = 0; i < gb.Players.Count; i++)
            {
                PlayerMove(gb.Players[i]);
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

                int i = tvitya.X;
                int j = tvitya.Y;

                if(tempbonus[i,j] != null && tempbonus[i,j].Visible == true)
                {

                    tvitya.Points += Config.player_bonus_find_points;

                    if (tempbonus[i,j] is Bonus_big)
                    {
                        switch (tvitya.BonusType)
                        {
                            case BonusType.None:
                                tvitya.BonusType = BonusType.BigBomb;
                                break;

                            case BonusType.Ammunition:
                                tvitya.BonusType = BonusType.All;
                                if (tvitya.BombsCount + 1 <= Config.player_bombs_count_max)
                                {
                                    tvitya.BombsCount++;
                                }
                                break;
                        }
                    }
                    else if (tempbonus[i,j] is Bonus_fast)
                    {
                        switch (tvitya.BonusType)
                        {
                            case BonusType.None:
                                tvitya.BonusType = BonusType.Ammunition;
                                break;

                            case BonusType.BigBomb:
                                tvitya.BonusType = BonusType.All;
                                break;

                            default:
                                if (tvitya.BombsCount+1<=Config.player_bombs_count_max)
                                {
                                    tvitya.BombsCount++;
                                }
                                break;
                        }
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


  

        /// <summary>
        /// Перезарядка игроков
        /// </summary>
        /// <param name="_player"></param>
        public void PlayerReload(Player _player)
        {
            if (_player.BonusType == BonusType.Ammunition || _player.BonusType == BonusType.All)
            {
                _player.BombsCount = Config.player_bombs_count_max;
            }
            else
            {
                _player.BombsCount = Config.player_bombs_count_start;
            }
        }

   

        /// <summary>
        /// Создать бомбу на месте игрока
        /// </summary>
        /// <param name="_player">Игрок, создающий бомбу</param>
        public void CreateBomb(Player _player)
        {
            Bomb tbomb;
            if (_player.BonusType == BonusType.BigBomb || _player.BonusType == BonusType.All)
            {
                tbomb = new Bomb_big();
            }
            else
            {
                tbomb = new Bomb();                
            }

            tbomb.LiveTime = Config.bomb_live_time;
            tbomb.PlayerID = _player.ID;
            tbomb.X = _player.X;
            tbomb.Y = _player.Y;

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
                    tplayer.Points += Config.player_kill_points;
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
        public void PlayerKill(Player pplayer, Lava plava)
        {
            if (plava.PlayerID != pplayer.ID)
            {
                PlayerAddPointsKill(plava);
            }

            if (pplayer is User)
            {
                PlayerDisconnect(clients[pplayer]);
            }

            clients[pplayer] = null;

            PlayerDeath(pplayer);
        }

        /// <summary>
        /// Похороны игрока
        /// </summary>
        /// <param name="pplayer"></param>
        public void PlayerDeath(Player pplayer)
        {
            gb.Players.Remove(pplayer);
            gb.DeadPlayers.Add(pplayer);
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
                MessageBox.Show("Ошибка в функции PlayerDisconnect: " + e.Message);
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

                if (tplayer.X == plava.X && tplayer.Y == plava.Y)
                {
                    tplayer.Health--;
                    if (tplayer.Health < 1)
                    {
                        PlayerKill(tplayer, plava);
                    }
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

                PlayerAddPointsCellDestroy(plava);
            }
        }


        /// <summary>
        /// Обработка состояния лавы
        /// </summary>
        public void LavasProccess()
        {

            LogUpdate("LavasProccess, lavasCount перед проверкой " + gb.Lavas.Count);

            for (int k = 0; k < gb.Lavas.Count; k++)
            {
                var tlava = gb.Lavas[k];

                if (tlava.LiveTime<1)
                {
                    gb.Lavas.Remove(tlava);
                    k--;
                }                     
                tlava.LiveTime--;
            }

            LogUpdate("LavasProccess, lavasCountпосле проверки " + gb.Lavas.Count);

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
            int tradius = Config.lava_radius;

            if (_bomb is Bomb_big)
            {
                tradius = Config.lava_radius_big;
            }

            for (int i = _bomb.X+1; i <= _bomb.X + tradius; i++)
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


            for (int j = _bomb.Y+1; j <= _bomb.Y + tradius; j++)
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


            for (int j = _bomb.Y-1; j >= _bomb.Y - tradius; j--)
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
            g.FillEllipse(new SolidBrush(cl), x * cw + cw/6, y * cw+ cw/6, cw-cw/3, cw-cw/3);
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
            log_box.Text += message + Environment.NewLine;
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

            if (game_timer.Interval<100)
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
