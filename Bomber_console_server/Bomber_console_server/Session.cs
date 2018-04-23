using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

using ClassLibrary_CGC;
using User_class;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using Bomber_console_server.Model;

namespace Bomber_console_server
{
    public class Session
    {
        GameBoard gb;
        int GameTimer;
        int TimeLimit;
        int globalTimeLimit;
        bool isGameOver;
        IPAddress serverIp;
        int serverPort;
        TcpListener server;
        List<UserInfo> usersInfo;
        List<GameBoard> gameBoardStates;
        List<GameBoard> savedGameBoardStates;
        string[] php_compiled_path;
        static int phpgameID;       

        public static string gameID;
        public string gameboardTempJsonForClient;
        static SandboxGame sandboxgame;

        Random rn = new Random();

        public Session(SandboxGame _sandboxgame)
        {
            sandboxgame = _sandboxgame;           

            globalTimeLimit = 120000;
            TimeLimit = 1000;
            isGameOver = false;
            serverIp = IPAddress.Parse("127.0.0.1");
            usersInfo = new List<UserInfo>();
            gameBoardStates = new List<GameBoard>();
            savedGameBoardStates = new List<GameBoard>();

            // gameID = Helper.CalculateMD5Hash(DateTime.Now.Millisecond * Helper.rn.NextDouble() + "JOPAJOPA");
            gameID = "JOPAJOPA";

            GameTimer = Config.gameTicksMax;

            isGameOver = false;

            serverPort = rn.Next(1001, 65001);

           
        }

        /// <summary>
        /// Начать сеанс игры
        /// </summary>
        public void InitGame()
        {
            try
            {
                gb = new GameBoard();

                serverStart();
                initUsersInfo();

                SetGameBoardCast();
                gameBoardStates.Add((GameBoard)gb.Clone());

                NextTick();
            }
            catch (Exception er)
            {
                StopClearTempFiles();
                throw new Exception(er.Message);
            }
        }

        void initUsersInfo()
        {
            try
            {
                for (int i = 0; i < sandboxgame.usergroup.users.Count; i++)
                {
                    Compiler cmp = MoveStartUserExe(sandboxgame.usergroup.users[i].user_exe_phppath, i);
                    if (cmp == null)
                    {
                        continue;
                    }
                    TcpClient tcp = server.AcceptTcpClient();
                    if (tcp == null)
                    {
                        continue;
                    }

                    User player = InitPlayersInfo(sandboxgame.usergroup.users[i], i);

                    usersInfo.Add(new UserInfo(player, tcp, cmp));
                    gb.Players.Add(player);
                }
            }
            catch (Exception er)
            {               
                throw new Exception(er.Message);
            }
        }
    



        /// <summary>
        /// Создание игрока на основе данных из формы
        /// </summary>
        /// <param name="i"></param>
        User InitPlayersInfo(dbUser user, int i)
        {         
            User pplayer = new User();

            pplayer.ID = user.id + "";
            pplayer.Name = user.name;         

            pplayer.Color = System.Drawing.Color.Black;
            pplayer.Points = 0;
            //    pplayer.Health = Config.player_health;
            pplayer.Health = 1;
            pplayer.Bang_radius = Config.bang_start_radius;
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
            return pplayer;
        }




        public void serverStart()
        {
            try
            {
                server = new TcpListener(serverIp, serverPort);
                server.Start();
            }
            catch (SocketException)
            {
                serverPort = rn.Next(1001, 65001);
                serverStart();
            }
        }




        /// <summary>
        /// Получить информацию об Игроках (класс Player) от Клиентов
        /// </summary>
        public void RecieveUserInfo()
        {
            for (int i = 0; i < usersInfo.Count; i++)
            {
                try
                {        
                    if (usersInfo[i].client==null)
                    {
                        usersInfo[i].player.ACTION = PlayerAction.Wait;
                        continue;
                    }   

                    NetworkStream strm = usersInfo[i].client.GetStream();

                    string sleeptimeStr = Helper.readStream(strm);
                    int sleepTime = int.Parse(sleeptimeStr);

                    usersInfo[i].globalTimeLimit += sleepTime;

                    if (sleepTime > TimeLimit)
                    {
                        throw new Exception("Стратегия игрока " + usersInfo[i].player.Name + " слишком долго думала: " + sleepTime + "ms");
                    }

                    if (usersInfo[i].globalTimeLimit > globalTimeLimit)
                    {
                        throw new Exception("Стратегия игрока " + usersInfo[i].player.Name + " превысила общий лимит времени");
                    }

                    Helper.writeStream(strm, "p");

                    string message = Helper.readStream(strm);
                    usersInfo[i].player.ACTION = Helper.DecryptAction(message);
                }
                catch (Exception er)
                {
                    Helper.LOG(Compiler.LogPath, $"RecieveUserInfo ERROR: {er.Message} - {usersInfo[i].player.Name}:{usersInfo[i].compiler.containerName}");
                    usersInfo[i].player.ACTION = PlayerAction.Wait;
                }
            }
        }


