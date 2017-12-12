using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Bomber_wpf
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            g = panel1.CreateGraphics();
            p = new Pen(Color.Black);
            sb = new SolidBrush(Color.DimGray);
        }

        public int cw = 30;
        Graphics g;
        Pen p;
        SolidBrush sb;


        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            g = panel1.CreateGraphics();
            p = new Pen(Color.Black);
            for (int i = 1; i < 15; i++)
            {
                g.DrawLine(p, i * cw, 0, i * cw, 15 * cw);
                g.DrawLine(p, 0, i * cw, 15 * cw, i * cw);

               
            }
            


            GameBoard gb = new GameBoard();

            for (int i = 1; i < gb.Cells.GetLength(0)-1; i++)
            {
                for (int j = 1; j < gb.Cells.GetLength(1)-1; j++)
                {
                    var tcell = gb.Cells[i, j];
                    if (tcell is Cell_indestructible)
                    {
                        PaintRect(tcell.X, tcell.Y, Color.Black);
                    }
                    else if (tcell is Cell_destructible)
                    {
                        PaintRect(tcell.X, tcell.Y, Color.Bisque);
                    }
                    
                }
            }


            //Thread.Sleep(10);
            //panel1.Refresh();
        }


        public void PaintRect(int x, int y, Color cl)
        {
            sb = new SolidBrush(cl);
            g.DrawRectangle(p, x * cw, y * cw, cw, cw);
            g.FillRectangle(sb, x * cw, y * cw, cw, cw);
        }

      
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

        public Random rn = new Random();


        public GameBoard(int _w, int _h, int _size, int _players_count)
        {
            W = _w;
            H = _h;
        }

        public GameBoard ()
        {
            int size = 15;
            W = size;
            H = size;
            GenerateBoard(size);
            // Players = new List<Player>(2);
            
        }


        public void GenerateBoard(int size)
        {
            Cells = new Cell[size, size];
            int temp_rn = 0;


            for (int i = 0; i < Cells.GetLength(0); i++)
            {
                for (int j = 0; j < Cells.GetLength(1); j++)
                {
                    Cells[i, j] = new Cell();
                    Cells[i, j].X = j;
                    Cells[i, j].Y = i;
                }
            }

            for (int i = 1; i < Cells.GetLength(0) - 1; i++)
            {
                for (int j = 1; j < Cells.GetLength(1) - 1; j++)
                {
                    temp_rn = rn.Next(0, 10);
                    if (temp_rn < 4)
                    {
                        Cells[i, j] = new Cell_destructible();
                        Cells[i, j].X = j;
                        Cells[i, j].Y = i;
                    }
                    else if (temp_rn > 8)
                    {
                        Cells[i, j] = new Cell_indestructible();
                        Cells[i, j].X = j;
                        Cells[i, j].Y = i;
                    }
                }
            }
         
        }


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
        int id;

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
