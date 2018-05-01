using System;
using System.Collections.Generic;
using System.Drawing;

namespace ClassLibrary_CGC
{
    public enum PlayerAction
    {
        Wait, Bomb, Left, Up, Right, Down
    }

    public enum BonusType
    {
        Radius, Ammunition
    }

    public enum CellType
    {
        Free, Indestructible, Destructible
    }


    [Serializable]
    public class GameBoard : ICloneable
    {
        private int w, h;
        private Cell[,] cells;
        private List<Player> players;
        private List<Bonus> bonuses;
        private List<Bomb> bombs;
        private List<Lava> lavas;
        public XYInfo[,] XYinfo = new XYInfo[15, 15];
        public int Tick = 0;

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


        public GameBoard (int [,] pole)
        {
            int size = 15;
            W = size;
            H = size;

            Cells = new Cell[size, size];
            Bonuses = new List<Bonus>();
            Bombs = new List<Bomb>();
            Lavas = new List<Lava>();
            Players = new List<Player>();

            for (int i = 0; i < XYinfo.GetLength(0); i++)
            {
                for (int j = 0; j < XYinfo.GetLength(1); j++)
                {
                    XYinfo[i, j] = new XYInfo();
                }
            }

            for (int i = 0; i < Cells.GetLength(0); i++)
            {
                for (int j = 0; j < Cells.GetLength(1); j++)
                {
                    Cells[i, j] = new Cell()
                    {
                        X = i,
                        Y = j,
                        Type = CellType.Free
                    };
                }
            }

            for (int i = 0; i < pole.GetLength(0); i++)
            {
                for (int j= 0; j< pole.GetLength(1); j++)
                {

                    switch (pole[j,i])
                    {
                        case 0:
                            break;

                        case 1:
                            Cells[i, j].Type = CellType.Indestructible;
                            break;

                        case 2:
                            Cells[i, j].Type = CellType.Destructible;
                            break;

                        case 3:
                            Cells[i, j].Type = CellType.Destructible;
                            Bonus bonus = new Bonus();                           
                            bonus.X = i;
                            bonus.Y = j;
                            bonus.Type = BonusType.Ammunition;
                            Bonuses.Add(bonus);
                            break;

                        case 4:
                            Cells[i, j].Type = CellType.Destructible;
                            Bonus tbonus = new Bonus();                            
                            tbonus.X = i;
                            tbonus.Y = j;
                            tbonus.Type = BonusType.Radius;
                            Bonuses.Add(tbonus);
                            break;

                        case 5:
                            Player player = new Player();
                            player.X = i;
                            player.Y = j;
                            Players.Add(player);
                            break;
                        case 6:
                            Bonus ttbonus = new Bonus();
                           ttbonus.Type = BonusType.Ammunition;
                            ttbonus.X = i;
                            ttbonus.Y = j;
                            ttbonus.Visible = true;
                            Bonuses.Add(ttbonus);
                            break;
                        case 7:
                            Bonus tttbonus = new Bonus();
                            tttbonus.Type = BonusType.Radius;
                            tttbonus.X = i;
                            tttbonus.Y = j;
                            Bonuses.Add(tttbonus);
                            break;
                    }
                }
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
                    Cells[i, j] = new Cell()
                    {
                        X = i,
                        Y = j,
                        Type = CellType.Free                        
                    };
                }
            }

            for (int i = 2; i < Cells.GetLength(0) - 2; i += 2)
            {
                int j = (Cells.GetLength(1) / 2);
                Cells[i, j].Type = CellType.Destructible;

                j = 0;
                Cells[i, j].Type = CellType.Destructible;

                j = Cells.GetLength(1) - 1;
                Cells[i, j].Type = CellType.Destructible;
            }

