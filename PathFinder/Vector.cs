using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PathFinder
{
    public class Vector
    {
        /// <summary>
        /// 正向水平单位向量
        /// </summary>
        public static Vector HorizontalVector { get; } = new Vector(1, 0);
        /// <summary>
        /// 反向水平单位向量
        /// </summary>
        public static Vector ReverseHorizontalVector { get; } = new Vector(-1, 0);
        /// <summary>
        /// 正向垂直单位向量
        /// </summary>
        public static Vector VerticalVector { get; } = new Vector(0, 1);
        /// <summary>
        /// 反向垂直单位向量
        /// </summary>   
        public static Vector ReverseVerticalVector { get; } = new Vector(0, -1);

        public readonly float x, y;
        public readonly float Length;
        public readonly float absX, absY;
        public Vector(float x, float y)
        {
            this.x = x;
            this.y = y;
            absX = Math.Abs(x);
            absY = Math.Abs(y);
            Length = (float) Math.Pow((Math.Pow(x, 2) + Math.Pow(y, 2)), 0.5);
        }
        public Vector(Pos a, Pos b):this(a.x - b.x, a.y - b.y) { }
                
        public static double Cos(Vector v1, Vector v2)
        {
            return (v1.x * v2.x + v1.y * v2.y) / Math.Pow((v1.x * v1.x + v1.y * v1.y) * (v2.x * v2.x + v2.y * v2.y), 0.5);
        } 

        public static double Sin(Vector v1, Vector v2)
        {
            var cos = Cos(v1, v2);
            return Math.Pow(1 - cos * cos, 0.5);
        }
    }
}
