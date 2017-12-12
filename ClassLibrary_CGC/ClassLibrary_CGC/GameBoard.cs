using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary_CGC
{
    public enum Action
    {
        Right, Left, Up, Down, Bomb
    }
    public enum BonusType
    {
        multiple, big
    }
    public enum BombType
    {
        multiple, big
    }
    public enum CellType
    {
        indestructible, destructible, free
    }


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
    }

    [Serializable]
    public class Cell : GameObject
    {

    }

    [Serializable]
    public class Cell_indestructible : Cell
    {

    }

    [Serializable]
    public class Cell_destructible : Cell
    {

    }

    [Serializable]
    public class Lava : GameObject
    {
        int liveTime;
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
    public class GameBoard
    {
        int w, h;
        Cell[,] cells;
        List<Player> players;
        List<Bonus> bonuses;
        List<Bomb> bombs;
        List<Lava> lavas;

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

        public List<Bonus> Bonuses
        {
            get
            {
                return bonuses;
            }
            set
            {
                bonuses = value;
            }
        }

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

        public Action Action
        {
            get;
            set;
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
    public class Bonus_multiple : Bonus
    {

    }

    [Serializable]
    public class Bonus_big : Bonus
    {

    }

    [Serializable]
    public class Bomb : GameObject
    {
        int liveTime;

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
    public class Bomb_multiple : Bomb
    {

    }

    [Serializable]
    public class Bomb_big : Bomb
    {

    }

}