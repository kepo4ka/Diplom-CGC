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
using System.Threading;

namespace Bomber_console_server
{
    public class Session
    {
        GameBoard gb;
        GameBoard tempGB;

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
        string MapPath;       

        public static string GameType;
        public string gameboardjson;
        static Game sandboxgame;

        List<int> Prioritets = new List<int>();

        List<Cell> DestroyedCells;
        List<Player> DestroyedPlayers;
        int[,] gbpseudo = new int[15, 15];
        List<Player> pseudoplayers;


        Random rn = new Random();

        public Session(Game _sandboxgame, string type)
        {
            Compiler cmp = new Compiler();

            sandboxgame = _sandboxgame;

            globalTimeLimit = 120000;
            TimeLimit = 1000;

            serverIp = IPAddress.Parse("127.0.0.1");
            usersInfo = new List<UserInfo>();
            gameBoardStates = new List<GameBoard>();
            savedGameBoardStates = new List<GameBoard>();

            // GameType = Helper.CalculateMD5Hash(DateTime.Now.Millisecond * Helper.rn.NextDouble() + "JOPAJOPA");

            GameType = "sandbox";

            switch (type)
            {
                case "rating":
                    GameType = "rating";
                    break;
                    
            }
                  

            isGameOver = false;

            serverPort = rn.Next(1001, 65001);
            serverStart();

            InitGame();
        }

       






        public void InitGame()
        {
            isGameOver = false;
            MapPath = "";
            GameTimer = Config.gameTicksMax;
            pseudoplayers = new List<Player>();
            Prioritets = new List<int>();
            DestroyedPlayers = new List<Player>();
            DestroyedCells = new List<Cell>();

            gbpseudo = Helper.LoadMap(out MapPath);

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
          //  SetPseudoPlayers();
          
            try
            {
                InitUsers();
                SetPrioritets();

                gameBoardStates.Add((GameBoard)gb.Clone());

                NextTick();
            }
            catch (Exception er)
            {
                StopClearTempFiles();
                throw new Exception(er.Message + er.StackTrace);
            }
        }


        void InitUsers()
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
                    tcp.ReceiveTimeout = Config.wait_time;
                    if (tcp == null)
                    {
                        continue;
                    }

