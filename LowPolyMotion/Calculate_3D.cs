using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowPolyMotion
{
    class Calculate_3D
    {
        public static LowPolyMotion.Vector Cal_3DGetVector(LowPolyMotion.xyz p1, LowPolyMotion.xyz p2) // 求空间向量
        {
            LowPolyMotion.xyz deltaXYZ = p2 - p1;
            LowPolyMotion.Vector v = new LowPolyMotion.Vector(deltaXYZ.x, deltaXYZ.y, deltaXYZ.z);
            return v;
        }

        public static float Cal_3DVLength(LowPolyMotion.Vector vector) // 求空间向量模长
        {
            return (float)Math.Sqrt(Math.Pow(vector.x, 2) + Math.Pow(vector.y, 2) + Math.Pow(vector.z, 2));
        }

        public static float Cal_3DdotProduct(LowPolyMotion.Vector vector1, LowPolyMotion.Vector vector2) // 求空间向量点乘
        {
            return Cal_3DVLength(vector1) * Cal_3DVLength(vector2) * Cal_3Dcos(vector1, vector2);
        }

        public static float Cal_3Dcos(LowPolyMotion.Vector vector1, LowPolyMotion.Vector vector2) // 求空间向量夹角余弦值
        {
            return (vector1.x * vector2.x + vector1.y * vector2.y + vector1.z * vector2.z) / (Cal_3DVLength(vector1) * Cal_3DVLength(vector2));
        }

        public static LowPolyMotion.xyz Cal_3DGetFocus(LowPolyMotion.xyz p1, LowPolyMotion.xyz p2, LowPolyMotion.xyz p3) // 求空间三角形重心坐标
        {
            LowPolyMotion.xyz oP = new LowPolyMotion.xyz(0, 0, 0);
            oP.x = (p1.x + p2.x + p3.x) / 3;
            oP.y = (p1.y + p2.y + p3.y) / 3;
            oP.z = (p1.z + p2.z + p3.z) / 3;
            return oP;
        }

        public static LowPolyMotion.Vector Cal_3DNormalVector(LowPolyMotion.xyz p1, LowPolyMotion.xyz p2, LowPolyMotion.xyz p3) // 计算空间面的法向量
        {
            LowPolyMotion.Vector NV = new LowPolyMotion.Vector(0, 0, 0);
            LowPolyMotion.Vector tempV1 = new LowPolyMotion.Vector(0, 0, 0);
            LowPolyMotion.Vector tempV2 = new LowPolyMotion.Vector(0, 0, 0);
            tempV1 = Cal_3DGetVector(p1, p2);
            tempV2 = Cal_3DGetVector(p1, p3);
            NV.x = tempV1.y * tempV2.z - tempV1.z * tempV2.y;
            NV.y = -(tempV1.x * tempV2.z - tempV1.z * tempV2.x);
            NV.z = tempV1.x * tempV2.y - tempV1.y * tempV2.x;
            return NV;
        }

        public static LowPolyMotion.Vector Turn2EffectiveNV(LowPolyMotion.Vector NV) // 转为有效法向量(指向屏幕外侧)
        {
            LowPolyMotion.Vector _NV = new LowPolyMotion.Vector(0, 0, 0);
            LowPolyMotion.Vector zV = new LowPolyMotion.Vector(0, 0, 1);
            _NV = NV;
            if(Cal_3Dcos(_NV,zV)<0)
            {
                _NV.x = -_NV.x;
                _NV.y = -_NV.y;
                _NV.z = -_NV.z;
            }
            return _NV;

        }

        /*
        public static LowPolyMotion.xyz Cal_3DoutCenter(LowPolyMotion.xyz p1, LowPolyMotion.xyz p2, LowPolyMotion.xyz p3) // 求空间三角形外心坐标
        {
            LowPolyMotion.xyz result;

            return result;
        }
        */
    }
}
