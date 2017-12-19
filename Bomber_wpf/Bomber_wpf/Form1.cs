using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bomber_wpf
{
    public enum Action
    {
        Right, Left, Up, Down, Bomb, wait
    }

    public enum BonusType
    {
       Big, Fast, None, All
    }

    public partial class Form1 : Form
    {

        private readonly SynchronizationContext synchronizationContext;
        private DateTime previousTime = DateTime.Now;


        public int cw = 30;
        Graphics g;
        Pen p;
        SolidBrush sb;
        GameBoard gb;

        public Form1()
        {
            InitializeComponent();
            g = panel1.CreateGraphics();
            p = new Pen(Color.Black);
            sb = new SolidBrush(Color.DimGray);

            gb = new GameBoard();


            for (int i = 0; i < gb.W; i++)
            {
                gb.Cells[i, 0] = new Cell_free()
                {
                    X = i,
                    Y = 0
                };
                gb.Cells[i, 1] = new Cell_indestructible()
                {
                    X = i,
                    Y = 1
                };
            }

            

            Bot vitya = new Bot()
            {               
                ID = 0,
                X = 14,
                Y = 0,               
                ReloadTime = 0,
                Color = Color.Purple               
            };
            
            Bot yura = new Bot()
            {               
                ID = 1,
                X = 5,
                Y = 0,               
                ReloadTime = 0,
                Color = Color.Blue               
            };

            gb.Players.Add(vitya);
            gb.Players.Add(yura);



            Bonus tmb = new Bonus_big(8, 0)
            {
                Visible = true
            };
            Bonus tmb1 = new Bonus_fast(7, 0)
            {
                Visible = true
            };
            gb.Bonuses.Add(tmb);
            gb.Bonuses.Add(tmb1);

            Cell cl = new Cell_destructible()
            {
                X = 3,
                Y = 0
            };
            gb.Cells[3, 0] = cl;

            Bomb bmb = new Bomb()
            {
                X = 2,
                Y = 0,
                PlayerID = 0
            };
            gb.Bombs.Add(bmb);

            players_ListBox.Items.Add(test);
        }

        int test = 0;
        

        

        public void UpdatePlayerInfoOnDisplay(int value)
        {
            var timeNow = DateTime.Now;

            if ((DateTime.Now - previousTime).Milliseconds <= 50) return;

            synchronizationContext.Post(new SendOrPostCallback(o =>
            {
                players_ListBox.Text = o + "";
            }), value);

            previousTime = timeNow;
        }

        public async void testt()
        {
            await Task.Run(() =>
            {
                
               // UpdatePlayerInfoOnDisplay(test);
            });
        }


        public  void panel1_Paint(object sender, PaintEventArgs e)
        {
            test++;
            if (test == 10)
            {
                testt();
            }

        

            DrawCells();

            LavasProccess();
            PlayerProcess();

            PlayerBonusCollision();

            BonusesProccess();

            PaintPlayers();
            BombsProccess();

            DrawGrid();
            Thread.Sleep(15);
            panel1.Refresh();
        }

        public void PlayerProcess()
        {
            List<Player> tempplayers = new List<Player>();
            for (int i = 0; i < gb.Players.Count; i++)
            {
                var tvitya = gb.Players[i];
                
               tvitya.Play();
               
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
                        gb.Players[i].ACTION = Action.wait;
                        gb.Players[j].ACTION = Action.wait;
                    }
                    if (tvitya2.X == gb.Players[i].X  && tvitya2.Y == gb.Players[i].Y && tvitya1.X == gb.Players[i].X && tvitya1.Y == gb.Players[i].Y)
                    {
                        gb.Players[i].ACTION = Action.wait;
                        gb.Players[j].ACTION = Action.wait;
                    }

                }
            }

            for (int i = 0; i < gb.Players.Count; i++)
            {
                PlayerMove(gb.Players[i]);

            }
        }



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




        public Bomb[,] ListToMass(List<Bomb> pbombs)
        {
            Bomb[,] tbombs_mass = new Bomb[gb.W, gb.H];

            for (int i = 0; i < pbombs.Count; i++)
            {
                tbombs_mass[pbombs[i].X, pbombs[i].Y] = pbombs[i];
            }
            return tbombs_mass;
        }

        public Player[,] ListToMass(List<Player> pplayers)
        {
            Player[,] tplayers_mass = new Player[gb.W, gb.H];

            for (int i = 0; i < pplayers.Count; i++)
            {

                tplayers_mass[pplayers[i].X, pplayers[i].Y] = pplayers[i];
            }
            return tplayers_mass;
        }

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
                case Action.Right:
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

                case Action.Left:
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

                case Action.Down:
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

                case Action.Up:
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
                case Action.wait:
                    break;
            }
        }


        public void PlayerFire(Player pplayer)
        {
            if (pplayer.ACTION == Action.Bomb)
            {
                if (pplayer.ReloadTime < 1)
                {
                    CreateBomb(pplayer);
                    PlayerReload(pplayer);                   
                }
            }
            pplayer.ReloadTime--;
        }

        public void PaintPlayers()
        {
            for (int i = 0; i < gb.Players.Count; i++)
            {
                var tplayer = gb.Players[i];
                PaintEllipse(tplayer.X, tplayer.Y, tplayer.Color);
            }
        }


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

   


        public void CreateBomb(Player _player)
        {
            Bomb tbomb = new Bomb()
            {
                LiveTime = CONST.bomb_live_time,
                PlayerID = _player.ID,
                X = _player.X,
                Y = _player.Y
            };
            gb.Bombs.Add(tbomb);
        }

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

        public void PlayerAddPointsKill(Lava plava)
        {
            for (int i = 0; i < gb.Players.Count; i++)
            {
                var tplayer = gb.Players[i];
                if (tplayer.ID == plava.PlayerID)
                {
                    tplayer.Points += CONST.player_kill_points;
                    break;
                }
            }
        }

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


        public void LavaPlayersCollision(Lava plava, int i)
        {
            for (int k = 0; k < gb.Players.Count; k++)
            {
                var tplayer = gb.Players[k];

                if (tplayer.X == i && tplayer.Y == plava.Y)
                {
                    PlayerAddPointsKill(plava);
                    tplayer.Health = 0;
                    gb.Players.Remove(tplayer);
                    gb.DeadPlayers.Add(tplayer);
                }
            }
        }


        public void LavaCollision(Lava plava)
        {
            Bonus[,] tbonuses_mass = ListToMass(gb.Bonuses);
            
            for (int i = plava.X - plava.Radius; i <= plava.X + plava.Radius; i++)
            {
                if (i < 0 || i > gb.W - 1 || gb.Cells[i, plava.Y] is Cell_indestructible)
                {
                    continue;
                }

                LavaPlayersCollision(plava, i);
               

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

                LavaPlayersCollision(plava, j);

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
        Action action;
        int points;
        BonusType bonusType;

        int reloadTime;


        public Player()
        {
            Health = 1;
            ACTION = Action.wait;
            ReloadTime = CONST.player_reload;
            BonusType = BonusType.None;
        }

        public Player(int ID)
        {
            this.ID = ID;
            Health = 1;
            ACTION = Action.wait;
            ReloadTime = CONST.player_reload;
            BonusType = BonusType.None;
        }

        public Player(int ID, string NAME)
        {
            this.ID = ID;
            this.Name = NAME;
            Health = 1;
            ACTION = Action.wait;
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
    

       public Action ACTION
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
                    ACTION = Action.wait;
                    break;
                case 1:
                    
                    ACTION = Action.Bomb;
                    break;
                case 2:
                    ACTION = Action.Right;
                    break;
                case 3:
                    ACTION = Action.Left;
                    break;
                case 4:
                    ACTION = Action.Up;
                    break;
                case 5:
                    ACTION = Action.Down;
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
            this.Color = Color.Transparent;
        }
    }

    [Serializable]
    public class Lava : GameObject
    {
        int liveTime;

        int playerID;
        int radius;

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

}