                    User player = CreatePlayer(sandboxgame.usergroup.users[i]);

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
        /// Создание игрока на основе данных из модели
        /// </summary>
        /// <param name="i"></param>
        User CreatePlayer(dbUser user)
        {
            User pplayer = new User();

            pplayer.ID = user.id + "";
            pplayer.Name = user.name;

            pplayer.Points = 0;
            //    pplayer.Health = Config.player_health;
            pplayer.Health = 1;
            pplayer.BangRadius = Config.bang_start_radius;
            pplayer.BombsCount = Config.player_bombs_count_start;

            pplayer.X = pseudoplayers[0].X;
            pplayer.Y = pseudoplayers[0].Y;
            pseudoplayers.RemoveAt(0);

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


        public void NextTick()
        {
            CheckGameOver();

            if (isGameOver == false)
            {
                Console.WriteLine("TICK - " + GameTimer);
                GameProccess();

                GameboardInfoProccess();
                CommunicateWithClients();

                NextTick();
            }
            else
            {
                return;
            }

        
        }


        /// <summary>
        /// Обновить состояние игрового мира
        /// </summary>
        public void GameProccess()
        {
            GameTimer--;
            gb.Tick++;

            PlayerProccess();
            PlayerBonusCollision();
            BombsProccess();
            LavasProccess();

            DestroyedObjectProccess();

            ChangePrioritet();
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
                    
                    tXYInfo.Type = tempGB.Cells[i, j].Type;

                    for (int ii = 0; ii < tempGB.Players.Count; ii++)
                    {
                        if (tempGB.Players[ii].X == i && tempGB.Players[ii].Y == j)
                        {
                            tXYInfo.Player = tempGB.Players[ii];
                            break;
                        }
                    }

                    for (int ii = 0; ii < tempGB.Lavas.Count; ii++)
                    {
                        if (tempGB.Lavas[ii].X == i && tempGB.Lavas[ii].Y == j)
                        {
                            tXYInfo.Lavas.Add(tempGB.Lavas[ii]);
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
                    if (tXYInfo.Type == CellType.None && tXYInfo.Player == null && tXYInfo.Bomb == null && tXYInfo.Bonus == null)
                    {
                        tXYInfo.Free = true;
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
        public void SaveGameInfo()
        {
            //try
            //{
            //string gameResultDirectoryName = Directory.GetCurrentDirectory() + "\\" + GetInfoAboutThisGame() + "\\";

            //Helper.DeleteDirectory(gameResultDirectoryName);
            //Directory.CreateDirectory(gameResultDirectoryName);

            Compiler.SaveGameStatesForVisualizer(gameBoardStates);
            Compiler.SaveGameResult(GetPlayerResult());
           // Compiler.SavePlayersAllCommands(GetPlayersInfoAllTicks());
            Compiler.Compress();
            string unity = GetPlayerCommandsUnity();
            Compiler.SavePlayersAllCommandsUnity(unity);              

                //sw.WriteLine(gameBoardStates.Count);

                //string players = "";
                //string prioritets = "";
                //for (int i = 0; i < gb.Players.Count; i++)
                //{
                //    prioritets += Prioritets[i] + " ";
                //    players += gb.Players[i].Name + " ";
                //}
                //sw.WriteLine(players);
                //sw.WriteLine(prioritets);


                //sw.WriteLine(Helper.SpliteEndPath(MapPath));


                //for (int i = 0; i < gameBoardStates.Count; i++)
                //{
                //    GameBoard tempgb = gameBoardStates[i];
                //    string actions = "";

                //    for (int j = 0; j < tempgb.Players.Count; j++)
                //    {
                //        actions += Helper.ActionToSymbol(tempgb.Players[j].ACTION) + " ";
                //    }
                //    sw.WriteLine(actions);              //}            
        }




        public string GetPlayerCommandsUnity()
        {
            string result = "";

            result += gameBoardStates.Count + "\r\n";

            string players = "";
            string prioritets = "";
            for (int i = 0; i < gb.Players.Count; i++)
            {
                prioritets += Prioritets[i] + " ";
                players += gb.Players[i].Name + " ";
            }
            result += gb.Players.Count + "\r\n";
            result += players + "\r\n";
            result += prioritets + "\r\n";
            result += Helper.SpliteEndPath(MapPath) + "\r\n";

            for (int i = 0; i < gameBoardStates.Count; i++)
            {
                GameBoard tempgb = gameBoardStates[i];
                string actions = "";

                for (int j = 0; j < tempgb.Players.Count; j++)
                {
                    actions += Helper.ActionToSymbol(tempgb.Players[j].ACTION) + " ";
                }
                result += actions + "\r\n";
            }

            return result;
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
        /// Игроки выполняют свои действия, если могут
        /// </summary>
        public void PlayerProccess()
        {
            for (int i = 0; i < Prioritets.Count; i++)
            {
                Player tplayer = gb.Players[Prioritets[i]];

                if (tplayer.Health < 1)
                {
                    continue;
                }

                //if (tplayer is Bot)
                //{
                //    tplayer.ACTION = tplayer.Play();
                //}
                PlayerMove(tplayer);
            }
        }



        ///// <summary>
        ///// обработка действий игроков
        ///// </summary>
        //public void PlayerProcess()
        //{
        //    List<Player> tempplayers = new List<Player>();
        //    for (int i = 0; i < gb.Players.Count; i++)
        //    {
        //        var tvitya = gb.Players[i];

        //        if (tvitya.Health == 0)
        //        {
        //            continue;
        //        }

        //        if (tvitya is Bot)
        //        {
        //            tvitya.ACTION = tvitya.Play();
        //        }

        //        PlayerFire(tvitya);

        //        Player tempplayer = new Player()
        //        {
        //            ACTION = tvitya.ACTION,
        //            X = tvitya.X,
        //            Y = tvitya.Y
        //        };
        //        PlayerMove(tempplayer);
        //        tempplayers.Add(tempplayer);
        //    }

        //    //for (int i = 0; i < tempplayers.Count; i++)
        //    //{
        //    //    var tvitya1 = tempplayers[i];
        //    //    for (int j = i + 1; j < tempplayers.Count; j++)
        //    //    {
        //    //        var tvitya2 = tempplayers[j];
        //    //        if (tvitya1.X == tvitya2.X && tvitya1.Y == tvitya2.Y)
        //    //        {
        //    //            gb.Players[i].ACTION = PlayerAction.Wait;
        //    //            gb.Players[j].ACTION = PlayerAction.Wait;
        //    //        }
        //    //        if (tvitya2.X == gb.Players[i].X && tvitya2.Y == gb.Players[i].Y && tvitya1.X == gb.Players[i].X && tvitya1.Y == gb.Players[i].Y)
        //    //        {
        //    //            gb.Players[i].ACTION = PlayerAction.Wait;
        //    //            gb.Players[j].ACTION = PlayerAction.Wait;
        //    //        }
        //    //    }
        //    //}

        //    //for (int i = 0; i < tempplayers.Count; i++)
        //    //{
        //    //    var tvitya1 = tempplayers[i];
        //    //    for (int j = i + 1; j < tempplayers.Count; j++)
        //    //    {
        //    //        var tvitya2 = tempplayers[j];
        //    //        if (tvitya1.X == tvitya2.X && tvitya1.Y == tvitya2.Y)
        //    //        {
        //    //            if (usersInfo.Find(c => c.player == tvitya1).prioritet > usersInfo.Find(c => c.player == tvitya2).prioritet)
        //    //            {
        //    //              //  gb.Players[i].ACTION = PlayerAction.Wait;
        //    //                gb.Players[j].ACTION = PlayerAction.Wait;
        //    //            }
        //    //            else
        //    //            {
        //    //                gb.Players[i].ACTION = PlayerAction.Wait;
        //    //            }
        //    //        }
        //    //        //if (tvitya2.X == gb.Players[i].X && tvitya2.Y == gb.Players[i].Y && tvitya1.X == gb.Players[i].X && tvitya1.Y == gb.Players[i].Y)
        //    //        //{
        //    //        //    gb.Players[i].ACTION = PlayerAction.Wait;
        //    //        //    gb.Players[j].ACTION = PlayerAction.Wait;
        //    //        //}
        //    //    }
        //    //}


        //    for (int i = 0; i < tempplayers.Count; i++)
        //    {
        //        var tvitya1 = tempplayers[i];
        //        for (int j = i + 1; j < tempplayers.Count; j++)
        //        {
        //            var tvitya2 = tempplayers[j];
        //            if (tvitya1.X == tvitya2.X && tvitya1.Y == tvitya2.Y)
        //            {
        //                if (Prioritets[i] > Prioritets[j])
        //                {
        //                    // gb.Players[i].ACTION = PlayerAction.Wait;
        //                    gb.Players[j].ACTION = PlayerAction.Wait;
        //                }
        //                else
        //                {
        //                    gb.Players[i].ACTION = PlayerAction.Wait;
        //                }
        //            }
        //            //if (tvitya2.X == gb.Players[i].X && tvitya2.Y == gb.Players[i].Y && tvitya1.X == gb.Players[i].X && tvitya1.Y == gb.Players[i].Y)
        //            //{                       
        //            //    gb.Players[i].ACTION = PlayerAction.Wait;
        //            //    gb.Players[j].ACTION = PlayerAction.Wait;
        //            //}
        //        }
        //    }


        //    for (int i = 0; i < gb.Players.Count; i++)
        //    {
        //        if (gb.Players[i].Health > 0)
        //        {
        //            PlayerMove(gb.Players[i]);
        //        }
        //    }
        //}


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
            int tx = pplayer.X;
            int ty = pplayer.Y;

            switch (pplayer.ACTION)
            {
                case PlayerAction.Right:
                    tx = pplayer.X + 1;
                    ty = pplayer.Y;

                    if (tx > gb.W - 1 || gb.Cells[tx, ty].Type != CellType.None)
                    {
                        break;
                    }

                    if (tbombs_mass[tx, ty] != null || tplayer_mass[tx, ty] != null)
                    {
                        break;
                    }

                    pplayer.X++;
                    break;

                case PlayerAction.Left:
                    tx = pplayer.X - 1;
                    ty = pplayer.Y;

                    if (tx < 0 || gb.Cells[tx, ty].Type != CellType.None)
                    {
                        break;
                    }

                    if (tbombs_mass[tx, ty] != null || tplayer_mass[tx, ty] != null)
                    {
                        break;
                    }

                    pplayer.X--;
                    break;

                case PlayerAction.Down:
                    tx = pplayer.X;
                    ty = pplayer.Y + 1;

                    if (ty + 1 > gb.H - 1 || gb.Cells[tx, ty].Type != CellType.None)
                    {
                        break;
                    }

                    if (tbombs_mass[tx, ty] != null || tplayer_mass[tx, ty] != null)
                    {
                        break;
                    }

                    pplayer.Y++;
                    break;

                case PlayerAction.Up:
                    tx = pplayer.X;
                    ty = pplayer.Y - 1;

                    if (pplayer.Y - 1 < 0 || gb.Cells[tx, ty].Type != CellType.None)
                    {
                        break;
                    }

                    if (tbombs_mass[tx, ty] != null || tplayer_mass[tx, ty] != null)
                    {
                        break;
                    }

                    pplayer.Y--;
                    break;

                case PlayerAction.Bomb:
                    if (pplayer.BombsCount > 0 && tbombs_mass[tx,ty]==null)
                    {
                        CreateBomb(pplayer);
                        pplayer.BombsCount--;
                    }
                    break;

                case PlayerAction.Wait:
                    break;
            }
        }

        ///// <summary>
        ///// Выстрел игрока по возможности
        ///// </summary>
        ///// <param name="pplayer"></param>
        //public void PlayerFire(Player pplayer)
        //{
        //    if (pplayer.ACTION == PlayerAction.Bomb)
        //    {
        //        if (pplayer.BombsCount > 0)
        //        {
        //            CreateBomb(pplayer);
        //            pplayer.BombsCount--;
        //        }
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
                    throw new Exception($"{usersInfo[i].player.Name} Disconnect ERROR: {er.Message}");

                }
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
        /// Попытаться скомпилировать и запустить стратегию пользователя
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Compiler MoveStartUserExe(string php_exe_path, int i)
        {
            try
            {
                Compiler compiler = new Compiler(php_exe_path, i, sandboxgame.id, GameType);
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
        /// Обработка передаваемой и сохраняемой информации об игре в текущий Тик
        /// </summary>
        public void GameboardInfoProccess()
        {
            tempGB = null;
            tempGB = (GameBoard)gb.Clone();
            GameBoard tempGBmin = (GameBoard)gb.Clone();
            tempGBmin.XYinfo = null;
            gameBoardStates.Add(tempGBmin);
            SetXYInfo();
            gameboardjson = JsonConvert.SerializeObject(tempGB);
        }


        /// <summary>
        /// Отправить Клиентам инофрмацию об Игровом мире (объект класса GameBoard)
        /// </summary>
        public void CommunicateWithClients()
        {
            int lenght = gameboardjson.Length;
            gameboardjson = Helper.CompressString(gameboardjson);
          //  Helper.LOG(Compiler.LogPath, $"gameboardjson {lenght} {gameboardjson.Length}");

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

                    SendMessage(tempUserInfo.client.GetStream(), gameboardjson.Length + "");
                    ReceiveMessage(strm);

                    SendMessage(tempUserInfo.client.GetStream(), gameboardjson);
                    ReceiveMessage(strm);


                    userjson = Helper.CompressString(userjson);
                    Helper.LOG(Compiler.LogPath, $"SEND {tempUserInfo.player.Name}");

                    SendMessage(tempUserInfo.client.GetStream(), userjson);
                    
                    string action = ReceiveMessage(strm);
                    tempUserInfo.player.ACTION = Helper.DecryptAction(action);
                    Helper.LOG(Compiler.LogPath, $"RECIEVE {tempUserInfo.player.Name} action = {tempUserInfo.player.ACTION}");
                }
                catch (IOException er)
                {
                    Helper.LOG(Compiler.LogPath, $"Игрок {usersInfo[i].player.Name} превысил ограничение по времени : {er.Message}");                
                    PlayerDisconnect(usersInfo[i].player);
                    usersInfo[i].player.ACTION = PlayerAction.Wait;
                }
                catch (Exception er)
                {
                    Helper.LOG(Compiler.LogPath, "CommunicateWithClients ERROR: " + er.Message);
                    PlayerDisconnect(usersInfo[i].player);
                    usersInfo[i].player.ACTION = PlayerAction.Wait;
                }
            }
        }


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
                DestroyedCells[i].Type = CellType.None;
            }
            DestroyedCells.Clear();
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



        public void SetPrioritets()
        {

            while (Prioritets.Count < gb.Players.Count)
            {
                int prioritet = rn.Next(0, gb.Players.Count);
                bool isExist = false;
                for (int i = 0; i < Prioritets.Count; i++)
                {
                    if (Prioritets[i] == prioritet)
                    {
                        isExist = true;
                        break;
                    }
                }

                if (isExist == false)
                {
                    Prioritets.Add(prioritet);
                }
            }
        }


    }
}
