﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace ClassLibrary_CGC
{
    public enum PlayerAction
    {
        Right, Left, Up, Down, Bomb, Wait
    }

    public enum BonusType
    {
        BigBomb, Ammunition, None, All
    }


    [Serializable]
    public class GameBoard : ICloneable
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
            GenerateBonuses(Config.bonuses_count);
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
            GenerateBonuses(Config.bonuses_count);
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

            for (int i = 2; i < Cells.GetLength(0) - 2; i += 2)
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

            for (int j = 2; j < Cells.GetLength(1) - 2; j += 2)
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

            for (int i = 1; i < Cells.GetLength(0) - 1; i += 2)
            {
                for (int j = 1; j < Cells.GetLength(1) - 1; j += 2)
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

            for (int i = 0; i < W / 2; i++)
            {
                for (int j = 0; j < H / 2; j++)
                {
                    if (Cells[i, j] is Cell_destructible)
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
                if (rtype % 2 == 0)
                {
                    tbonus = new Bonus_fast(cells_dest[rpoint].X, cells_dest[rpoint].Y);
                    bonuses.Add(tbonus);

                    tbonus = new Bonus_fast(W - cells_dest[rpoint].X - 1, cells_dest[rpoint].Y);
                    bonuses.Add(tbonus);

                    tbonus = new Bonus_fast(cells_dest[rpoint].X, H - cells_dest[rpoint].Y - 1);
                    bonuses.Add(tbonus);

                    tbonus = new Bonus_fast(W - cells_dest[rpoint].X - 1, H - cells_dest[rpoint].Y - 1);
                    bonuses.Add(tbonus);
                }
                else
                {
                    tbonus = new Bonus_big(cells_dest[rpoint].X, cells_dest[rpoint].Y);
                    bonuses.Add(tbonus);

                    tbonus = new Bonus_big(W - cells_dest[rpoint].X - 1, cells_dest[rpoint].Y);
                    bonuses.Add(tbonus);

                    tbonus = new Bonus_big(cells_dest[rpoint].X, H - cells_dest[rpoint].Y - 1);
                    bonuses.Add(tbonus);

                    tbonus = new Bonus_big(W - cells_dest[rpoint].X - 1, H - cells_dest[rpoint].Y - 1);
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
                List<Bonus> visible_bombs = new List<Bonus>();

                for (int k = 0; k < bonuses.Count; k++)
                {
                    if (bonuses[k].Visible == true)
                    {
                        visible_bombs.Add(bonuses[k]);
                    }
                }
                return visible_bombs;

              //  return bonuses;
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

        public object Clone()
        {
            GameBoard nGameBoard = new GameBoard();
            nGameBoard =(GameBoard) this.MemberwiseClone();


            nGameBoard.Cells = new Cell[nGameBoard.W, nGameBoard.H];

            for (int i = 0; i < nGameBoard.Cells.GetLength(0); i++)
            {
                for (int j = 0; j < nGameBoard.Cells.GetLength(1); j++)
                {
                    if (this.Cells[i, j] is Cell_destructible)
                    {
                        nGameBoard.Cells[i, j] = new Cell_destructible();
                    }
                    else if (this.Cells[i, j] is Cell_indestructible)
                    {
                        nGameBoard.Cells[i, j] = new Cell_indestructible();
                    }
                    else
                    {
                        nGameBoard.Cells[i, j] = new Cell_free();
                    }
                    nGameBoard.Cells[i, j].X = i;
                    nGameBoard.Cells[i, j].Y = j;
                }
            }

            nGameBoard.Bonuses = new List<Bonus>();

            for (int i = 0; i < this.Bonuses.Count; i++)
            {
                Bonus nbonus;
                if (this.Bonuses[i] is Bonus_big)
                {
                    nbonus = new Bonus_big(this.Bonuses[i].X, this.Bonuses[i].Y);                    
                }
                else
                {
                    nbonus = new Bonus_fast(this.Bonuses[i].X, this.Bonuses[i].Y);
                }
                nbonus.Visible = this.Bonuses[i].Visible;
                nbonus.Color = this.Bonuses[i].Color;
                nGameBoard.Bonuses.Add(nbonus);
            }

            nGameBoard.Bombs = new List<Bomb>();

            for (int i = 0; i < this.Bombs.Count; i++)
            {
                Bomb nbomb;
                if (this.Bombs[i] is Bomb_big)
                {
                    nbomb = new Bomb_big();
                }
                else
                {
                    nbomb = new Bomb();
                }
                nbomb.X = this.Bombs[i].X;
                nbomb.Y = this.Bombs[i].Y;
                nbomb.Color = this.Bombs[i].Color;
                nbomb.PlayerID = this.Bombs[i].PlayerID;
                nbomb.LiveTime = this.Bombs[i].LiveTime;

                nGameBoard.Bombs.Add(nbomb);
            }

            nGameBoard.Lavas = new List<Lava>();

            for (int i = 0; i < this.Lavas.Count; i++)
            {
                var tlava = this.Lavas[i];
                Lava nlava = new Lava();
                nlava.Color = tlava.Color;
                nlava.LiveTime = tlava.LiveTime;
                nlava.PlayerID = tlava.PlayerID;
                nlava.Radius = tlava.Radius;
                nlava.X = tlava.X;
                nlava.Y = tlava.Y;
                nGameBoard.Lavas.Add(nlava);
            }


            nGameBoard.Players = new List<Player>();

            for (int i = 0; i < this.Players.Count; i++)
            {
                Player nplayer = new Player();              

                nplayer.X = this.Players[i].X;
                nplayer.Y = this.Players[i].Y;
                nplayer.Health = this.Players[i].Health;
                nplayer.ID = this.Players[i].ID;
                nplayer.Name = this.Players[i].Name;
                nplayer.Points = this.Players[i].Points;
                nplayer.BombsCount = this.Players[i].BombsCount;
                nplayer.BonusType = this.Players[i].BonusType;
                nplayer.Color = this.Players[i].Color;
                nplayer.ACTION = this.Players[i].ACTION;

                nGameBoard.Players.Add(nplayer);
            }

            nGameBoard.DeadPlayers = new List<Player>();

            for (int i = 0; i < this.DeadPlayers.Count; i++)
            {
                Player nplayer = new Player();
                nplayer.X = this.DeadPlayers[i].X;
                nplayer.Y = this.DeadPlayers[i].Y;
                nplayer.Health = this.DeadPlayers[i].Health;
                nplayer.ID = this.DeadPlayers[i].ID;
                nplayer.Name = this.DeadPlayers[i].Name;
                nplayer.Points = this.DeadPlayers[i].Points;
                nplayer.BombsCount = this.DeadPlayers[i].BombsCount;
                nplayer.BonusType = this.DeadPlayers[i].BonusType;
                nplayer.Color = this.DeadPlayers[i].Color;
                nplayer.ACTION = this.DeadPlayers[i].ACTION;

                nGameBoard.DeadPlayers.Add(nplayer);
            }

            return nGameBoard;

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
            this.Color = Config.bonus_fast;
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
            this.Color = Config.bonus_big;
        }
    }

    [Serializable]
    public class Bomb : GameObject
    {
        int liveTime;
        int playerID;

        public Bomb()
        {
            this.Color = Config.bomb_color;
        }

        public Bomb(int x, int y)
        {
            this.X = x;
            this.Y = y;
            this.Color = Config.bomb_color;
        }

        public Bomb(int x, int y, Player pplayer)
        {
            this.X = x;
            this.Y = y;
            this.Color = Config.bomb_color;
            this.playerID = pplayer.ID;
        }

        public Bomb(Player pplayer)
        {
            this.X = pplayer.X;
            this.Y = pplayer.Y;
            this.Color = Config.bomb_color;
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


    [Serializable]
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
            this.Color = Config.cell_indestructible_color;
        }
    }

    [Serializable]
    public class Cell_destructible : Cell
    {
        public Cell_destructible()
        {
            this.Color = Config.cell_destructible_color;
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

        public Lava()
        {
            this.Color = Config.lava_color;
            this.LiveTime = Config.lava_livetime;
        }
        public Lava(Bomb pbomb)
        {
            this.Color = Config.lava_color;
            this.LiveTime = Config.lava_livetime;
            if (pbomb is Bomb_big)
            {
                this.Radius = Config.lava_radius_big;
            }
            else
            {
                this.Radius = Config.lava_radius;
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
    public class Player : GameObject
    {
        int health;
        int id;
        string name;
        PlayerAction action;
        int points;
        BonusType bonusType;

        int bombs_count;


        public Player()
        {
            Health = 1;
            ACTION = PlayerAction.Wait;
            BombsCount = Config.player_bombs_count_start;
            BonusType = BonusType.None;
        }

        public Player(int ID)
        {
            this.ID = ID;
            Health = 1;
            ACTION = PlayerAction.Wait;
            BombsCount = Config.player_bombs_count_start;
            BonusType = BonusType.None;
        }

        public Player(int ID, string NAME)
        {
            this.ID = ID;
            this.Name = NAME;
            Health = 1;
            ACTION = PlayerAction.Wait;
            BombsCount = Config.player_bombs_count_start;
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
                if (value.Length > 0)
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
                if (value > 0)
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

        public int BombsCount
        {
            get
            {
                return bombs_count;
            }
            set
            {
                if (value >= 0)
                {
                    bombs_count = value;
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
                if (value >= 0)
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


        public virtual PlayerAction Play()
        {
            return PlayerAction.Wait;
        }

        public virtual PlayerAction Play(GameBoard gb)
        {
            return PlayerAction.Wait;
        }
    }




    [Serializable]
    public class Bot : Player
    {
        public static Random rnd;

        public Bot()
        {
            rnd = new Random();
        }

        public override PlayerAction Play()
        {
            int rnumber = rnd.Next(0, 6);
            PlayerAction taction = PlayerAction.Wait;
            switch (rnumber)
            {
                case 0:
                    taction = PlayerAction.Wait;
                    break;
                case 1:

                    taction = PlayerAction.Bomb;
                    break;
                case 2:
                    taction = PlayerAction.Right;
                    break;
                case 3:
                    taction = PlayerAction.Left;
                    break;
                case 4:
                    taction = PlayerAction.Up;
                    break;
                case 5:
                    taction = PlayerAction.Down;
                    break;
            }
         //   taction = PlayerAction.Bomb;
            return taction;
        }
    }


    //[Serializable]
    //public class User : Player
    //{   
    //    public override PlayerAction Play()
    //    {
    //        PlayerAction taction = PlayerAction.Wait;


    //        //Код пользователя


    //        return taction;
    //    }
    //}



    /// <summary>
    /// Постоянные параметры игры
    /// </summary>
    [Serializable]
    public class Config
    {
        public static int gameTicksMax = 300;

        public static int bonuses_count = 3;
        public static int lava_radius = 1;
        public static int lava_radius_big = 2;
        public static int lava_livetime = 2;
        public static int bomb_live_time = 3;
        
        public static int player_health = 3;
        public static int player_bombs_count_start = 1;
        public static int player_bombs_count_max = 3;

        public static int player_kill_points = 20;
        public static int player_survive_points = 20;
        public static int player_cell_destroy_points = 1;
        public static int player_win_points = 100;
        public static int player_bonus_find_points = 10;


        public static Color cell_destructible_color = Color.Bisque;
        public static Color cell_indestructible_color = Color.Black;

        public static Color lava_color = Color.Orange;
        public static Color bomb_color = Color.Red;

        public static Color bonus_fast = Color.Yellow;
        public static Color bonus_big = Color.IndianRed;

    }




}