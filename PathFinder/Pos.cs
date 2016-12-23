using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PathFinder
{
    [Serializable]
    public class Pos
    {
        public readonly int x, y;

        #region 四周点坐标
        public Pos Up { get { return Shift(0, -1); } }
        public Pos Down { get { return Shift(0, 1); } }
        public Pos Left { get { return Shift(-1, 0); } }
        public Pos Right { get { return Shift(1, 0); } }
        public Pos LeftUp { get { return Shift(-1, -1); } }
        public Pos LeftDown { get { return Shift(-1, 1); } }
        public Pos RightUp { get { return Shift(1, -1); } }
        public Pos RightDown { get { return Shift(1, 1); } }
        public List<Pos> AdjacentPos { get { return new List<Pos>() { Up,Down,Left,Right,LeftUp,LeftDown,RightUp,RightDown}; } }
        #endregion

        public Pos(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public Pos(double x, double y):this((int)Math.Round(x), (int)Math.Round(y)) { }
        
        public static Vector operator -(Pos a, Pos b)
        {
            return new Vector(a, b);
        }

        /// <summary>
        /// 点位移
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Pos Shift(int x, int y)
        {
            return new Pos(this.x + x, this.y + y);
        }
                
        public static bool operator ==(Pos a, Pos b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(Pos a, Pos b)
        {
            return !(a==b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Pos p = obj as Pos;
            if ((object)p == null)
                return false;

            return this == p;
        }

        public bool Equals(Pos p)
        {
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return this == p;
        }

        public override int GetHashCode()
        {
            return x ^ y;
        }
    }

    
}
