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
        private int i = 10;
        public Form1()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true); // 开启双缓冲
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer.Elapsed += timer_event;
        }

        private void timer_event(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 在内存中建立虚拟画布
            Bitmap bitmap = new Bitmap(this.Width, this.Height);
            // 获取内存画布的 Graphics 引用
            Graphics g = Graphics.FromImage(bitmap);
            // 创建画笔
            Pen myPen = new Pen(Color.Black, 2);
            // 绘图
            g.Clear(Color.White);
            g.DrawLine(myPen, 100, 200 - 50 * (float)Math.Sin(Math.PI * i / 180), 300, 200 + 50 * (float)Math.Sin(Math.PI * i / 180));
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
            i += 10;

            bitmap.Dispose();
            g.Dispose();
            myPen.Dispose();
            myGraphics.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            timer.Interval = 5;
            timer.Start();
        }
    }
}