        /// <summary>
        /// Следующий ход игры
        /// </summary>
        public void NextTick()
        {
            CheckGameOver();

            if (isGameOver == false)
            {
                Console.WriteLine("TICK - " + GameTimer);
                GameProccess();

                SetXYInfo();
                SetGameBoardCast();

                gameBoardStates.Add((GameBoard)gb.Clone());

                SendGameInfo();
                RecieveUserInfo();
                NextTick();
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Сериализовать в json класс Gameboard
        /// </summary>
        public void SetGameBoardCast()
        {
            GameBoard tempGB =(GameBoard) gb.Clone();
            gameboardTempJsonForClient = JsonConvert.SerializeObject(tempGB); 
        }

        /// <summary>
        /// Отправить Клиентам инофрмацию об Игровом мире (объект класса GameBoard)
        /// </summary>
        public void SendGameInfo()
        {
            for (int i = 0; i < usersInfo.Count; i++)
            {
                try
                {   
                    if (usersInfo[i].client == null)
                    {
                        continue;
                    }       
                    UserInfo tempUserInfo = usersInfo[i];
                    NetworkStream strm = tempUserInfo.client.GetStream();
                    string userjson = JsonConvert.SerializeObject(tempUserInfo.player);
                    tempUserInfo.compiler.SaveTempGameInfo(gameboardTempJsonForClient, userjson);

                    Helper.writeStream(strm, "s");
                }
                catch (Exception er)
                {
                    Helper.LOG(Compiler.LogPath, "SendGameInfo ERROR: " + er.Message);
                    PlayerDisconnect(usersInfo[i].player);
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
        /// Окончание игры: очищение, разрыв соединения, отображение и сохранение информации
        /// </summary>
        public void GameOver(List<Player> winners)
        {
            isGameOver = true;

            for (int i = 0; i < winners.Count; i++)
            {
                winners[i].Points += Config.player_win_points;
            }

            gameBoardStates.Add(gb);
            SaveGameInfoFile();
            StopClearTempFiles();


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
                        if (gb.Bonuses[ii].X == i && gb.Bonuses[ii].Y == j && gb.Bonuses[ii].Visible)
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


        void StopClearTempFiles()
        {
            try
            {               
                gameBoardStates.Clear();

                Disconnect();
                server.Stop();

                for (int i = 0; i < usersInfo.Count; i++)
                {
                    try
                    {
                        Console.WriteLine($"Stopping container {usersInfo[i].compiler.containerName}");
                        usersInfo[i].compiler.StopContainer();
                    }
                    catch (Exception er)
                    {
                        Helper.LOG($"{Compiler.LogPath}", $"Не удалось остановить контейнер {usersInfo[i].compiler.containerName}: {er.Message}");
                        continue;
                    }
                }
                usersInfo.Clear();


                Compiler.EndProccess();


            }
            catch (Exception e)
            {
                Helper.LOG(Compiler.LogPath, "StopClearTempFiles ERROR: " + e.Message);
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
        /// Сохранение итогов игры
        /// </summary>
        public void SaveGameInfoFile()
        {
            //try
            //{
            //string gameResultDirectoryName = Directory.GetCurrentDirectory() + "\\" + GetInfoAboutThisGame() + "\\";

            //Helper.DeleteDirectory(gameResultDirectoryName);
            //Directory.CreateDirectory(gameResultDirectoryName);

            Compiler.SaveGameStatesForVisualizer(gameBoardStates);
            Compiler.SaveGameResult(GetPlayerResult());
            Compiler.SavePlayersAllCommands(GetPlayersInfoAllTicks());
            Compiler.Compress();
         
        }


        /// <summary>
        /// Получить информацию о результатах игроков
        /// </summary>
        /// <returns></returns>
        List<Player> GetPlayerResult()
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


        public List<List<Player>> GetPlayersInfoAllTicks()
        {
            List<List<Player>> AllPlayersStates = new List<List<Player>>();

            for (int i = 0; i < gameBoardStates.Count; i++)
            {
                AllPlayersStates.Add(gameBoardStates[i].Players);
            }
            return AllPlayersStates;
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

               // gb.Players[i].ACTION = PlayerAction.Wait;
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
                    PlayerDeath(tplayer);
                }
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
            pplayer.Health = 0;

            if (pplayer is User)
            {
                PlayerDisconnect(pplayer);
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
                try
                {
                    if (usersInfo[i].client != null)
                    {
                        usersInfo[i].client.Close();
                    }
                }
                catch (Exception er)
                {
                    throw new Exception($"Disconnect ERROR ({usersInfo[i].player.Name})");

                }
            }


        }


        /// <summary>
        /// Отключение игрока-клиента
        /// </summary>
        /// <param name="pclient"></param>
        public void PlayerDisconnect(Player pplayer)
        {
            TcpClient tempclient;
            try
            {
                tempclient = usersInfo.Find(c => c.player == pplayer).client;           
                tempclient.Close();                
            }
            catch (Exception e)
            {
                Helper.LOG(Compiler.LogPath, "Ошибка в функции PlayerDisconnect: " + e.Message);               
            }
            tempclient = null;
            usersInfo.Find(c => c.player == pplayer).client = tempclient;
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
        /// Попытаться скомпилировать и запустить стратегию пользователя
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Compiler MoveStartUserExe(string php_exe_path, int i)
        {
            try
            {
                Compiler compiler = new Compiler(php_exe_path, i, sandboxgame.id);
               // compiler.Compile();
                compiler.StartProccess(serverPort);
                //  Thread.Sleep(1000);
                //   compiler.UserClientStart(serverPort);        

                return compiler;
            }
            catch (Exception e)
            {
                throw new Exception("ERROR in MoveStartUserExe: " + e.Message);
               // return null;
            }
        }
    }
}