            for (int j = 2; j < Cells.GetLength(1) - 2; j += 2)
            {
                int i = (Cells.GetLength(0) / 2);
                Cells[i, j].Type = CellType.Destructible;

                i = 0;
                Cells[i, j].Type = CellType.Destructible;

                i = Cells.GetLength(0) - 1;
                Cells[i, j].Type = CellType.Destructible;
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
                        Cells[i, j].Type = CellType.Destructible;
                        Cells[ii, jj].Type = CellType.Destructible;
                        Cells[ii, j].Type = CellType.Destructible;
                        Cells[i, jj].Type = CellType.Destructible;
                    }
                }
            }

            for (int i = 1; i < Cells.GetLength(0) - 1; i += 2)
            {
                for (int j = 1; j < Cells.GetLength(1) - 1; j += 2)
                {
                    Cells[i, j].Type = CellType.Indestructible;
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
                    if (Cells[i, j].Type == CellType.Destructible)
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
                    tbonus = new Bonus();
                    tbonus.X = cells_dest[rpoint].X;
                    tbonus.Y = cells_dest[rpoint].Y;
                    tbonus.Type = BonusType.Ammunition;
                    Bonuses.Add(tbonus);

                    tbonus = new Bonus();
                    tbonus.X = W - cells_dest[rpoint].X - 1;
                    tbonus.Y = cells_dest[rpoint].Y;
                    tbonus.Type = BonusType.Ammunition;
                    Bonuses.Add(tbonus);

                    tbonus = new Bonus();
                    tbonus.X = cells_dest[rpoint].X;
                    tbonus.Y = H - cells_dest[rpoint].Y - 1;
                    tbonus.Type = BonusType.Ammunition;
                    Bonuses.Add(tbonus);

                    tbonus = new Bonus();
                    tbonus.X = W - cells_dest[rpoint].X - 1;
                    tbonus.Y = H - cells_dest[rpoint].Y - 1;
                    tbonus.Type = BonusType.Ammunition;
                    Bonuses.Add(tbonus);
                }
                else
                {
                    tbonus = new Bonus();
                    tbonus.X = cells_dest[rpoint].X;
                    tbonus.Y = cells_dest[rpoint].Y;
                    tbonus.Type = BonusType.Radius;
                    Bonuses.Add(tbonus);

                    tbonus = new Bonus();
                    tbonus.X = W - cells_dest[rpoint].X - 1;
                    tbonus.Y = cells_dest[rpoint].Y;
                    tbonus.Type = BonusType.Radius;
                    Bonuses.Add(tbonus);

                    tbonus = new Bonus();
                    tbonus.X = cells_dest[rpoint].X;
                    tbonus.Y = H - cells_dest[rpoint].Y - 1;
                    tbonus.Type = BonusType.Radius;
                    Bonuses.Add(tbonus);

                    tbonus = new Bonus();
                    tbonus.X = W - cells_dest[rpoint].X - 1;
                    tbonus.Y = H - cells_dest[rpoint].Y - 1;
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
                    w = value;
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
                    h = value;
                }
            }
        }


        /// <summary>
        /// Клетки на поле
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
        /// Список игроков
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
        /// Список видимых бонусов
        /// </summary>
        public List<Bonus> Bonuses
        {
            get
            {
                return bonuses;
                // return bonuses;
            }
            set
            {
                bonuses = value;
            }
        }

        /// <summary>
        /// Список бомб
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
        /// Список объектов Лава
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
            nGameBoard = (GameBoard)MemberwiseClone();

            nGameBoard.Cells = new Cell[nGameBoard.W, nGameBoard.H];

            for (int i = 0; i < nGameBoard.Cells.GetLength(0); i++)
            {
                for (int j = 0; j < nGameBoard.Cells.GetLength(1); j++)
                {
                    nGameBoard.Cells[i, j] = new Cell();
                    nGameBoard.Cells[i, j].X = i;
                    nGameBoard.Cells[i, j].Y = j;
                    nGameBoard.cells[i, j].Type = Cells[i, j].Type;
                }
            }

            nGameBoard.Bonuses = new List<Bonus>();

            for (int i = 0; i < Bonuses.Count; i++)
            {
                if (Bonuses[i].Visible == false)
                {
                    continue;
                }
                Bonus nbonus = new Bonus(Bonuses[i]);  
                nGameBoard.Bonuses.Add(nbonus);
            }

            nGameBoard.Bombs = new List<Bomb>();

            for (int i = 0; i < Bombs.Count; i++)
            {
                Bomb nbomb = new Bomb(Bombs[i]);
                nGameBoard.Bombs.Add(nbomb);
            }

            nGameBoard.Lavas = new List<Lava>();

            for (int i = 0; i < Lavas.Count; i++)
            {
                Lava nlava = new Lava(Lavas[i]);
                nGameBoard.Lavas.Add(nlava);
            }

            nGameBoard.Players = new List<Player>();

            for (int i = 0; i < Players.Count; i++)
            {
                Player nplayer = new Player(Players[i]);
                nGameBoard.Players.Add(nplayer);
            }

            nGameBoard.XYinfo = new XYInfo[15, 15];

            for (int i = 0; i < nGameBoard.XYinfo.GetLength(0); i++)
            {
                for (int j = 0; j < nGameBoard.XYinfo.GetLength(1); j++)
                {
                    nGameBoard.XYinfo[i, j] = new XYInfo(XYinfo[i, j]);
                }
            }

            return nGameBoard;
        }
    }



    /// <summary>
    /// Объект Бонус
    /// </summary>
    [Serializable]
    public class Bonus : GameObject
    {
        bool visible = false;

        BonusType type;

        public Bonus ()
        {
            visible = false;            
        }

        public Bonus(Bonus pbonus)
        {
            this.Visible = pbonus.visible;
            X = pbonus.X;
            Y = pbonus.Y;
            Type = pbonus.Type;
        }

        /// <summary>
        /// Тип Бонуса
        /// </summary>
        public BonusType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }

        /// <summary>
        /// Видимость Бонуса
        /// </summary>
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

    /// <summary>
    /// Объект Бомба
    /// </summary>
    [Serializable]
    public class Bomb : GameObject
    {
        int liveTime;
        string playerID;
        int bang_radius;

        public Bomb()
        {
            LiveTime = 0;
            PlayerID = "";
            Bang_radius = 0;          
        }

        public Bomb(Bomb origin)
        {

            X = origin.X;
            Y = origin.Y;           
            PlayerID = origin.PlayerID;
            LiveTime = origin.LiveTime;
            Bang_radius = origin.Bang_radius;
        }

        public Bomb(int x, int y)
        {
            X = x;
            Y = y;         
        }

        public Bomb(int x, int y, Player pplayer)
        {
            X = x;
            Y = y;           
            playerID = pplayer.ID;
            Bang_radius = pplayer.BangRadius;
        }

        public Bomb(Player pplayer)
        {
            X = pplayer.X;
            Y = pplayer.Y;          
            playerID = pplayer.ID;
            Bang_radius = pplayer.BangRadius;
        }

        /// <summary>
        /// ID игрока, который поставил эту Бомбу
        /// </summary>
        public string PlayerID
        {
            get
            {
                return playerID;
            }

            set
            {
                playerID = value;
            }
        }

        /// <summary>
        /// Количество Тиков до взрыва
        /// </summary>
        public int LiveTime
        {
            get
            {
                return liveTime;
            }

            set
            {
                if (value >= 0)
                    liveTime = value;
            }
        }

        /// <summary>
        /// Радиус взрыва 
        /// </summary>
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
    }


    [Serializable]
    public class GameObject
    {
        int x, y;

        public int X
        {
            get
            {
                return x;
            }
            set
            {
                if (value >= 0)
                    x = value;
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
                    y = value;
            }
        }

    }

    /// <summary>
    /// Клетки поля
    /// </summary>
    [Serializable]
    public class Cell : GameObject
    {
        CellType type;

        /// <summary>
        /// Тип клетки
        /// </summary>
        public CellType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }
    }


    /// <summary>
    /// Объект Лава
    /// </summary>
    [Serializable]
    public class Lava : GameObject
    {
        int liveTime;
        string playerID;

        public Lava()
        {
            PlayerID = "";
            LiveTime = 0;
            this.LiveTime = Config.lava_livetime;
        }

        public Lava(Lava origin)
        {
            this.LiveTime = origin.LiveTime;
            this.PlayerID = origin.PlayerID;
            this.X = origin.X;
            this.Y = origin.Y;
        }

        public Lava(Bomb pbomb)
        {
            this.LiveTime = Config.lava_livetime;
            this.PlayerID = pbomb.PlayerID;
        }

        /// <summary>
        /// ID игрока, которому принадлежит эта Лава
        /// </summary>
        public string PlayerID
        {
            get
            {
                return playerID;
            }
            set
            {

                playerID = value;
            }
        }

        /// <summary>
        /// Количество Тиков до исчезновения
        /// </summary>
        public int LiveTime
        {
            get
            {
                return liveTime;
            }
            set
            {
                if (value >= 0)
                    liveTime = value;
            }
        }
    }

    /// <summary>
    /// Игрок
    /// </summary>
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
            Name = "";
            ID = "";
            Points = 0;
            BangRadius = 0;
            Health = 1;
            ACTION = PlayerAction.Wait;
            BombsCount = Config.player_bombs_count_start;
        }

        public Player(Player origin)
        {
            this.X = origin.X;
            this.Y = origin.Y;
            this.Health = origin.Health;
            this.ID = origin.ID;
            this.Name = origin.Name;
            this.Points = origin.Points;
            this.BombsCount = origin.BombsCount;
            this.BangRadius = origin.BangRadius;
            this.ACTION = origin.ACTION;
        }


        public Player(string ID)
        {
            this.ID = ID;
            Health = 1;
            ACTION = PlayerAction.Wait;
            BombsCount = Config.player_bombs_count_start;
            BangRadius = Config.bang_start_radius;
        }


        public Player(string ID, string NAME)
        {
            this.ID = ID;
            this.Name = NAME;
            Health = 1;
            ACTION = PlayerAction.Wait;
            BombsCount = Config.player_bombs_count_start;
            BangRadius = Config.bang_start_radius;
        }

        /// <summary>
        /// Имя
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
            }
        }


        /// <summary>
        /// Количество Очков
        /// </summary>
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

        /// <summary>
        /// Радиус взрыва (зависит от количества поднятых бонусов)
        /// </summary>
        public int BangRadius
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

        /// <summary>
        /// Боезапас (зависит от количества поднятых бонусов)
        /// </summary>
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

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public string ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        /// <summary>
        /// Команда, которую хочет выполнить Игрок
        /// </summary>
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

        /// <summary>
        /// Количество здоровья (если 0, то мёртв)
        /// </summary>
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
                    health = value;
                }
            }
        }

        /// <summary>
        /// Задать команду
        /// </summary>
        /// <returns>Команда</returns>
        public virtual PlayerAction Play()
        {
            return PlayerAction.Wait;
        }

        /// <summary>
        /// Задать команду
        /// </summary>
        /// <returns>Команда</returns>
        public virtual PlayerAction Play(GameBoard gb)
        {
            return PlayerAction.Wait;
        }
    }

    /// <summary>
    /// Бот, совершающий случайное действие
    /// </summary>
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
            PlayerAction taction = (PlayerAction)rnumber;
            return taction;
        }
    }

    /// <summary>
    /// Объект, содержащий всю информацию о одной клетке и игровых объектах внутри неё
    /// </summary>
    [Serializable]
    public class XYInfo
    {
        Player player;
        Bonus bonus;
       List<Lava> lavas;
        Bomb bomb;

        public XYInfo()
        {
            Player = null;
            Bomb = null;
            Bonus = null;
            lavas = new List<Lava>();
        }

        public XYInfo(XYInfo origin)
        {
            Player = origin.Player;
            Bomb = origin.Bomb;
            Bonus = origin.Bonus;
            Lavas = origin.Lavas;
        }

        /// <summary>
        /// Игрок
        /// </summary>
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

        /// <summary>
        /// Бонус
        /// </summary>
        public Bonus Bonus
        {
            get
            {
                if (bonus != null && bonus.Visible == true)
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

        /// <summary>
        /// Лавы
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

        /// <summary>
        /// Бомба
        /// </summary>
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
    /// Конфигурация
    /// </summary>
    [Serializable]
    public class Config
    {
        public static int gameTicksMax = 300;
        public static int all_game_client_max_wait_timeout = 120000;
        public static int one_tick_client_wait_time = 2000;
        public static int client_program_memory_quote = 64;

        public static int bonuses_count = 3;
        public static int bang_start_radius = 1;
        public static int lava_livetime = 2;
        public static int bomb_live_time = 3;

        public static int player_bombs_count_start = 1;
        public static int player_kill_points = 20;
        public static int player_survive_points = 20;
        public static int player_cell_destroy_points = 1;
        public static int player_win_points = 100;
        public static int player_bonus_find_points = 4;

        public static Color cell_destructible_color = Color.Bisque;
        public static Color cell_indestructible_color = Color.Black;

        public static Color lava_color = Color.Orange;
        public static Color bomb_color = Color.Red;

        public static Color bonus_fast = Color.Yellow;
        public static Color bonus_big = Color.IndianRed;

        public Config ()
        {

        }
    }
}