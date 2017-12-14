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
            gb = new GameBoard();
            g = panel1.CreateGraphics();           

        }

        public int cw = 30;
        Graphics g;
        Pen p;
        SolidBrush sb;
        GameBoard gb;

        private void panel1_Paint(object sender, PaintEventArgs e)
        {           
            for (int i = 0; i < gb.Cells.GetLength(0); i++)
            {
                for (int j = 0; j < gb.Cells.GetLength(1); j++)
                {
                    var tcell = gb.Cells[i, j];
                    if (tcell is Cell_indestructible)
                    {
                        PaintRect(tcell.X, tcell.Y, Color.Black);
                    }
                    else if (tcell is Cell_destructible)
                    {
                        PaintRect(tcell.X, tcell.Y, Color.Bisque);
                      //  g.FillEllipse(new SolidBrush(Color.Red), tcell.X * cw, tcell.Y * cw, cw, cw);

                    }
                }
            }

            for (int i = 0; i < gb.Bonuses.Count; i++)
            {
                var tbonus = gb.Bonuses[i];
                PaintElliple(tbonus.X, tbonus.Y, Color.Red);
            }
          


            DrawGrid();

            //Thread.Sleep(103);
            //panel1.Refresh();
        }


        public void PaintElliple(int x, int y, Color cl)
        {
            sb = new SolidBrush(cl);
            g.FillEllipse(new SolidBrush(cl), x * cw, y * cw, cw, cw);
        }


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


        public GameBoard(int _size,  int _players_count)
        {
            W = _size;
            H = _size;
        }

        public GameBoard()
        {
            int size = 15;
            W = size;
            H = size;
            GenerateBoard(size);
            GenerateBonuses(3);

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
                    Cells[i, j] = new Cell();
                    Cells[i, j].X = j;
                    Cells[i, j].Y = i;
                }
            }

            for (int i = 2; i < Cells.GetLength(0) - 2; i+=2)
            {
                int j = (Cells.GetLength(1) / 2);
                Cells[i, j] = new Cell_destructible();

                Cells[i, j].X = i;
                Cells[i, j].Y = j;

                j = 0;
                Cells[i, j] = new Cell_destructible();

                Cells[i, j].X = i;
                Cells[i, j].Y = j;

                j = Cells.GetLength(1) - 1;
                Cells[i, j] = new Cell_destructible();

                Cells[i, j].X = i;
                Cells[i, j].Y = j;
            }

            for (int j = 2; j < Cells.GetLength(1) - 2; j+=2)
            {
                int i = (Cells.GetLength(0) / 2);
                Cells[i, j] = new Cell_destructible();

                Cells[i, j].X = i;
                Cells[i, j].Y = j;

                i = 0;
                Cells[i, j] = new Cell_destructible();

                Cells[i, j].X = i;
                Cells[i, j].Y = j;

                i = Cells.GetLength(0) - 1;
                Cells[i, j] = new Cell_destructible();

                Cells[i, j].X = i;
                Cells[i, j].Y = j;
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
                        Cells[i, j] = new Cell_destructible();
                        Cells[i, j].X = j;
                        Cells[i, j].Y = i;

                        Cells[ii, jj] = new Cell_destructible();
                        Cells[ii, jj].X = jj;
                        Cells[ii, jj].Y = ii;

                        Cells[ii, j] = new Cell_destructible();
                        Cells[ii, j].X = j;
                        Cells[ii, j].Y = ii;

                        Cells[i, jj] = new Cell_destructible();
                        Cells[i, jj].X = jj;
                        Cells[i, jj].Y = i;
                    }                    
                }
            }

            for (int i = 1; i < Cells.GetLength(0)-1; i+=2)
            {
                for (int j = 1; j < Cells.GetLength(1)-1; j+=2)
                {
                    Cells[i, j] = new Cell_indestructible();
                    Cells[i, j].X = j;
                    Cells[i, j].Y = i;
                }
            }
        }





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


            for (int i = 0; i < bonuses_count; i++)
            {
                int rpoint = rn.Next(0, cells_dest.Count);
                int rtype = rn.Next(0, 2);
                Bonus tbonus;
                if (rtype%2==0)
                {
                    tbonus = new Bonus_multiple(cells_dest[rpoint].X, cells_dest[rpoint].Y);
                    bonuses.Add(tbonus);

                    bonuses.Add(tbonus);

                    bonuses.Add(tbonus);

                    bonuses.Add(tbonus);
                }
                else
                {
                    tbonus = new Bonus_big(cells_dest[rpoint].X, cells_dest[rpoint].Y);
                    bonuses.Add(tbonus);

                    bonuses.Add(tbonus);

                    bonuses.Add(tbonus);

                    bonuses.Add(tbonus);
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
                //List<Bonus> visible_bombs = new List<Bonus>();

                //for (int i = 0; i < bonuses.Count; i++)
                //{
                //    if (bonuses[i].Visible == true)
                //    {
                //        visible_bombs.Add(bonuses[i]);
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
       public Bonus_multiple(int x, int y)
        {
            this.X = x;
            this.Y = y;
            Visible = false;
        }


    }

    [Serializable]
    public class Bonus_big : Bonus
    {
        public Bonus_big(int x, int y)
        {
            this.X = x;
            this.Y = y;
            Visible = false;
        }
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
