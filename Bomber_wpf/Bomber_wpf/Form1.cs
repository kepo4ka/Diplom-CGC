using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ClassLibrary_CGC;
using User_class;
using System.IO;
using System.Globalization;

namespace Bomber_wpf
{
    //public enum PlayerAction
    //{
    //    Right, Left, Up, Down, Bomb, wait
    //}

    //public enum BonusType
    //{
    //   Big, Fast, None, All
    //}


    public partial class Form1 : Form
    {

        //private readonly SynchronizationContext synchronizationContext;
        //private DateTime previousTime = DateTime.Now;

        //List<Player> allPlayers = new List<Player>();

        public int cw = 30;
        Graphics g;
        Pen p;
        SolidBrush sb;
        GameBoard gb;
        int GameTimer;

        string serverIp = "127.0.0.1";
        TcpListener server;
        List<Player> winners = new List<Player>();
        Dictionary<Player, TcpClient> clients = new Dictionary<Player, TcpClient>();
        List<GameBoard> gameBoardStates = new List<GameBoard>();

        bool isGameOver = false;

        bool testbool = false;

        public Form1()
        {
            InitializeComponent();
            g = panel1.CreateGraphics();
            p = new Pen(Color.Black);
            sb = new SolidBrush(Color.DimGray);

            IPAddress ip = IPAddress.Parse(serverIp);
            server = new TcpListener(ip, 9595);
            server.Start();

         
            InitGame();
            // server.Stop();

        }

