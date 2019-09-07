using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private System.Timers.Timer timer = new System.Timers.Timer();
        private LowPolyMotion.GridConfig grid = new LowPolyMotion.GridConfig();
        private LowPolyMotion.Rect[,] rects;
        private int i = 10;
        private int c_x, c_y, c_w, c_h; // x方向网格数, y方向网格数, x方向网格宽度, y方向网格高度

        public Form1()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true); // 开启双缓冲
            InitializeLowPolyConfig();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer.Elapsed += timer_event;
            this.SizeChanged += Form1_SizeChanged;
        }

        private void InitializeLowPolyConfig()
        {
            // Initialize Grid
            grid.initGridConfig(this.Size.Width - 16, this.Size.Height - 46, 10);

            // Initialize Rects
            c_x = grid.getCrossCellCount();
            c_y = grid.getDirectionCellCount();
            c_w = grid.getCrossCellWidth();
            c_h = grid.getDirectionCellHeight();
            rects = new LowPolyMotion.Rect[c_x, c_y];
            for (int i = 0; i < c_x; i++)
            {
                for (int j = 0; j < c_y; j++)
                {
                    rects[i, j] = new LowPolyMotion.Rect();
                    rects[i, j].xyz = new LowPolyMotion.xyz(c_w * i, c_h * j, 0);
                    rects[i, j].w = c_w;
                    rects[i, j].h = c_h;
                    rects[i, j].T_Diagonal = new Random().Next(0, 2); // [0,2)
                }
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            InitializeLowPolyConfig();
            Draw();
        }

        private void timer_event(object sender, System.Timers.ElapsedEventArgs e)
        {
            Draw();
        }

        private void Draw()
        {
            // 在内存中建立虚拟画布
            Bitmap bitmap = new Bitmap(this.Size.Width, this.Size.Height);

            // 获取内存画布的 Graphics 引用
            Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);

            // 创建画笔
            Pen myPen = new Pen(Color.Black, 2);

            // 绘图
            for (int i = 0; i < c_x + 1; i++)
            {
                g.DrawLine(myPen, c_w * i, 0, c_w * i, c_h * c_y);
            }
            for (int j = 0; j < c_y + 1; j++)
            {
                g.DrawLine(myPen, 0, c_h * j, c_w * c_x, c_h * j);
            }

            // 将内存画布绘制到屏幕上
            Graphics myGraphics = this.CreateGraphics();
            myGraphics.DrawImage(bitmap, 0, 0);

            /*
            Brush myBrush = new SolidBrush(Color.BlueViolet);
            Point[] myPoints = new Point[3];
            myPoints[0].X = moveLength; myPoints[0].Y = moveLength;
            myPoints[1].X = 100 + moveLength; myPoints[1].Y = 10 + moveLength;
            myPoints[2].X = 100 + moveLength; myPoints[2].Y = 100 + moveLength;
            g.FillPolygon(myBrush, myPoints);
            */
            // i += 10;


            // 释放资源
            bitmap.Dispose();
            g.Dispose();
            myPen.Dispose();
            myGraphics.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*
            timer.Interval = 5;
            timer.Start();
            */
            InitializeLowPolyConfig();
            Draw();

        }
    }
}
