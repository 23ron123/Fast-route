using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace FastestMovement
{
    public partial class Form1 : Form
    {
        bool doevents =true ;
        #region class Tag for data of each button
        public class Tag
        {
            public Point place;
            public double distance;
            public bool wasChecked;

            public Tag(Point place, double distance)
            {
                this.place = place;
                this.distance = distance;
                this.wasChecked = false;
            }
        }
        #endregion
        #region Data
        Button[,] b = new Button[40,20];
        List<Tag> ring1 = new List<Tag>();
        List<Tag> ring2 = new List<Tag>();
        int w = 30;
        int padd = 5;
        bool swich = false;
        Button from, to;
        Point[] kivuns;
        Color[] colors = { Color.Magenta, Color.LightGreen, Color.LightBlue, Color.Orange };
        #endregion
        #region init the board :
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            create_shoshana();
            create_Board();
            create_obstacles(
            #region params
new Point(2, 2),
                 new Point(4, 0),
                 new Point(5, 1),
                 new Point(4, 2),
                 new Point(5, 3),
                 new Point(2, 3),
                 new Point(3, 4),
                 new Point(1, 5),
                 new Point(6, 5),
                 new Point(5, 6)
            #endregion
);

        }
        #endregion
        #region create Board
        private void create_Board()
        {
            for (int y = 0; y < b.GetLength(1); y++)
            {
                for (int x = 0; x < b.GetLength(0); x++)
                {
                    b[x, y] = new Button();
                    b[x, y].Padding = Padding.Empty;
                    b[x, y].Margin = Padding.Empty;
                    b[x, y].Size = new Size(w, w);
                    b[x, y].Show();
                    b[x, y].Left = x * w + padd;
                    b[x, y].Top = y * w + padd;
                    b[x, y].Text = x.ToString() + " " + y.ToString();
                    b[x, y].ForeColor = Color.Gray;
                    b[x, y].Tag = new Tag(new Point(x, y), double.MaxValue);
                    this.Controls.Add(b[x, y]);
                    b[x, y].Click += new EventHandler(Form1_Click);
                    b[x, y].MouseDown += new MouseEventHandler(mouse_down);
                    //b[x, y].mouse += new EventHandler(Form1_Click);
                }
            }

        }
        void mouse_down(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (btn.BackColor == Color.Red)
                {
                    btn.BackColor = Control.DefaultBackColor;
                    btn.FlatStyle = FlatStyle.Standard;
                    btn.UseVisualStyleBackColor = true;
                }
                else
                {
                    btn.BackColor = Color.Red;
                }
            }
        }
        void Form1_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;


            swich = !swich;
            if (swich)
            {
                #region set from
                if (from != null)
                {
                    from.Text = ((Tag)from.Tag).place.X + " " + ((Tag)from.Tag).place.Y;
                    from.BackColor = Control.DefaultBackColor;
                    from.FlatStyle = FlatStyle.Standard;
                    from.UseVisualStyleBackColor = true;
                    //   from.ForeColor = Color.Gray;
                }

                b.BackColor = Color.Blue;
                from = b;
                ((Tag)from.Tag).distance = 0;
                from.Text = "from";
                #endregion
            }
            else
            {
                #region set to
                if (to != null)
                {
                    to.BackColor = Control.DefaultBackColor;
                    to.FlatStyle = FlatStyle.Standard;
                    to.UseVisualStyleBackColor = true;

                    to.Text = ((Tag)to.Tag).place.X + " " + ((Tag)to.Tag).place.Y;
                }
                b.BackColor = Color.Yellow;
                to = b;
                to.Text = "to";
                ((Tag)to.Tag).distance = double.MaxValue;
                #endregion
            }
        }
        #endregion
        #region create obsacles
        private void create_obstacles(params Point[] arrP)
        {
            foreach (Point p in arrP)
            {
                b[p.X, p.Y].BackColor = Color.Red;
            }

            int n = 200;
            Random rnd = new Random();

            for (int i = 0; i < n; i++)
            {
                b[rnd.Next(b.GetLength(0)), rnd.Next(b.GetLength(1))].BackColor = Color.Red;
            }
        }


        #endregion
        #region click to calculate
        private void button1_Click_1(object sender, EventArgs e)
        {
            //  Thread t = new Thread(new ThreadStart(show_path));
            //     calc_distances_with_rings();
            show_path();
            //  t.Start();
        }
        #endregion
        #region Create rings of distance from Location to Target.
        private void Create_rings_of_distance()
        {


            #region clear ring 1 and 2
            ring1.Clear();
            ring2.Clear();
            #endregion
            #region prepare all buttons to function as ring members
            foreach (Button btn in b)
            {
                Tag tag = (Tag)btn.Tag;
                tag.wasChecked = false;
                btn.Text = tag.place.X.ToString() + " " + tag.place.Y.ToString();
                if (btn.BackColor != Color.Red && btn != from && btn != to)
                {
                    tag.distance = double.MaxValue;
                    btn.BackColor = Control.DefaultBackColor;
                    btn.FlatStyle = FlatStyle.Standard;
                    btn.UseVisualStyleBackColor = true;
                }
            }
            #endregion
            #region add "from" to ring1 and prepare to loop and create ring2
            ring1.Add((Tag)from.Tag);
            bool targetFound = false;
            int i = 0;
            #endregion
            #region loop to create ring2
            while (!targetFound)
            {
                foreach (Tag t in ring1)
                {
                    foreach (Point kvn in kivuns)
                    {
                        #region Advance in direction and create ring member.
                        #region Init new X and Y
                        int nX = t.place.X + kvn.X;
                        int nY = t.place.Y + kvn.Y;
                        #endregion

                        if (is_in_bounds(nX, nY, b))
                        {
                            if (!is_obstacle(b[nX, nY]) &&
                                is_not_through_alachson(
                                         kvn.X, kvn.Y, t.place.X, t.place.Y))
                            {
                                #region If meet target then stop creating rings but finish this one.
                                if (b[nX, nY] == to)
                                    targetFound = true;
                                #endregion

                                double distance = t.distance +
                                                    get_distance(nX, nY, t.place.X, t.place.Y);
                                /**/
                                Tag tag = (Tag)b[nX, nY].Tag;
                                if (distance < tag.distance)
                                {
                                    #region Update minimum distance.
                                    tag.distance = distance;
                                    bool already_in_ring = tag.wasChecked;
                                    if (!already_in_ring)
                                    {
                                        ring2.Add(tag);
                                        tag.wasChecked = true;
                                    }
                                    #endregion
                                }
                            }
                        }
                        #endregion
                    }
                }
                #region set ring 2 in ring 1 and clear ring 2 for next loop
                ring1.Clear();
                ring1.AddRange(ring2);
                ring2.Clear();
                #endregion

                #region Color the ring.
                foreach (Tag t in ring1)
                {
                    b[t.place.X, t.place.Y].Text = t.distance.ToString();
                    b[t.place.X, t.place.Y].BackColor = colors[i % colors.Length];
                    if (doevents)
                    {
                        Application.DoEvents();
                        Thread.Sleep(10);
                    }

                } /* */
                i++;
                #endregion
            }
            #endregion


        }
        #endregion
        #region sketch path from location to target:
        private void show_path()
        {
            long ringsMSec, findMSec;

            /**/
            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            Create_rings_of_distance();
            sw.Stop();
            ringsMSec = sw.ElapsedMilliseconds;
            sw.Reset();
            sw.Start();

            #region free each caftor from achavat hatabaat and go to
            foreach (Button btn in b)
            {
                ((Tag)btn.Tag).wasChecked = false;
            }
            Button currentButton = to;
            #endregion
            while (currentButton != from)
            {
                Tag t = ((Tag)currentButton.Tag);
                currentButton.BackColor = Color.Black;
                Button nextButton = null;
                foreach (Point k in kivuns)
                {
                    bool in_bounds = is_in_bounds(t.place.X + k.X, t.place.Y + k.Y, b);
                    if (in_bounds)
                    {
                        bool next_null = nextButton == null;
                        bool closer = false;

                        if (!next_null)
                        {
                            Tag nT = ((Tag)nextButton.Tag);

                            closer = ((Tag)b[t.place.X + k.X, t.place.Y + k.Y].Tag).distance
                                                  < nT.distance;
                        }

                        bool is_better = next_null || closer;
                        is_better &= is_not_through_alachson(k.X, k.Y, t.place.X, t.place.Y);

                        if (is_better)
                        {
                            bool wasnt_checked =
                                !((Tag)b[t.place.X + k.X, t.place.Y + k.Y].Tag).wasChecked;
                            if (wasnt_checked)
                            {
                                nextButton = b[t.place.X + k.X, t.place.Y + k.Y];
                                ((Tag)nextButton.Tag).wasChecked = true;
                            }
                        }

                    }
                }
                nextButton.BackColor = Color.Black;
                currentButton = nextButton;
                if (doevents)
                {
                    Application.DoEvents();
                    Thread.Sleep(10);
                }
                /**/
            }
            sw.Stop();
            findMSec = sw.ElapsedMilliseconds;
            MessageBox.Show("find: " + findMSec.ToString() + "\n" +
                                 "rings: " + ringsMSec.ToString());
            /**/

            if (doevents)
            { Application.DoEvents(); }
        }
        #endregion
        //service:
        #region  3 funcs: calc_shoshana    is_obstacle?     get_distance
        private bool is_obstacle(Button b)
        {
            return b.BackColor == Color.Red;
        }

        private double get_distance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        private void create_shoshana()
        {
            kivuns = new Point[8];
            int j = 0;
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (x != 0 || y != 0)
                    {
                        kivuns[j++] = new Point(x, y);
                    }
                }
            }
        }
        #endregion
        #region 2 funcs: is_in_bounds  ,is_not_through_alachson
        private bool is_in_bounds<T>(int x, int y, T[,] arr)
        {
            return x >= 0 && x < arr.GetLength(0) && y >= 0 && y < arr.GetLength(1);
        }
        private bool is_not_through_alachson(int kvnX, int kvnY, int tX, int tY)
        {
            int nX = tX + kvnX;
            int nY = tY + kvnY;

            bool alashson = kvnX != 0 || kvnY != 0;
            bool may_move = true;
            bool may_move_alashson = true;
            if (is_in_bounds(nX, tY, b) &&
                is_in_bounds(tX, nY, b))
                may_move_alashson = !(is_obstacle(b[nX, tY])
                                         && is_obstacle(b[tX, nY]));
            if (alashson)
                may_move = may_move_alashson;

            return may_move;
        }
        #endregion
    }
}