        /// <summary>
        /// Начать сеанс игры
        /// </summary>
        public void InitGame()
        {        
            clients.Clear();      
            winners.Clear();
            gameBoardStates.Clear();

            isGameOver = false;    
            
            GameTimer = CONST.gameTicksMax;
            gb = new GameBoard();

            Player vitya = new Bot()
            {
                Name = "vitya_bot",
                ID = 1,
                BonusType = BonusType.All,
                X = gb.W-1,
                Y = 0,
                ReloadTime = 0,
                Health = 2,
                Color = Color.Purple
            };

            Player user = new User()
            {
                Name = "User",
                Health = 50,
                ID = 333,
                X = 0,
                Y = 14,
                ReloadTime = 0,
                Color = Color.Blue
            };

            gb.Players.Add(user);
            gb.Players.Add(vitya);        

            while (clients.Count < 1)
            {
                MessageBox.Show("Ожидаем клиентов в фоновом режиме");
                clients.Add(user, server.AcceptTcpClient());
                MessageBox.Show("Клиент подключился");
            }
            SendGameInfo();

            gameBoardStates.Add((GameBoard)gb.Clone());
            SaveGameInfoFile();

            /* Дополнительные боты
            Player Yura = new Bot()
            {
                Name = "Yura_bot",
                ID = 2,
                BonusType = BonusType.None,
                X = gb.W - 1,
                Y = gb.H - 1,
                ReloadTime = 0,
                Health = 200,
                Color = Color.Aqua
            };

            Player Oleg = new Bot()
            {
                Name = "Oleg_bot",
                ID = 2,
                BonusType = BonusType.Big,
                X = 0,
                Y = 0,
                ReloadTime = 0,
                Health = 350,
                Color = Color.BlueViolet
            };
            gb.Players.Add(Yura);
            gb.Players.Add(Oleg);
             Дополнительные боты */



            //players_ListBox.Items.Add(vitya.Name + vitya.ID + ":" + "bonuses: " + vitya.BonusType.ToString() + "|" + "Health: " + vitya.Health + "|" + "reloadTime: " + vitya.ReloadTime + " |" + "Points: " + gb.Players[0].Points);
            //players_ListBox.Items.Add(yura.Name + yura.ID + ":" + "bonuses: " + yura.BonusType.ToString() + "|" + "Health: " + yura.Health + "|" + "reloadTime: " + yura.ReloadTime + " |" + "Points: " + gb.Players[0].Points);



            //Bonus tmb = new Bonus_big(8, 0)
            //{
            //    Visible = true
            //};
            //Bonus tmb1 = new Bonus_fast(7, 0)
            //{
            //    Visible = true
            //};
            //gb.Bonuses.Add(tmb);
            //gb.Bonuses.Add(tmb1);

            //Cell cl = new Cell_destructible()
            //{
            //    X = 3,
            //    Y = 0
            //};
            //gb.Cells[3, 0] = cl;

            //Bomb bmb = new Bomb_big()
            //{
            //    X = 0,
            //    Y = 12,
            //    PlayerID = 0,
            //    LiveTime = CONST.bomb_live_time               

            //};
            //gb.Bombs.Add(bmb);

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
                        PlayerDisconnect(tclient.Value);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Получить информацию об Игроках (класс Player) от Клиентов
        /// </summary>
        public void ReceiveUserInfo()
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
                    PlayerDisconnect(tclient.Value);
                }
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
                    tplayer.Name, tplayer.ID.ToString(),
                    tplayer.Health.ToString(),
                    tplayer.Points.ToString(),
                    tplayer.ACTION.ToString(),
                    tplayer.ReloadTime.ToString(),
                    tplayer.BonusType.ToString(),
                    tplayer.X.ToString(),
                    tplayer.Y.ToString()
                });
                players_listView.Items.Add(item);
            }
        }
       

        /// <summary>
        /// Перенести умершершего игрока в список мётрвых
        /// </summary>
        /// <param name="pplayer">Почивший игрок</param>
        public void ChangeListView(Player pplayer)
        {
            for (int i = 0; i < players_listView.Items.Count; i++)
            {
                var tItem = players_listView.Items[i].SubItems[0].Text;
                if (tItem == pplayer.Name)
                {
                    players_listView.Items.RemoveAt(i);
                    break;
                }
            }

            var item = new ListViewItem(new[] {
                    pplayer.Name,
                    pplayer.ID.ToString(),
                    pplayer.Health.ToString(),
                    pplayer.Points.ToString(),
                    pplayer.ACTION.ToString(),
                    pplayer.ReloadTime.ToString(),
                    pplayer.BonusType.ToString(),
                    pplayer.X.ToString(),
                    pplayer.Y.ToString()
                });
            dead_players_listvView.Items.Add(item);
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
            players_listView.Columns.Add("reloadTime");
            players_listView.Columns.Add("Bonus");
            players_listView.Columns.Add("X");
            players_listView.Columns.Add("Y");


            dead_players_listvView.Columns.Add("Name");
            dead_players_listvView.Columns.Add("ID");
            dead_players_listvView.Columns.Add("Health");
            dead_players_listvView.Columns.Add("Points");
            dead_players_listvView.Columns.Add("PlayerAction");
            dead_players_listvView.Columns.Add("reloadTime");
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
                    tplayer.ReloadTime.ToString(),
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


            //allPlayers = new List<Player>();
            //for (int i = 0; i < gb.Players.Count; i++)
            //{
            //    var tplayer = gb.Players[i];
            //    Player nplayer = new Player()
            //    {
            //        Name = tplayer.Name,
            //        ID = tplayer.ID,
            //        ReloadTime = tplayer.ReloadTime,
            //        BonusType = tplayer.BonusType,
            //        ACTION = tplayer.ACTION,
            //        Color = tplayer.Color,
            //        Points = tplayer.Points,
            //        X = tplayer.X,
            //        Y = tplayer.Y
            //    };           
            //    allPlayers.Add(nplayer);
            //}


            //players_ListBox.Items[0] = allPlayers[0].Name + allPlayers[0].ID + ":" + "bonuses: " + allPlayers[0].BonusType.ToString() + "|" + "Health: " + allPlayers[0].Health + "|" + "reloadTime: " + allPlayers[0].ReloadTime + "|" + "Points: " + allPlayers[0].Points;
            //players_ListBox.Items[1] = allPlayers[1].Name + allPlayers[1].ID + ":" + "bonuses: " + allPlayers[1].BonusType.ToString() + "|" + "Health: " + allPlayers[1].Health + "|" + "reloadTime: " + allPlayers[1].ReloadTime + "|" + "Points: " + allPlayers[1].Points;

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
                GameTimer--;

                DrawCells();

                LavasProccess();
                PlayerProcess();

                PlayerBonusCollision();
                BonusesProccess();

                PaintPlayers();
                BombsProccess();

                DrawGrid();

                SendGameInfo();

                gameBoardStates.Add((GameBoard)gb.Clone());

                CheckGameOver();
            }
        }


        public void panel1_Paint(object sender, PaintEventArgs e)
        {
            //DateTime now = DateTime.Now;
            //if ((now - previousTime).Milliseconds > 50)
            //{
            //    players_ListBox.Items[0] = test + "";
            //    previousTime = now;
            //}
            //NextTick();

        }

        /// <summary>
        /// Проверить наступили ли условия для наступления Конца игры
        /// </summary>
        public void CheckGameOver()
        {
            if (isGameOver == false)
            {
                if (GameTimer < 1)
                {
                    isGameOver = true;
                    game_timer.Stop();
                    SaveGameInfoFile();

                    string message = "Время истекло. \n";
                    message += "Живые игроки и их Очки: \n";


                    for (int i = 0; i < gb.Players.Count; i++)
                    {
                        var tplayer = gb.Players[i];
                        winners.Add(tplayer);
                        message += tplayer.Name + ": " + tplayer.Points + " \n";
                    }

                    message += "Начать заново?";

                    var result = MessageBox.Show(message, "GAME OVER",
                                      MessageBoxButtons.YesNo,
                                      MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        InitGame();
                    }
                }

                if (gb.Players.Count == 1)
                {
                    isGameOver = true;
                    game_timer.Stop();
                    SaveGameInfoFile();

                    string message = "Всех порешал игрок - " + gb.Players[0].Name + ", количество Очков - " + gb.Players[0].Points + ". \n Начать заново?";

                    var result = MessageBox.Show(message, "GAME OVER",
                                      MessageBoxButtons.YesNo,
                                      MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        InitGame();
                    }
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
                gameStatesFileName += gameBoardStates[0].Players[gameBoardStates[0].Players.Count-1].Name + ")";


                gameStatesFileName += " " + time_str;
                gameStatesFileName += ".dat";

                BinaryFormatter form = new BinaryFormatter();
                using (FileStream fs = new FileStream(gameStatesFileName, FileMode.OpenOrCreate))
                {
                    form.Serialize(fs, gameBoardStates);
                }
            //}
            //catch (Exception e)
            //{
            //    MessageBox.Show("Ошибка при сохранении информации об игре в файл: " + e.Message);
            //}
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
                        gb.Players[i].ACTION = PlayerAction.wait;
                        gb.Players[j].ACTION = PlayerAction.wait;
                    }
                    if (tvitya2.X == gb.Players[i].X  && tvitya2.Y == gb.Players[i].Y && tvitya1.X == gb.Players[i].X && tvitya1.Y == gb.Players[i].Y)
                    {
                        gb.Players[i].ACTION = PlayerAction.wait;
                        gb.Players[j].ACTION = PlayerAction.wait;
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
                    if (tempbonus[i,j] is Bonus_big)
                    {
                        switch (tvitya.BonusType)
                        {
                            case BonusType.None:
                                tvitya.BonusType = BonusType.Big;
                                
                                break;
                            case BonusType.Fast:
                                tvitya.BonusType = BonusType.All;
                                tvitya.ReloadTime = CONST.player_reload_fast;
                                break;
                        }
                    }
                    else if (tempbonus[i,j] is Bonus_fast)
                    {
                        switch (tvitya.BonusType)
                        {
                            case BonusType.None:
                                tvitya.BonusType = BonusType.Fast;
                                tvitya.ReloadTime = CONST.player_reload_fast;
                                break;
                            case BonusType.Big:
                                tvitya.BonusType = BonusType.All;
                                tvitya.ReloadTime = CONST.player_reload_fast;
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
                case PlayerAction.wait:
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
                if (pplayer.ReloadTime < 1)
                {
                    CreateBomb(pplayer);
                    PlayerReload(pplayer);                   
                }
            }
            pplayer.ReloadTime--;
        }


        /// <summary>
        /// Отрисовать живых игроков
        /// </summary>
        public void PaintPlayers()
        {
            for (int i = 0; i < gb.Players.Count; i++)
            {
                var tplayer = gb.Players[i];
                PaintEllipse(tplayer.X, tplayer.Y, tplayer.Color);
            }
        }

        /// <summary>
        /// Перезарядка игроков
        /// </summary>
        /// <param name="_player"></param>
        public void PlayerReload(Player _player)
        {
            if (_player.BonusType == BonusType.Fast || _player.BonusType == BonusType.All)
            {
                _player.ReloadTime = CONST.player_reload_fast;
            }
            else
            {
                _player.ReloadTime = CONST.player_reload;
            }
        }

   

        /// <summary>
        /// Создать бомбу на месте игрока
        /// </summary>
        /// <param name="_player">Игрок, создающий бомбу</param>
        public void CreateBomb(Player _player)
        {
            Bomb tbomb;
            if (_player.BonusType == BonusType.Big || _player.BonusType == BonusType.All)
            {
                tbomb = new Bomb_big();
            }
            else
            {
                tbomb = new Bomb();                
            }

            tbomb.LiveTime = CONST.bomb_live_time;
            tbomb.PlayerID = _player.ID;
            tbomb.X = _player.X;
            tbomb.Y = _player.Y;

            gb.Bombs.Add(tbomb);
        }

        /// <summary>
        /// Нарисовать видимые бонусы
        /// </summary>
        public void BonusesProccess()
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
        /// Нарисовать бомбы
        /// </summary>
        public void BombsProccess()
        {          
            for (int i = 0; i < gb.Bombs.Count; i++)
            {
                var tbomb = gb.Bombs[i];
                PaintBomb(tbomb.X, tbomb.Y, tbomb.Color);

                if (tbomb.LiveTime < 1)
                {
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
                    tplayer.Points += CONST.player_kill_points;
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
                    tplayer.Points += CONST.player_cell_destroy_points;
                    break;
                }
            }
        }


        public void PlayerDeath(Player pplayer, Lava plava)
        {
            if (plava.PlayerID != pplayer.ID)
            {
                PlayerAddPointsKill(plava);
            }
            gb.Players.Remove(pplayer);

            if (pplayer is User && clients[pplayer] != null)
            {
                clients[pplayer].Close();
            }

            clients[pplayer] = null;

            ChangeListView(pplayer);
            gb.DeadPlayers.Add(pplayer);
        }



        public void PlayerDisconnect(TcpClient client)
        {
            if (client != null)
            {
                client.Close();
            }


            Player tplayer = new Player();
            foreach (var tclient in clients)
            {
                if (tclient.Value == client)
                {
                    tplayer = tclient.Key;
                    break;
                }
            }                     

            gb.Players.Remove(tplayer);
            gb.DeadPlayers.Add(tplayer);
            ChangeListView(tplayer);

            //clients[tplayer] = null;           
        }


        /// <summary>
        /// Взаимодействие лавы и игроков
        /// </summary>
        /// <param name="plava"></param>
        /// <param name="i"></param>
        public void LavaPlayersCollision(Lava plava, int i, bool _k)
        {
            for (int k = 0; k < gb.Players.Count; k++)
            {                
                var tplayer = gb.Players[k];
                if (_k)
                {
                    if (tplayer.X == i && tplayer.Y == plava.Y)
                    {
                        tplayer.Health--;
                        if (tplayer.Health < 1)
                        {
                            PlayerDeath(tplayer, plava);
                        }
                    }
                }
                else
                {
                    if (tplayer.X == plava.X && tplayer.Y == i)
                    {
                        tplayer.Health--;
                        if (tplayer.Health < 1)
                        {
                            PlayerDeath(tplayer, plava);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Взаимодействие лавы с остальными объектами
        /// </summary>
        /// <param name="plava">Лава</param>
        public void LavaCollision(Lava plava)
        {
            Bonus[,] tbonuses_mass = ListToMass(gb.Bonuses);
            
            for (int i = plava.X - plava.Radius; i <= plava.X + plava.Radius; i++)
            {
                if (i < 0 || i > gb.W - 1 || gb.Cells[i, plava.Y] is Cell_indestructible)
                {
                    continue;
                }

                LavaPlayersCollision(plava, i, true);               

                if (gb.Cells[i, plava.Y] is Cell_destructible)
                {
                    gb.Cells[i, plava.Y] = new Cell_free()
                    {
                        X = i,
                        Y = plava.Y
                    };
                    if (tbonuses_mass[i, plava.Y] !=null)
                    {
                        tbonuses_mass[i, plava.Y].Visible = true;
                    }
                    PlayerAddPointsCellDestroy(plava);
                }                
            }

            for (int j = plava.Y - plava.Radius; j <= plava.Y + plava.Radius; j++)
            {
                if (j < 0 || j > gb.H - 1 || gb.Cells[plava.X, j] is Cell_indestructible)
                {
                    continue;
                }

                LavaPlayersCollision(plava, j, false);

                if (gb.Cells[plava.X, j] is Cell_destructible)
                {
                    gb.Cells[plava.X, j] = new Cell_free()
                    {
                        X = plava.X,
                        Y = j
                    };
                    if (tbonuses_mass[plava.X, j] != null)
                    {
                        tbonuses_mass[plava.X, j].Visible = true;
                    }

                    PlayerAddPointsCellDestroy(plava);
                }
            }  
        }

        
        /// <summary>
        /// Обработка лавы
        /// </summary>
        public void LavasProccess()
        {
            for (int k = 0; k < gb.Lavas.Count; k++)
            {
                var tlava = gb.Lavas[k];

                if (tlava.LiveTime<1)
                {
                    gb.Lavas.Remove(tlava);
                    continue;
                }

                LavaCollision(tlava);

                for (int i = tlava.X - tlava.Radius; i <= tlava.X + tlava.Radius; i++)
                {
                    if (i < 0 || i > gb.W - 1 || gb.Cells[i, tlava.Y] is Cell_indestructible)
                    {                       
                        continue;
                    }
                    PaintRect(i, tlava.Y, CONST.lava_color);
                }

                for (int j = tlava.Y - tlava.Radius; j <= tlava.Y + tlava.Radius; j++)
                {
                    if (j < 0 || j > gb.H - 1 || gb.Cells[tlava.X, j] is Cell_indestructible)
                    {
                        continue;
                    }

                    PaintRect(tlava.X, j, CONST.lava_color);
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
            int tradius = CONST.lava_radius;
            if (_bomb is Bomb_big)
            {
                tradius = CONST.lava_radius_big;
            }

            Lava tlava = new Lava()
            {
                X = _bomb.X,
                Y = _bomb.Y,
                Radius = tradius,
                LiveTime = CONST.lava_livetime,
                PlayerID = _bomb.PlayerID
            };
            gb.Lavas.Add(tlava);
        }


        /// <summary>
        /// Создать лаву, на месте взрыва бомбы
        /// </summary>
        /// <param name="x">Координата бомбы</param>
        /// <param name="y">Координата бомбы</param>
        public void GenerateLava(int x, int y)
        {
            Lava tlava = new Lava()
            {
                X = x,
                Y = y,
                Radius = CONST.lava_radius_big,
                LiveTime = CONST.lava_livetime
            };
            gb.Lavas.Add(tlava);
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
    }


    /*
    [Serializable]
    public class GameBoard
    {
        int w, h;
        Cell[,] cells;
        List<Player> players;
        List<Player> dead_players;
        List<Bonus> bonuses;
        List<Bomb> bombs;
        List<Lava> lavas;

        public Random rn = new Random();


        public GameBoard()
        {
            int size = 15;
            W = size;
            H = size;
            GenerateBoard(size);
            GenerateBonuses(CONST.bonuses_count);
            Bombs = new List<Bomb>();
            Lavas = new List<Lava>();
            Players = new List<Player>();
            dead_players = new List<Player>();
        }

        public GameBoard(int SIZE)
        {            
            W = SIZE;
            H = SIZE;
            GenerateBoard(SIZE);
            GenerateBonuses(CONST.bonuses_count);
            Bombs = new List<Bomb>();
            Lavas = new List<Lava>();
            Players = new List<Player>();
            dead_players = new List<Player>();
        }


        public List<Player> DeadPlayers
        {
            get
            {
                return dead_players;
            }
            set
            {
                this.dead_players = value;
            }
        }

        /// <summary>
        /// Сгенерировать стенки на квадратном поле
        /// </summary>
        /// <param name="size"></param>
        private void GenerateBoard(int size)
        {
            Cells = new Cell[size, size];
            int temp_rn = 0;

            for (int i = 0; i < Cells.GetLength(0); i++)
            {
                for (int j = 0; j < Cells.GetLength(1); j++)
                {
                    Cells[i, j] = new Cell_free()
                    {
                        X = i,
                        Y = j
                    };
                }
            }

            for (int i = 2; i < Cells.GetLength(0) - 2; i+=2)
            {
                int j = (Cells.GetLength(1) / 2);
                Cells[i, j] = new Cell_destructible()
                {
                    X = i,
                    Y = j
                };
                j = 0;
                Cells[i, j] = new Cell_destructible()
                {
                    X = i,
                    Y = j
                };
                j = Cells.GetLength(1) - 1;
                Cells[i, j] = new Cell_destructible()
                {
                    X = i,
                    Y = j
                };
            }

            for (int j = 2; j < Cells.GetLength(1) - 2; j+=2)
            {
                int i = (Cells.GetLength(0) / 2);
                Cells[i, j] = new Cell_destructible()
                {
                    X = i,
                    Y = j
                };
                i = 0;
                Cells[i, j] = new Cell_destructible()
                {
                    X = i,
                    Y = j
                };
                i = Cells.GetLength(0) - 1;
                Cells[i, j] = new Cell_destructible()
                {
                    X = i,
                    Y = j
                };
            }


            for (int i = 1; i < (Cells.GetLength(0) / 2); i++)
            {
                for (int j = 1; j < (Cells.GetLength(1) / 2); j++)
                {
                    temp_rn = rn.Next(0, 10);
                    int ii = Cells.GetLength(0) - i - 1;
                    int jj = Cells.GetLength(1) - j - 1;

                    if (temp_rn < 5)
                    {
                        Cells[i, j] = new Cell_destructible()
                        {
                            X = i,
                            Y = j
                        };
                        Cells[ii, jj] = new Cell_destructible()
                        {
                            X = ii,
                            Y = jj
                        };
                        Cells[ii, j] = new Cell_destructible()
                        {
                            X = ii,
                            Y = j
                        };
                        Cells[i, jj] = new Cell_destructible()
                        {
                            X = i,
                            Y = jj
                        };
                    }
                }
            }

            for (int i = 1; i < Cells.GetLength(0)-1; i+=2)
            {
                for (int j = 1; j < Cells.GetLength(1)-1; j+=2)
                {
                    Cells[i, j] = new Cell_indestructible()
                    {
                        X = j,
                        Y = i
                    };
                }
            }
        }




        /// <summary>
        /// Сгенерировать бонусы внутри разрушаемых стен
        /// </summary>
        /// <param name="bonuses_count">Количество бонусов (будет умножено в 4 раза)</param>
        private void GenerateBonuses(int bonuses_count)
        {
            bonuses = new List<Bonus>();

            List<Cell> cells_dest = new List<Cell>();

            for (int i = 0; i < W/2; i++)
            {
                for (int j = 0; j < H/2; j++)
                {
                    if (Cells[i,j] is Cell_destructible)
                    {
                        cells_dest.Add(Cells[i, j]);
                    }
                }
            }

            bonuses_count = bonuses_count % cells_dest.Count;

            List<int> cells_dest_indexes = new List<int>();
            for (int i = 0; i < cells_dest.Count; i++)
            {
                cells_dest_indexes.Add(i);
            }

            for (int i = 0; i < bonuses_count; i++)
            {
                int rpoint = cells_dest_indexes[rn.Next(0, cells_dest_indexes.Count)];
                cells_dest_indexes.Remove(rpoint);

                int rtype = rn.Next(0, 2);
                Bonus tbonus;
                if (rtype%2==0)
                {
                    tbonus = new Bonus_fast(cells_dest[rpoint].X, cells_dest[rpoint].Y);
                    bonuses.Add(tbonus);

                    tbonus = new Bonus_fast(W - cells_dest[rpoint].X-1, cells_dest[rpoint].Y);
                    bonuses.Add(tbonus);

                    tbonus = new Bonus_fast(cells_dest[rpoint].X, H - cells_dest[rpoint].Y-1);
                    bonuses.Add(tbonus);

                    tbonus = new Bonus_fast(W - cells_dest[rpoint].X-1, H - cells_dest[rpoint].Y-1);
                    bonuses.Add(tbonus);
                }
                else
                {
                    tbonus = new Bonus_big(cells_dest[rpoint].X, cells_dest[rpoint].Y);
                    bonuses.Add(tbonus);

                    tbonus = new Bonus_big(W - cells_dest[rpoint].X-1, cells_dest[rpoint].Y);
                    bonuses.Add(tbonus);

                    tbonus = new Bonus_big(cells_dest[rpoint].X, H - cells_dest[rpoint].Y-1);
                    bonuses.Add(tbonus);

                    tbonus = new Bonus_big(W - cells_dest[rpoint].X-1, H - cells_dest[rpoint].Y-1);
                    bonuses.Add(tbonus);
                }               
            }
        }




        /// <summary>
        /// Длина поля
        /// </summary>
        public int W
        {
            get
            {
                return w;
            }
            set
            {
                if (value > 2)
                {
                    w = (int)value;
                }
            }
        }

        /// <summary>
        /// Ширина поля
        /// </summary>
        public int H
        {
            get
            {
                return h;
            }
            set
            {
                if (value > 2)
                {
                    h = (int)value;
                }
            }
        }


        /// <summary>
        /// Массив клеток поля
        /// </summary>
        public Cell[,] Cells
        {
            get
            {
                return cells;
            }
            set
            {
                cells = value;
            }
        }

        /// <summary>
        /// Список игроков на поле
        /// </summary>
        public List<Player> Players
        {
            get
            {
                return players;
            }
            set
            {
                players = value;
            }
        }


        /// <summary>
        /// Список бонусов
        /// </summary>
        public List<Bonus> Bonuses
        {
            get
            {
                //List<Bonus> visible_bombs = new List<Bonus>();

                //for (int k = 0; k < bonuses.Count; k++)
                //{
                //    if (bonuses[k].Visible == true)
                //    {
                //        visible_bombs.Add(bonuses[k]);
                //    }
                //}
                //return visible_bombs;

                return bonuses;
            }
            set
            {
                bonuses = value;
            }
        }

        /// <summary>
        /// Список бонусов
        /// </summary>
        public List<Bomb> Bombs
        {
            get
            {
                return bombs;
            }
            set
            {
                bombs = value;
            }
        }


        /// <summary>
        /// Список центров крестовидных лав
        /// </summary>
        public List<Lava> Lavas
        {
            get
            {
                return lavas;
            }
            set
            {
                lavas = value;
            }
        }
    }
    






    [Serializable]
    public class Player : GameObject
    {
        int health;
        int id;
        string name;
        PlayerAction action;
        int points;
        BonusType bonusType;

        int reloadTime;


        public Player()
        {
            Health = 1;
            ACTION = PlayerAction.wait;
            ReloadTime = CONST.player_reload;
            BonusType = BonusType.None;
        }

        public Player(int ID)
        {
            this.ID = ID;
            Health = 1;
            ACTION = PlayerAction.wait;
            ReloadTime = CONST.player_reload;
            BonusType = BonusType.None;
        }

        public Player(int ID, string NAME)
        {
            this.ID = ID;
            this.Name = NAME;
            Health = 1;
            ACTION = PlayerAction.wait;
            ReloadTime = CONST.player_reload;
            BonusType = BonusType.None;
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (value.Length>0)
                {
                    this.name = value;
                }
            }
        }

        public int Points
        {
            get
            {
                return points;
            }
            set
            {
                if (value>0)
                {
                    points = value;
                }
            }
        }

        public BonusType BonusType
        {
            get
            {
                return bonusType;
            }
            set
            {
                if (value >= 0)
                {
                    bonusType = value;
                }
            }
        }

        public int ReloadTime
        {
            get
            {
                return reloadTime;
            }
            set
            {
                if (value >= 0)
                {
                    reloadTime = value;
                }
            }
        }

        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                if (value>=0)
                {
                    id = value;
                }
            }
        }
    

       public PlayerAction ACTION
        {
            get
            {
                return action;
            }
            set
            {
                action = value;
            }
        }

        public int Health
        {
            get
            {
                return health;
            }
            set
            {
                if (value >= 0)
                {
                    health = (int)value;
                }
            }
        }


        public virtual void Play ()
        {

        }
    }

    public class Bot: Player
    {
        public static Random rnd;


        public Bot()
        {
            rnd = new Random();
        }

        public override void Play()
        {
            int rnumber = rnd.Next(0,6);
            switch(rnumber)
            {
                case 0:
                    ACTION = PlayerAction.wait;
                    break;
                case 1:
                    
                    ACTION = PlayerAction.Bomb;
                    break;
                case 2:
                    ACTION = PlayerAction.Right;
                    break;
                case 3:
                    ACTION = PlayerAction.Left;
                    break;
                case 4:
                    ACTION = PlayerAction.Up;
                    break;
                case 5:
                    ACTION = PlayerAction.Down;
                    break; 
            }    
        }
    }


    


    [Serializable]
    public class Bonus : GameObject
    {
        bool visible;
 
        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                visible = value;
            }
        }
    }

    [Serializable]
    public class Bonus_fast : Bonus
    {
       public Bonus_fast(int x, int y)
        {
            this.X = x;
            this.Y = y;
            this.Visible = false;
            this.Color = CONST.bonus_fast;
        }
    }

    [Serializable]
    public class Bonus_big : Bonus
    {
        public Bonus_big(int x, int y)
        {
            this.X = x;
            this.Y = y;
            this.Visible = false;
            this.Color = CONST.bonus_big;
        }
    }

    [Serializable]
    public class Bomb : GameObject
    {
        int liveTime;
        int playerID;
        
        public Bomb()
        {
            this.Color = CONST.bomb_color;
        }

        public Bomb(int x, int y)
        {
            this.X = x;
            this.Y = y;
            this.Color = CONST.bomb_color;
        }

        public Bomb(int x, int y, Player pplayer)
        {
            this.X = x;
            this.Y = y;
            this.Color = CONST.bomb_color;
            this.playerID = pplayer.ID;
        }

        public int PlayerID
        {
            get
            {
                return playerID;
            }

            set
            {
                if (value >= 0)
                    playerID = (int)value;
            }
        }

        public int LiveTime
        {
            get
            {
                return liveTime;
            }

            set
            {
                if (value >= 0)
                    liveTime = (int)value;
            }
        }
    }

    [Serializable]
    public class Bomb_big : Bomb
    {

    }

    public class GameObject
    {
        int x, y;
        Color color;

        public int X
        {
            get
            {
                return x;
            }
            set
            {
                if (value >= 0)
                    x = (int)value;
            }
        }
        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                if (value >= 0)
                    y = (int)value;
            }
        }     

        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }
    }

    [Serializable]
    public class Cell : GameObject
    {
     

    }

    [Serializable]
    public class Cell_indestructible : Cell
    {
        public Cell_indestructible()
        {
            this.Color = CONST.cell_indestructible_color;
        }
    }

    [Serializable]
    public class Cell_destructible : Cell
    {
        public Cell_destructible()
        {
            this.Color = CONST.cell_destructible_color;
        }
    }


    [Serializable]
    public class Cell_free : Cell
    {
        public Cell_free()
        {
           
        }
    }

    [Serializable]
    public class Lava : GameObject
    {
        int liveTime;
        int playerID;
        int radius;

        public Lava ()
        {
            this.Color = CONST.lava_color;
            this.LiveTime = CONST.lava_livetime;
        }
        public Lava(Bomb pbomb)
        {
            this.Color = CONST.lava_color;
            this.LiveTime = CONST.lava_livetime;
            if (pbomb is Bomb_big)
            {
                this.Radius = CONST.lava_radius_big;
            }
            else
            {
                this.Radius = CONST.lava_radius;
            }
        }


        public int Radius
        {
            get
            {
                return radius;
            }
            set
            {
                if (value >= 0)
                    radius = (int)value;
            }
        }

        public int PlayerID
        {
            get
            {
                return playerID;
            }
            set
            {
                if (value >= 0)
                    playerID = (int)value;
            }
        }

        public int LiveTime
        {
            get
            {
                return liveTime;
            }
            set
            {
                if (value >= 0)
                    liveTime = (int)value;
            }
        }
    }

    [Serializable]
    public class CONST
    {
        public static int bonuses_count = 3;
        public static int lava_radius = 1;
        public static int lava_radius_big = 2;
        public static int lava_livetime = 2;
        public static int bomb_live_time = 3;

        public static int player_health = 3;
        public static int player_reload = 3;
        public static int player_reload_fast = 1;
        public static int player_kill_points = 10;
        public static int player_cell_destroy_points = 1;

        public static Color cell_destructible_color = Color.Bisque;
        public static Color cell_indestructible_color = Color.Black;

        public static Color lava_color = Color.Orange;
        public static Color bomb_color = Color.Red;

        public static Color bonus_fast = Color.Yellow;
        public static Color bonus_big = Color.IndianRed;

    }
    */
}
