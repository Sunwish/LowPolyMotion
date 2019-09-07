using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private int boardWidth;
        private int boardHeight;
        private System.Timers.Timer timer = new System.Timers.Timer();
        private LowPolyMotion.GridConfig grid;
        private LowPolyMotion.Rect[,] rects;
        private LowPolyMotion.MotionPoint[,] points;
        private LowPolyMotion.MotionPoint light;
        private Random rand = new Random();
        private static Mutex mutex = null;

        public Form1()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true); // 开启双缓冲

            boardWidth = this.Size.Width - 16;
            boardHeight = this.Size.Height - 46;

            // LowPolyMotion 全局初始化
            grid = new LowPolyMotion.GridConfig();
            InitializeLowPolyConfig(grid, boardWidth, boardHeight, int.Parse(textBox1.Text));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer.Elapsed += timer_event;
            this.SizeChanged += Form1_SizeChanged;
            this.Paint += Form1_Paint;
            this.textBox1.KeyPress += textBox1_KeyPress;
            mutex = new Mutex();
        }

        private void Form1_Paint(object sender, EventArgs e)
        {
            Draw(grid, rects, points, checkBox1.Checked);
        }

        private void InitializeLowPolyConfig(LowPolyMotion.GridConfig gridConfig, int boardWidth, int boardHeight, int pointCount)
        {
            if (pointCount <= 1)
                pointCount = 2;

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
            light = new LowPolyMotion.MotionPoint(rand.Next(LowPolyMotion.rangeN.min, LowPolyMotion.rangeN.max), 0, new LowPolyMotion.xyz(boardWidth / 2, boardHeight / 2, rand.Next(LowPolyMotion.rangeZ.min, LowPolyMotion.rangeZ.max)), NewEndXYZ(boardWidth, boardHeight, LowPolyMotion.rangeZ.light_min, LowPolyMotion.rangeZ.light_max));

            // Color Rendering
            ColorRendering(gridConfig, ref rects, points);
        }

        private void InitializePoints(LowPolyMotion.GridConfig gridConfig, ref LowPolyMotion.MotionPoint[,] points)
        {
            LowPolyMotion.GridInfo gridInfo = gridConfig.getGridInfo();
            for (int i = 0; i < gridInfo.c_x; i++)
            {
                for (int j = 0; j < gridInfo.c_y; j++)
                {
                    points[i, j] = new LowPolyMotion.MotionPoint(rand.Next(LowPolyMotion.rangeN.min, LowPolyMotion.rangeN.max), 0, NewEndXYZ(gridConfig, i, j), NewEndXYZ(gridConfig, i, j));
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

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            timer.Stop();
            boardWidth = this.Size.Width - 16;
            boardHeight = this.Size.Height - 46;
            InitializeLowPolyConfig(grid, boardWidth, boardHeight, int.Parse(textBox1.Text));
            Draw(grid, rects, points, checkBox1.Checked);
            panel1.Show();
        }

        private LowPolyMotion.xyz NewEndXYZ(int maxX, int maxY, int minZ, int maxZ)
        {
            LowPolyMotion.xyz result = new LowPolyMotion.xyz(rand.Next(0, maxX + 1), rand.Next(0, maxY + 1), rand.Next(0, maxZ + 1));
            return result;
        }

        private void timer_event(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 上锁 (c# System.Timers.Timer 是多线程执行的, 若不上锁则一旦绘制时间超过了时钟周期将出现写冲突)
            mutex.WaitOne();

            LowPolyMotion.GridInfo gridInfo = grid.getGridInfo();

            // 更新点坐标
            for (int i = 0; i < gridInfo.c_x; i++)
            {
                for (int j = 0; j < gridInfo.c_y; j++)
                {
                    if(points[i, j].motionData.NextStep())
                    {
                        points[i, j].motionData.begin_xyz = points[i, j].motionData.target_xyz;
                        points[i, j].motionData.target_xyz = NewEndXYZ(grid, i, j);
                        points[i, j].motionData.i = 0;
                        points[i, j].motionData.n = rand.Next(LowPolyMotion.rangeN.min, LowPolyMotion.rangeN.max);
                    }
                }
            }

            // 更新光源坐标
            if(light.motionData.NextStep())
            {
                light.motionData.begin_xyz = light.motionData.target_xyz;
                light.motionData.target_xyz = NewEndXYZ(boardWidth, boardHeight, LowPolyMotion.rangeZ.light_min, LowPolyMotion.rangeZ.light_max);
                light.motionData.i = 0;
                light.motionData.n = rand.Next(LowPolyMotion.rangeN.min, LowPolyMotion.rangeN.max);
            }

            // 更新重心坐标
            CalOutCenter(grid, ref rects, points);

            // 重新渲染颜色
            ColorRendering(grid, ref rects, points);

            Draw(grid, rects, points, checkBox1.Checked);

            // 解锁
            mutex.ReleaseMutex();
        }

        private void Draw(LowPolyMotion.GridConfig gridConfig, LowPolyMotion.Rect[,] rects, LowPolyMotion.MotionPoint[,] points, Boolean showExtrInf = false)
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
            if(showExtrInf)
            {
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
                        if (rects[i, j].T_Diagonal == 0) // 0.\  1./
                            g.DrawLine(myPen, ro_p[0].motionData.current_xyz.x, ro_p[0].motionData.current_xyz.y, ro_p[3].motionData.current_xyz.x, ro_p[3].motionData.current_xyz.y);
                        else
                            g.DrawLine(myPen, ro_p[1].motionData.current_xyz.x, ro_p[1].motionData.current_xyz.y, ro_p[2].motionData.current_xyz.x, ro_p[2].motionData.current_xyz.y);
                    }
                }
            }

            // 颜色渲染
            SolidBrush myBrush = new SolidBrush(Color.BlueViolet);
            Point[] myPoints = new Point[3];
            for (int i = 0; i < gridInfo.c_x - 1; i++)
            {
                for (int j = 0; j < gridInfo.c_y - 1; j++)
                {
                    if(rects[i,j].T_Diagonal == 0) // 0.\ 1./
                    {
                        // 左边面
                        myPoints[0].X = (int)points[i, j].motionData.current_xyz.x; myPoints[0].Y = (int)points[i, j].motionData.current_xyz.y;
                        myPoints[1].X = (int)points[i, j + 1].motionData.current_xyz.x; myPoints[1].Y = (int)points[i, j + 1].motionData.current_xyz.y;
                        myPoints[2].X = (int)points[i + 1, j + 1].motionData.current_xyz.x; myPoints[2].Y = (int)points[i + 1, j + 1].motionData.current_xyz.y;
                        myBrush.Color = Color.FromArgb(rects[i, j].color[0].R, rects[i, j].color[0].G, rects[i, j].color[0].B);
                        g.FillPolygon(myBrush, myPoints);
                        // 右边面
                        myPoints[1].X = (int)points[i + 1, j].motionData.current_xyz.x; myPoints[1].Y = (int)points[i + 1, j].motionData.current_xyz.y;
                        myBrush.Color = Color.FromArgb(rects[i, j].color[1].R, rects[i, j].color[1].G, rects[i, j].color[1].B);
                        g.FillPolygon(myBrush, myPoints);
                    }
                    else
                    {
                        // 左边面
                        myPoints[0].X = (int)points[i, j].motionData.current_xyz.x; myPoints[0].Y = (int)points[i, j].motionData.current_xyz.y;
                        myPoints[1].X = (int)points[i + 1, j].motionData.current_xyz.x; myPoints[1].Y = (int)points[i + 1, j].motionData.current_xyz.y;
                        myPoints[2].X = (int)points[i, j + 1].motionData.current_xyz.x; myPoints[2].Y = (int)points[i, j + 1].motionData.current_xyz.y;
                        myBrush.Color = Color.FromArgb(rects[i, j].color[0].R, rects[i, j].color[0].G, rects[i, j].color[0].B);
                        g.FillPolygon(myBrush, myPoints);
                        // 右边面
                        myPoints[0].X = (int)points[i + 1, j + 1].motionData.current_xyz.x; myPoints[0].Y = (int)points[i + 1, j + 1].motionData.current_xyz.y;
                        myBrush.Color = Color.FromArgb(rects[i, j].color[1].R, rects[i, j].color[1].G, rects[i, j].color[1].B);
                        g.FillPolygon(myBrush, myPoints);
                    }
                }
            }

            if (showExtrInf)
            {
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

                // 画点光源
                myPen.Color = Color.Red;
                g.DrawRectangle(myPen, light.motionData.current_xyz.x - 2, light.motionData.current_xyz.y - 2, 4, 4);
            }

            // 将内存画布绘制到屏幕上
            Graphics myGraphics = this.CreateGraphics();
            myGraphics.DrawImage(bitmap, 0, 0); 

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

        private void ColorRendering(LowPolyMotion.GridConfig gridConfig, ref LowPolyMotion.Rect[,] rects, LowPolyMotion.MotionPoint[,] points)
        {
            LowPolyMotion.GridInfo gridInfo = gridConfig.getGridInfo();
            LowPolyMotion.RGB[] paintRGB = new LowPolyMotion.RGB[2];
            LowPolyMotion.Vector[] v1 = new LowPolyMotion.Vector[2]; // 面的法向量
            LowPolyMotion.Vector[] v2 = new LowPolyMotion.Vector[2]; // 重心到光源的向量
            float[] cosin = new float[2]; // v1与v2夹角的余弦值

            for (int i = 0; i < gridInfo.c_x - 1; i++)
            {
                for (int j = 0; j < gridInfo.c_y - 1; j++)
                {
                    // 计算 v1
                    if(rects[i,j].T_Diagonal == 0) // 0.\ 1./
                    {
                        v1[0] = LowPolyMotion.Calculate_3D.Cal_3DNormalVector(points[i, j].motionData.current_xyz, points[i, j + 1].motionData.current_xyz, points[i + 1, j + 1].motionData.current_xyz);
                        v1[1] = LowPolyMotion.Calculate_3D.Cal_3DNormalVector(points[i, j].motionData.current_xyz, points[i + 1, j].motionData.current_xyz, points[i + 1, j + 1].motionData.current_xyz);
                    }
                    else
                    {
                        v1[0] = LowPolyMotion.Calculate_3D.Cal_3DNormalVector(points[i, j].motionData.current_xyz, points[i + 1, j].motionData.current_xyz, points[i, j + 1].motionData.current_xyz);
                        v1[1] = LowPolyMotion.Calculate_3D.Cal_3DNormalVector(points[i + 1, j + 1].motionData.current_xyz, points[i + 1, j].motionData.current_xyz, points[i, j + 1].motionData.current_xyz);
                    }
                    v1[0] = LowPolyMotion.Calculate_3D.Turn2EffectiveNV(v1[0]);
                    v1[1] = LowPolyMotion.Calculate_3D.Turn2EffectiveNV(v1[1]);

                    // 计算 v2
                    v2[0] = LowPolyMotion.Calculate_3D.Cal_3DGetVector(rects[i, j].outcentreXYZ[0], light.motionData.current_xyz);
                    v2[1] = LowPolyMotion.Calculate_3D.Cal_3DGetVector(rects[i, j].outcentreXYZ[1], light.motionData.current_xyz);

                    // 计算 cosine
                    cosin[0] = LowPolyMotion.Calculate_3D.Cal_3Dcos(v1[0], v2[0]);
                    cosin[1] = LowPolyMotion.Calculate_3D.Cal_3Dcos(v1[1], v2[1]);

                    // 计算着色
                    paintRGB[0] = LowPolyMotion.PaintRGB.RGB_Mid;
                    paintRGB[1] = LowPolyMotion.PaintRGB.RGB_Mid;
                    /// 左边面
                    if (cosin[0] > 0) // 向光
                        paintRGB[0] += (LowPolyMotion.PaintRGB.RGB_Light - LowPolyMotion.PaintRGB.RGB_Mid) * Math.Abs(cosin[0]);
                    else if (cosin[0] < 0) // 背光
                        paintRGB[0] -= (LowPolyMotion.PaintRGB.RGB_Mid - LowPolyMotion.PaintRGB.RGB_Dark) * Math.Abs(cosin[0]);
                    /// 右边面
                    if (cosin[1] > 0) // 向光
                        paintRGB[1] += (LowPolyMotion.PaintRGB.RGB_Light - LowPolyMotion.PaintRGB.RGB_Mid) * Math.Abs(cosin[1]);
                    else if (cosin[1] < 0) // 背光
                        paintRGB[1] -= (LowPolyMotion.PaintRGB.RGB_Mid - LowPolyMotion.PaintRGB.RGB_Dark) * Math.Abs(cosin[1]);

                    // rect上色
                    rects[i, j].color[0] = paintRGB[0];
                    rects[i, j].color[1] = paintRGB[1];
                    
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer.Interval = 10;
            timer.Start();
            panel1.Hide();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
                return;
            grid = new LowPolyMotion.GridConfig();
            InitializeLowPolyConfig(grid, boardWidth, boardHeight, int.Parse(textBox1.Text));
            Draw(grid, rects, points, checkBox1.Checked);
        }

        private void textBox1_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (!((e.KeyChar >= 48 && e.KeyChar <= 57) || e.KeyChar == '.' || e.KeyChar == 8))
            {
                e.Handled = true;
            }
        }

    }
}
