using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PathFinder
{
    [Serializable]
    public class Node
    {
        
        public Pos Pos { private set; get; }
        public Node Father { private set; get; } = null;
        public float G { private set; get; } = 0;        
        public float H { private set; get; } = 0;
        public float F { private set; get; } = 0;

        public bool IsClosed { private set; get; } = false;
        public bool IsCheck { private set; get; } = false;
        public bool IsBlock { private set; get; } = false;

        /// <summary>
        /// 是否可抵达
        /// </summary>
        public bool IsMoveable { get { return NearToBlockCount == 0; } }     
           
        /// <summary>
        /// 机器人半径范围内的障碍点计数
        /// </summary>
        public int NearToBlockCount { private set; get; } = 0;

        /// <summary>
        /// 离机器人半径范围外的周边障碍点的距离，最近距离为相切
        /// </summary>
        private List<float> CloseToBlockDist { set; get; } = new List<float>();

        /// <summary>
        /// 离最近障碍点的距离
        /// </summary>
        private float ClosetDist { get { return CloseToBlockDist.Min(); } }

        /// <summary>
        /// 惩罚百分比
        /// </summary>
        private float Punish { get { return (float) -0.5 * (ClosetDist / RobotObj.SafeDist) + 1; } }

        /// <summary>
        /// 是否有惩罚
        /// </summary>
        public bool IsPunish { get { return CloseToBlockDist.Count > 0; } }

        public Node(Pos p)
        {
            Pos = p;
        }

        public Node(int x, int y) : this(new Pos(x, y)) { }
        
        /// <summary>
        /// 重置节点
        /// </summary>
        public void Reset()
        {
            G = 0;
            F = 0;
            H = 0;
            Father = null;
            IsClosed = false;
            IsCheck = false;
            
        }

        /// <summary>
        /// 设置父节点
        /// </summary>
        /// <param name="father"></param>
        /// <param name="endPos"></param>
        public void SetFather(Node father, Pos endPos)
        {
            if (father == null)
                return;

            Father = father;
            G = CalcG(father);
            CalcH(endPos);
            CalcF();
        }

        /// <summary>
        /// 检查从点n到本点的g值是否比从原父节点到本点的g值小
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool IsNewFather(Node n)
        {
            if (n == null)
                return false;

            if (CalcG(n) < G)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 计算从起点到本点的耗费值
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private float CalcG(Node n)
        {
            var v = Pos - n.Pos;
            var sameCol = v.absX == 0;
            var sameRow = v.absY == 0;
            var baseG = 0;
            if (!sameCol && !sameRow)
                baseG = 14;
            else if (sameRow || sameCol)
                baseG = 10;
            return baseG + n.G;
        }

        /// <summary>
        /// 计算本点到终点的估计值
        /// </summary>
        /// <param name="endPos"></param>
        private void CalcH(Pos endPos)
        {
            var v = Pos - endPos;
            //曼哈顿
            H = (v.absX + v.absY) * 10;

            ////对角线距离
            //var absX = Math.Abs(p.x);
            //var absY = Math.Abs(p.y);
            //H = 10 * (absX + absY) + (14 - 10) * Math.Min(absX, absY);            
        }

        /// <summary>
        /// 计算点的评分，如果靠近墙则加上惩罚值
        /// </summary>
        private void CalcF()
        {
            F = G + H;
            if (IsPunish)
                F *= (1 + Punish);
        }

        /// <summary>
        /// 设置本点已被查询过
        /// </summary>
        public void Check()
        {
            IsCheck = true;            
        }
        
        /// <summary>
        /// 关闭本点避免被获取
        /// </summary>
        public void Close()
        {
            IsClosed = true;
        }

        /// <summary>
        /// 设置为障碍点
        /// </summary>
        public void Block()
        {
            IsBlock = true;
        }

        /// <summary>
        /// 取消设置为障碍点
        /// </summary>
        public void UnBlock()
        {
            IsBlock = false;
            Reset();
        }

        /// <summary>
        /// 添加附近的障碍点
        /// </summary>
        /// <param name="p"></param>
        public void AddNearBlock(Pos p)
        {
            var dist = (p - Pos).Length;
            var d = dist - RobotObj.Size;
            if (d < 0)
            {
                NearToBlockCount++;
                return;
            }
            if (d == 0 || d <= RobotObj.SafeDist)
            {
                CloseToBlockDist.Add(d);
                return;
            }
        }

        /// <summary>
        /// 删除附近的障碍点
        /// </summary>
        /// <param name="p"></param>
        public void RemoveNearBlock(Pos p)
        {
            var dist = (p - Pos).Length;
            var d = dist - RobotObj.Size;
            if (d < 0 && NearToBlockCount > 0)
            {
                NearToBlockCount--;
                return;
            }

            
            if (d == 0 || d <= RobotObj.SafeDist)
            {
                CloseToBlockDist.Remove(d);
                return;
            }
        }
        
    }
}
