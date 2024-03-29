﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LowPolyMotion
{
    class rangeZ
    {
        public static int min = 0;
        public static int max = 50;
        public static int light_min = 100;
        public static int light_max = 120;
    }

    class rangeN
    {
        public static int min = 80;
        public static int max = 180;
    }

    class PaintRGB
    {
        public static LowPolyMotion.RGB RGB_Light = new LowPolyMotion.RGB(0, 255, 255);
        public static LowPolyMotion.RGB RGB_Dark = new LowPolyMotion.RGB(0, 50, 120);
        public static LowPolyMotion.RGB RGB_Mid = new LowPolyMotion.RGB((int)((RGB_Light.R + RGB_Dark.R) / 2), (int)((RGB_Light.G + RGB_Dark.G) / 2), (int)((RGB_Light.B + RGB_Dark.B) / 2));
    }

    class xyz
    {
        public float x;
        public float y;
        public float z;

        public xyz(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public xyz(xyz xyz)
        {
            this.x = xyz.x;
            this.y = xyz.y;
            this.z = xyz.z;
        }

        // 运算符重载
        public static xyz operator +(xyz xyz1, xyz xyz2)
        {
            xyz result = new xyz(xyz1.x + xyz2.x, xyz1.y + xyz2.y, xyz1.z + xyz2.z);
            return result;
        }

        public static xyz operator -(xyz xyz1, xyz xyz2)
        {
            xyz result = new xyz(xyz1.x - xyz2.x, xyz1.y - xyz2.y, xyz1.z - xyz2.z);
            return result;
        }

        public static xyz operator *(xyz xyz, float param)
        {
            xyz result = new xyz(xyz.x * param, xyz.y * param, xyz.z * param);
            return result;
        }
    }

    class Vector
    {
        public float x;
        public float y;
        public float z;

        public Vector() { }
        public Vector(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector(Vector vector)
        {
            this.x = vector.x;
            this.y = vector.y;
            this.z = vector.z;
        }

        // 运算符重载
        public static Vector operator +(Vector vector1, xyz vector2)
        {
            Vector result = new Vector(vector1.x + vector2.x, vector1.y + vector2.y, vector1.z + vector2.z);
            return result;
        }

        public static Vector operator -(Vector vector1, xyz vector2)
        {
            Vector result = new Vector(vector1.x - vector2.x, vector1.y - vector2.y, vector1.z - vector2.z);
            return result;
        }

        public static Vector operator *(Vector vector, float param)
        {
            Vector result = new Vector(vector.x * param, vector.y * param, vector.z * param);
            return result;
        }
    }

    class MotionPoint
    {
        public MotionData motionData;

        public MotionPoint(int n, int i, xyz begin_xyz, xyz target_xyz)
        {
            motionData = new MotionData(n, i, begin_xyz, target_xyz);
        }

        // 运算符重载
        public static MotionPoint operator +(MotionPoint point1, MotionPoint point2)
        {
            MotionPoint result = point1;
            result.motionData.current_xyz.x += point2.motionData.current_xyz.x;
            result.motionData.current_xyz.y += point2.motionData.current_xyz.y;
            return result;
        }

        public static MotionPoint operator -(MotionPoint point1, MotionPoint point2)
        {
            MotionPoint result = point1;
            result.motionData.current_xyz.x -= point2.motionData.current_xyz.x;
            result.motionData.current_xyz.y -= point2.motionData.current_xyz.y;
            return result;
        }
    }

    class RGB
    {
        public int R, G, B;
        public RGB(int R, int G, int B)
        {
            this.R = R;
            this.G = G;
            this.B = B;
        }

        public static LowPolyMotion.RGB operator +(LowPolyMotion.RGB rgb1, LowPolyMotion.RGB rgb2)
        {
            return new LowPolyMotion.RGB(rgb1.R + rgb2.R, rgb1.G + rgb2.G, rgb1.B + rgb2.B);
        }

        public static LowPolyMotion.RGB operator -(LowPolyMotion.RGB rgb1, LowPolyMotion.RGB rgb2)
        {
            return new LowPolyMotion.RGB(rgb1.R - rgb2.R, rgb1.G - rgb2.G, rgb1.B - rgb2.B);
        }

        public static LowPolyMotion.RGB operator *(LowPolyMotion.RGB rgb, float param)
        {
            return new LowPolyMotion.RGB((int)(rgb.R * param), (int)(rgb.G * param), (int)(rgb.B * param));
        }
    }

    class Rect
    {
        public xyz xyz = new xyz(0, 0, 0);
        public float w;
        public float h;
        public int T_Diagonal; // 对角线类型 (0.\ 1./)
        public Vector[] vector = new Vector[2]; // [0].左边面, [1].右边面
        public xyz[] outcentreXYZ = new xyz[2]; // 三角面外心坐标, 规则同 vector
        public RGB[] color = new RGB[2]; // 规则同 vector
    }

    class MotionData
    {
        public int n;
        public int i;
        public xyz begin_xyz;
        public xyz target_xyz;
        public xyz current_xyz;

        public MotionData(int n, int i, xyz begin_xyz, xyz target_xyz)
        {
            this.n = n;
            this.i = i;
            this.begin_xyz = begin_xyz;
            this.target_xyz = target_xyz;
            this.current_xyz = begin_xyz;
        }

        public Boolean NextStep()
        {
            this.i++;

            xyz deltaXYZ = new xyz(target_xyz - begin_xyz);
            float process = (float)i / n;

            this.current_xyz = this.begin_xyz + deltaXYZ * process;
            return this.i == this.n;
        }
    }

    class GridConfig
    {
        private int crossCellWidth;
        private int directionCellHeight;
        private int crossCellCount;
        private int directionCellCount;

        public GridConfig() { }
        public GridConfig(int crossCellWidth, int directionCellHeight, int crossCellCount, int directionCellCount)
        {
            this.crossCellWidth = crossCellWidth;
            this.directionCellHeight = directionCellHeight;
            this.crossCellCount = crossCellCount;
            this.directionCellCount = directionCellCount;
        }

        public void initGridConfig(int boardWidth, int boardHeight, int pointCount)
        {
            // 粗算均分数
            float _crossCellCount = (float)Math.Sqrt((float)boardWidth / pointCount);
            float _directionCellCount = (float)Math.Sqrt((float)boardHeight / pointCount);
            if(_crossCellCount * _directionCellCount > pointCount)
            {
                do
                {
                    _crossCellCount--;
                    _directionCellCount--;
                } while (_crossCellCount * _directionCellCount > pointCount);
                _crossCellCount++;
                _directionCellCount++;
            }
            else if(_crossCellCount * _directionCellCount < pointCount)
            {
                do
                {
                    _crossCellCount++;
                    _directionCellCount++;
                } while (_crossCellCount * _directionCellCount < pointCount);
            }

            // 求最小误差对应的均分数
            int[] r_p = new int[4];
            int[] temp = new int[5];
            r_p[0] = (int)_crossCellCount * (int)_directionCellCount;
            r_p[1] = (int)Math.Round(_crossCellCount) * (int)_directionCellCount;
            r_p[2] = (int)_crossCellCount * (int)Math.Round(_directionCellCount);
            r_p[3] = (int)Math.Round(_crossCellCount) * (int)Math.Round(_directionCellCount);
            temp[4] = 0;
            for (int i = 0; i < 4; i++)
            {
                temp[i] = Math.Abs(r_p[i] - pointCount);
                if (temp[i] <= temp[temp[4]]) temp[4] = i;
            }
            switch(temp[4])
            {
                case 0:
                    this.crossCellCount = (int)_crossCellCount;
                    this.directionCellCount = (int)_directionCellCount;
                    break;
                case 1:
                    this.crossCellCount = (int)Math.Round(_crossCellCount);
                    this.directionCellCount = (int)_directionCellCount;
                    break;
                case 2:
                    this.crossCellCount = (int)_crossCellCount;
                    this.directionCellCount = (int)Math.Round(_directionCellCount);
                    break;
                default:
                    this.crossCellCount = (int)Math.Round(_crossCellCount);
                    this.directionCellCount = (int)Math.Round(_directionCellCount);
                    break;
            }

            System.Diagnostics.Debug.WriteLine("点数变更:");
            System.Diagnostics.Debug.WriteLine(this.crossCellCount * this.directionCellCount - pointCount);

            System.Diagnostics.Debug.WriteLine("网格划分:");
            System.Diagnostics.Debug.WriteLine("x: " + this.crossCellCount.ToString() + ", y: " + this.directionCellCount.ToString());

            // 网格宽高
            this.crossCellWidth = boardWidth / this.crossCellCount;
            this.directionCellHeight = boardHeight / this.directionCellCount;
        }

        public int getCrossCellWidth()
        {
            return this.crossCellWidth;
        }

        public int getDirectionCellHeight()
        {
            return this.directionCellHeight;
        }

        public int getCrossCellCount()
        {
            return this.crossCellCount;
        }

        public int getDirectionCellCount()
        {
            return this.directionCellCount;
        }

        public GridInfo getGridInfo()
        {
            return new GridInfo(getCrossCellCount(), getDirectionCellCount(), getCrossCellWidth(),getDirectionCellHeight());
        }
    }

    class GridInfo
    {
        public int c_x;
        public int c_y;
        public int c_w;
        public int c_h;

        public GridInfo(int c_x, int c_y, int c_w, int c_h)
        {
            this.c_x = c_x;
            this.c_y = c_y;
            this.c_w = c_w;
            this.c_h = c_h;
        }

        public GridInfo(GridInfo gridInfo)
        {
            this.c_x = gridInfo.c_x;
            this.c_y = gridInfo.c_y;
            this.c_w = gridInfo.c_w;
            this.c_h = gridInfo.c_h;
        }
    }
}
