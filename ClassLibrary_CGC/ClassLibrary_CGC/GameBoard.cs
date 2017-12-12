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

    public class Cell : GameObject
    {
        public CellType CellType
        {
            get;
            set;
        }
    }

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




    public class GameBoard
    {
        int w, h;
        Cell[,] board;
        List<Player> players;
        List<Bonus> bonuses;
        List<Bomb> bombs;
        List<Lava> lavas;

        public int W
        {
            get {
                return w;
            }
            set
            {
                if (value>2)
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
    }


    public class Player:GameObject
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



    public class Bonus : GameObject
    {
        bool visible;

        public BonusType BonusType
        {
            get;
            set;
        }
    }

    public class Bomb: GameObject
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


        public BombType BombType
        {
            get;
            set;
        }
    }





}
