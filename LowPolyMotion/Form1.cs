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
        private LowPolyMotion.GridConfig grid;
        private LowPolyMotion.Rect[,] rects;
        private LowPolyMotion.MotionPoint[,] points;
        private LowPolyMotion.MotionPoint light;
        private int i = 10;
        private Random rand = new Random();

        public Form1()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true); // 开启双缓冲

            // LowPolyMotion 全局初始化
            grid = new LowPolyMotion.GridConfig();
            InitializeLowPolyConfig(grid, this.Size.Width - 16, this.Size.Height - 46, 10);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer.Elapsed += timer_event;
            this.SizeChanged += Form1_SizeChanged;
        }

        private void InitializeLowPolyConfig(LowPolyMotion.GridConfig gridConfig, int boardWidth, int boardHeight, int pointCount)
        {
            // Initialize Grid
            gridConfig.initGridConfig(boardWidth, boardHeight, pointCount);

            // Initialize Rects
            LowPolyMotion.GridInfo gridInfo = gridConfig.getGridInfo();
            gridInfo.c_x = grid.getCrossCellCount();
            gridInfo.c_y = grid.getDirectionCellCount();
            gridInfo.c_w = grid.getCrossCellWidth();
            gridInfo.c_h = grid.getDirectionCellHeight();
            rects = new LowPolyMotion.Rect[gridInfo.c_x, gridInfo.c_y];
            for (int i = 0; i < gridInfo.c_x; i++)
            {
                for (int j = 0; j < gridInfo.c_y; j++)
                {
                    rects[i, j] = new LowPolyMotion.Rect();
                    rects[i, j].xyz = new LowPolyMotion.xyz(gridInfo.c_w * i, gridInfo.c_h * j, 0);
                    rects[i, j].w = gridInfo.c_w;
                    rects[i, j].h = gridInfo.c_h;
                    rects[i, j].T_Diagonal = rand.Next(0, 2); // [0,2)
                }
            }

            // Initialize Points
            points = new LowPolyMotion.MotionPoint[gridInfo.c_x, gridInfo.c_y];
            InitializePoints(gridConfig, ref points);
            CalOutCenter(gridConfig, ref rects, points);

            // Initialize Light
            light = new LowPolyMotion.MotionPoint(500, 0, new LowPolyMotion.xyz(boardWidth / 2, boardHeight / 2, rand.Next(LowPolyMotion.rangeZ.min, LowPolyMotion.rangeZ.max)), NewEndXYZ(boardWidth, boardHeight, LowPolyMotion.rangeZ.max));
        }

        private void InitializePoints(LowPolyMotion.GridConfig gridConfig, ref LowPolyMotion.MotionPoint[,] points)
        {
            LowPolyMotion.GridInfo gridInfo = gridConfig.getGridInfo();
            for (int i = 0; i < gridInfo.c_x; i++)
            {
                for (int j = 0; j < gridInfo.c_y; j++)
                {
                    points[i, j] = new LowPolyMotion.MotionPoint(500, 0, NewEndXYZ(gridConfig, i, j), NewEndXYZ(gridConfig, i, j));
                }
            }
        }

        private LowPolyMotion.xyz NewEndXYZ(LowPolyMotion.GridConfig gridConfig, int rect_i, int rect_j)
        {
            LowPolyMotion.xyz result = new LowPolyMotion.xyz(0, 0, rand.Next(LowPolyMotion.rangeZ.min, LowPolyMotion.rangeZ.max));
            int edgeIndex = rand.Next(0, 4); // 0~3: 上下左右边
            double bias = rand.NextDouble();
            switch (edgeIndex)
            {
                case 0:
                    result.x = (int)(gridConfig.getCrossCellWidth() * rect_i + gridConfig.getCrossCellWidth() * bias);
                    result.y = gridConfig.getDirectionCellHeight() * rect_j;
                    break;
                case 1:
                    result.x = (int)(gridConfig.getCrossCellWidth() * rect_i + gridConfig.getCrossCellWidth() * bias);
                    result.y = gridConfig.getDirectionCellHeight() * (rect_j + 1);
                    break;
                case 2:
                    result.x = gridConfig.getCrossCellWidth() * rect_i;
                    result.y = (int)(gridConfig.getDirectionCellHeight() * rect_j + gridConfig.getDirectionCellHeight() * bias);
                    break;
                default:
                    result.x = gridConfig.getCrossCellWidth() * (rect_i + 1);
                    result.y = (int)(gridConfig.getDirectionCellHeight() * rect_j + gridConfig.getDirectionCellHeight() * bias);
                    break;
            }
            return result;
        }

        private LowPolyMotion.xyz NewEndXYZ(int maxX, int maxY, int maxZ)
        {
            LowPolyMotion.xyz result = new LowPolyMotion.xyz(rand.Next(0, maxX + 1), rand.Next(0, maxY + 1), rand.Next(0, maxZ + 1));
            return result;
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            InitializeLowPolyConfig(grid, this.Size.Width - 16, this.Size.Height - 46, 10);
            Draw(grid, rects, points);
        }

        private void timer_event(object sender, System.Timers.ElapsedEventArgs e)
        {
            Draw(grid, rects, points);
        }

        private void Draw(LowPolyMotion.GridConfig gridConfig, LowPolyMotion.Rect[,] rects, LowPolyMotion.MotionPoint[,] points)
        {
            // 在内存中建立虚拟画布
            Bitmap bitmap = new Bitmap(this.Size.Width, this.Size.Height);

            // 获取内存画布的 Graphics 引用
            Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);

            // 获取网格基础信息
            LowPolyMotion.GridInfo gridInfo = gridConfig.getGridInfo();

            // 创建画笔
            Pen myPen = new Pen(Color.Black, 2);

            // 绘图
            /// 画网格线
            myPen.Color = Color.Tan;
            for (int i = 0; i < gridInfo.c_x + 1; i++)
            {
                g.DrawLine(myPen, gridInfo.c_w * i, 0, gridInfo.c_w * i, gridInfo.c_h * gridInfo.c_y);
            }
            for (int j = 0; j < gridInfo.c_y + 1; j++)
            {
                g.DrawLine(myPen, 0, gridInfo.c_h * j, gridInfo.c_w * gridInfo.c_x, gridInfo.c_h * j);
            }
            /// 画点
            myPen.Color = Color.Black;
            for (int i = 0; i < gridInfo.c_x; i++)
            {
                for (int j = 0; j < gridInfo.c_y; j++)
                {
                    g.DrawRectangle(myPen, points[i, j].motionData.current_xyz.x - 1, points[i, j].motionData.current_xyz.y - 1, 2, 2);
                }
            }
            /// 连线
            myPen.Color = Color.Black;
            LowPolyMotion.MotionPoint[] ro_p = new LowPolyMotion.MotionPoint[4];
            
            for (int i = 0; i < gridInfo.c_x - 1; i++)
            {
                for (int j = 0; j < gridInfo.c_y - 1; j++)
                {
                    // 被连线的网格顶点
                    ro_p[0] = points[i, j];
                    ro_p[1] = points[i + 1, j];
                    ro_p[2] = points[i, j + 1];
                    ro_p[3] = points[i + 1, j + 1];

                    // 四边形连线
                    g.DrawLine(myPen, ro_p[0].motionData.current_xyz.x, ro_p[0].motionData.current_xyz.y, ro_p[1].motionData.current_xyz.x, ro_p[1].motionData.current_xyz.y);
                    g.DrawLine(myPen, ro_p[0].motionData.current_xyz.x, ro_p[0].motionData.current_xyz.y, ro_p[2].motionData.current_xyz.x, ro_p[2].motionData.current_xyz.y);
                    g.DrawLine(myPen, ro_p[1].motionData.current_xyz.x, ro_p[1].motionData.current_xyz.y, ro_p[3].motionData.current_xyz.x, ro_p[3].motionData.current_xyz.y);
                    g.DrawLine(myPen, ro_p[2].motionData.current_xyz.x, ro_p[2].motionData.current_xyz.y, ro_p[3].motionData.current_xyz.x, ro_p[3].motionData.current_xyz.y);

                    // 对角线
                    if (rects[i,j].T_Diagonal == 0) // 0.\  1./
                        g.DrawLine(myPen, ro_p[0].motionData.current_xyz.x, ro_p[0].motionData.current_xyz.y, ro_p[3].motionData.current_xyz.x, ro_p[3].motionData.current_xyz.y);
                    else
                        g.DrawLine(myPen, ro_p[1].motionData.current_xyz.x, ro_p[1].motionData.current_xyz.y, ro_p[2].motionData.current_xyz.x, ro_p[2].motionData.current_xyz.y);
                }
            }

            // 画点光源
            myPen.Color = Color.Red;
            g.DrawRectangle(myPen, light.motionData.current_xyz.x - 2, light.motionData.current_xyz.y - 2, 4, 4);

            // 画重心
            myPen.Color = Color.DarkBlue;
            for (int i = 0; i < gridInfo.c_x - 1; i++)
            {
                for (int j = 0; j < gridInfo.c_y - 1; j++)
                {
                    g.DrawRectangle(myPen, rects[i, j].outcentreXYZ[0].x - 2, rects[i, j].outcentreXYZ[0].y - 2, 4, 4);
                    g.DrawRectangle(myPen, rects[i, j].outcentreXYZ[1].x - 2, rects[i, j].outcentreXYZ[1].y - 2, 4, 4);
                }
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

            // 释放资源
            bitmap.Dispose();
            g.Dispose();
            myPen.Dispose();
            myGraphics.Dispose();
        }

        private void CalOutCenter(LowPolyMotion.GridConfig gridConfig, ref LowPolyMotion.Rect[,] rects, LowPolyMotion.MotionPoint[,] points) // 重心坐标计算
        {
            LowPolyMotion.GridInfo gridInfo = gridConfig.getGridInfo();
            for (int i = 0; i < gridInfo.c_x - 1; i++) 
            {
                for (int j = 0; j < gridInfo.c_y - 1; j++)
                {
                    if(rects[i,j].T_Diagonal == 0) // 0.\ 1./
                    {
                        rects[i, j].outcentreXYZ[0] = new LowPolyMotion.xyz(LowPolyMotion.Calculate_3D.Cal_3DGetFocus(points[i, j].motionData.current_xyz, points[i, j + 1].motionData.current_xyz, points[i + 1, j + 1].motionData.current_xyz));
                        rects[i, j].outcentreXYZ[1] = new LowPolyMotion.xyz(LowPolyMotion.Calculate_3D.Cal_3DGetFocus(points[i, j].motionData.current_xyz, points[i + 1, j].motionData.current_xyz, points[i + 1, j + 1].motionData.current_xyz));
                    }
                    else
                    {
                        rects[i, j].outcentreXYZ[0] = new LowPolyMotion.xyz(LowPolyMotion.Calculate_3D.Cal_3DGetFocus(points[i, j].motionData.current_xyz, points[i + 1, j].motionData.current_xyz, points[i, j + 1].motionData.current_xyz));
                        rects[i, j].outcentreXYZ[1] = new LowPolyMotion.xyz(LowPolyMotion.Calculate_3D.Cal_3DGetFocus(points[i + 1, j + 1].motionData.current_xyz, points[i + 1, j].motionData.current_xyz, points[i, j + 1].motionData.current_xyz));
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*
            timer.Interval = 5;
            timer.Start();
            */
            InitializeLowPolyConfig(grid, this.Size.Width - 16, this.Size.Height - 46, 10);

            Draw(grid, rects, points);

        }
    }
}
