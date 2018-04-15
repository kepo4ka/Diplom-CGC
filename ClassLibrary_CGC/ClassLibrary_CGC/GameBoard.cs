using System;
using System.Collections.Generic;
using System.Drawing;

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
        List<Bonus> bonuses;
        List<Bomb> bombs;
        List<Lava> lavas;
        public XYInfo[,] XYinfo = new XYInfo[15,15];        

        Random rn = new Random();

        public GameBoard()
        {
            int size = 15;
            W = size;
            H = size;
            GenerateBoard(size);
           
            Bonuses = new List<Bonus>();
            Bombs = new List<Bomb>();
            Lavas = new List<Lava>();
            Players = new List<Player>();           

            GenerateBonuses(Config.bonuses_count);

            for (int i = 0; i < XYinfo.GetLength(0); i++)
            {
                for (int j = 0; j < XYinfo.GetLength(1); j++)
                {
                    XYinfo[i, j] = new XYInfo();
                }
            }
        }

        public GameBoard(int SIZE)
        {
            W = SIZE;
            H = SIZE;
            GenerateBoard(SIZE);
            Bonuses = new List<Bonus>();
            Bombs = new List<Bomb>();
            Lavas = new List<Lava>();
            Players = new List<Player>();          

            GenerateBonuses(Config.bonuses_count);
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
                    Bonuses.Add(tbonus);

                    tbonus = new Bonus_fast(W - cells_dest[rpoint].X - 1, cells_dest[rpoint].Y);
                    Bonuses.Add(tbonus);

                    tbonus = new Bonus_fast(cells_dest[rpoint].X, H - cells_dest[rpoint].Y - 1);
                    Bonuses.Add(tbonus);

                    tbonus = new Bonus_fast(W - cells_dest[rpoint].X - 1, H - cells_dest[rpoint].Y - 1);
                    Bonuses.Add(tbonus);
                }
                else
                {
                    tbonus = new Bonus_big(cells_dest[rpoint].X, cells_dest[rpoint].Y);
                    Bonuses.Add(tbonus);

                    tbonus = new Bonus_big(W - cells_dest[rpoint].X - 1, cells_dest[rpoint].Y);
                    Bonuses.Add(tbonus);

                    tbonus = new Bonus_big(cells_dest[rpoint].X, H - cells_dest[rpoint].Y - 1);
                    Bonuses.Add(tbonus);

                    tbonus = new Bonus_big(W - cells_dest[rpoint].X - 1, H - cells_dest[rpoint].Y - 1);
                    Bonuses.Add(tbonus);
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

        public object Clone()
        {
            GameBoard nGameBoard = new GameBoard();
            nGameBoard = (GameBoard)this.MemberwiseClone();


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
                if (this.bonuses[i].Visible == false)
                {
                    continue;
                }
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
                Bomb nbomb = new Bomb(this.Bombs[i]);
                nGameBoard.Bombs.Add(nbomb);
            }

            nGameBoard.Lavas = new List<Lava>();

            for (int i = 0; i < this.Lavas.Count; i++)
            {
                Lava nlava = new Lava(this.Lavas[i]);
                nGameBoard.Lavas.Add(nlava);
            }

            nGameBoard.Players = new List<Player>();

            for (int i = 0; i < this.Players.Count; i++)
            {
                Player nplayer = new Player(this.Players[i]);
                nGameBoard.Players.Add(nplayer);
            }

            nGameBoard.XYinfo = new XYInfo[15, 15];

            for (int i = 0; i < nGameBoard.XYinfo.GetLength(0); i++)
            {
                for (int j = 0; j < nGameBoard.XYinfo.GetLength(1); j++)
                {
                    nGameBoard.XYinfo[i, j] = new XYInfo(this.XYinfo[i, j]);
                }
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
        string playerID;
        int bang_radius;

        public Bomb()
        {
            LiveTime = 0;
            PlayerID = "null";
            Bang_radius = 0;
            this.Color = Config.bomb_color;
        }

        public Bomb(Bomb origin)
        {            
            
            this.X = origin.X;
            this.Y = origin.Y;
            this.Color = origin.Color;
            this.PlayerID = origin.PlayerID;
            this.LiveTime = origin.LiveTime;
            this.Bang_radius = origin.Bang_radius;
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
            this.Bang_radius = pplayer.Bang_radius;
        }

        public Bomb(Player pplayer)
        {
            this.X = pplayer.X;
            this.Y = pplayer.Y;
            this.Color = Config.bomb_color;
            this.playerID = pplayer.ID;
            this.Bang_radius = pplayer.Bang_radius;
        }


        public string PlayerID
        {
            get
            {
                return playerID;
            }

            set
            {
                if (value.Length>0 && value != "")
                    playerID = value;
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

        public int Bang_radius
        {
            get
            {
                return bang_radius;
            }
            set
            {
                if (value>=0)
                {
                    bang_radius = value;
                }
            }
        }
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
        string playerID;       

        public Lava()
        {
            PlayerID = "null";
            LiveTime = 0;
            this.Color = Config.lava_color;
            this.LiveTime = Config.lava_livetime;
        }

        public Lava(Lava origin)
        {

            this.Color = origin.Color;
            this.LiveTime = origin.LiveTime;
            this.PlayerID = origin.PlayerID;
            this.X = origin.X;
            this.Y = origin.Y;
        }


        public Lava(Bomb pbomb)
        {
            this.Color = Config.lava_color;
            this.LiveTime = Config.lava_livetime;
            this.PlayerID = pbomb.PlayerID;                       
        }       

     
        public string PlayerID
        {
            get
            {
                return playerID;
            }
            set
            {
                if (value.Length>0 && value!= "")
                    playerID = value;
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
        string id;
        string name;
        PlayerAction action;
        int points;
        int bang_radius;

        int bombs_count;

        public Player()
        {
            Name = "null";
            ID = "null";
            Points = 0;
            Bang_radius = 0;
            Health = 1;
            ACTION = PlayerAction.Wait;
            BombsCount = Config.player_bombs_count_start;            
        }

        public Player(Player origin)
        {
            this.X =origin.X;
            this.Y =origin.Y;
            this.Health =origin.Health;
            this.ID =origin.ID;
            this.Name =origin.Name;
            this.Points =origin.Points;
            this.BombsCount =origin.BombsCount;
            this.Bang_radius =origin.Bang_radius;
            this.Color =origin.Color;
            this.ACTION =origin.ACTION;
        }


        public Player(string ID)
        {
            this.ID = ID;
            Health = 1;
            ACTION = PlayerAction.Wait;
            BombsCount = Config.player_bombs_count_start;
            Bang_radius = Config.bang_start_radius;
        }


        public Player(string ID, string NAME)
        {
            this.ID = ID;
            this.Name = NAME;
            Health = 1;
            ACTION = PlayerAction.Wait;
            BombsCount = Config.player_bombs_count_start;
            Bang_radius = Config.bang_start_radius;
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
                if (value >= 0)
                {
                    points = value;
                }
            }
        }

        public int Bang_radius
        {
            get
            {
                return bang_radius;
            }
            set
            {
                if (value >= 0)
                {
                    bang_radius = value;
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

        public string ID
        {
            get
            {
                return id;
            }
            set
            {
                if (value.Length>0 && value != "")
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


    [Serializable]
    public class XYInfo
    {
        Player player;
        Bonus bonus;
        Lava lava;
        Bomb bomb;

        public XYInfo()
        {
            Player = null;
            Bomb = null;
            Bonus = null;
            Lava = null;
        }

        public XYInfo(XYInfo origin)
        {
            this.Player = origin.Player;
            this.Bomb = origin.Bomb;
            this.Bonus = origin.Bonus;
            this.Lava = origin.Lava;           
        }

        public Player Player
        {
            get
            {
                return player;
            }
            set
            {
                player = value;
            }
        }

        public Bonus Bonus
        {
            get
            {
                if (bonus.Visible == true)
                {
                    return bonus;
                }
                return null;
            }
            set
            {
                bonus = value;
            }
        }

        public Lava Lava
        {
            get
            {
                return lava;
            }
            set
            {
                lava = value;
            }
        }

        public Bomb Bomb
        {
            get
            {
                return bomb;
            }
            set
            {
                bomb = value;
            }
        }
    }
    

    /// <summary>
    /// Постоянные параметры игры
    /// </summary>
    [Serializable]
    public class Config
    {
        public static int gameTicksMax = 300;

        public static int bonuses_count = 3;
        public static int bang_start_radius = 1;
        public static int lava_livetime = 2;
        public static int bomb_live_time = 3;        
        
        public static int player_bombs_count_start = 1;

       // public static int player_kill_points = 20;
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