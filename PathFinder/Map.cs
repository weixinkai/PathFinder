using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PathFinder
{
    public struct MapBorder
    {
        public readonly int x, y;
        public MapBorder(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
    public class RobotObj
    {
        /// <summary>
        /// 机器人半径
        /// </summary>
        public static float Size { get; } = 12.0F;  
        
        /// <summary>
        /// 不贴墙的安全距地
        /// </summary>                      
        public static int SafeDist { get; } = 8;

        /// <summary>
        /// 障碍点的影响范围
        /// </summary>
        public static int EffectDistFromBlock { get { return SafeDist + (int)Size; } }
    }
    public class Map
    {
        private Node[,] nodes;

        /// <summary>
        /// 检查过的节点列表
        /// </summary>
        private List<Pos> openList = new List<Pos>();

        /// <summary>
        /// 路径
        /// </summary>
        private List<Pos> path = new List<Pos>();

        public Pos EndPos { private set; get; } = null;
        public Pos StartPos { private set; get; } = null;        
        public MapBorder Border { private set; get; }
        public Node this[Pos p]
        {
            get {
                return this[p.x, p.y];
            }
            private set {
                this[p.x, p.y] = value;
            }
        }
        public Node this[int x, int y]
        {
            get {
                if (!(CheckPosInMap(x, y)))
                    return null;
                return nodes[y, x];
            }
            private set
            {
                if (CheckPosInMap(x, y))
                    nodes[y, x] = value;
            }
        }
        private bool CheckPosInMap(int x, int y)
        {
            return x >= 0 && x < Border.x && y >= 0 && y < Border.y;
        }
        private bool CheckPosInMap(Pos p)
        {
            return CheckPosInMap(p.x, p.y);
        }

        public Map(MapBorder b)
        {
            Border = b;
            nodes = new Node[b.y, b.x];

            for (int x = 0; x < b.x; x++)
                for (int y = 0; y < b.y; y++)
                    this[x, y] = new Node(x, y);
        }
        public Map(Node[,] nodes)
        {
            this.nodes = nodes;
            Border = new MapBorder(nodes.GetLength(1), nodes.GetLength(0));
        }

        /// <summary>
        /// 重置所有节点
        /// </summary>
        private void Reset()
        {
            openList.Clear();
            foreach (var node in nodes)
                node.Reset();
        }
        
        /// <summary>
        /// 设置障碍点对周围点的影响
        /// </summary>
        /// <param name="p"></param>
        private void SetBlockEffect(Pos p)
        {
            for(int x = -RobotObj.EffectDistFromBlock; x <= RobotObj.EffectDistFromBlock; x++)
                for(int y = -RobotObj.EffectDistFromBlock; y<= RobotObj.EffectDistFromBlock; y++)
                {
                    if (x == 0 && y == 0)
                        continue;
                    var node = this[p.Shift(x, y)];
                    if (node == null || node.IsBlock)
                        continue;
                    node.AddNearBlock(p);
                }
        }

        /// <summary>
        /// 取消障碍点对周围点的影响
        /// </summary>
        /// <param name="p"></param>
        private void ClearBlockEffect(Pos p)
        {
            for (int x = -RobotObj.EffectDistFromBlock; x <= RobotObj.EffectDistFromBlock; x++)
                for (int y = -RobotObj.EffectDistFromBlock; y <= RobotObj.EffectDistFromBlock; y++)
                {
                    if (x == 0 && y == 0)
                        continue;
                    var node = this[p.Shift(x, y)];
                    if (node == null)
                        continue;


                    if (node.IsBlock)
                        //添加该点对本点的影响
                        this[p].AddNearBlock(node.Pos);
                    else
                    {
                        //去除本点对该点的影响
                        if (node.RemoveNearBlock(p))
                            GetNearBlockEffect(node.Pos);
                    }
                        
                }            
        }

        /// <summary>
        /// 获取周围障碍点对本点的影响
        /// </summary>
        /// <param name="p"></param>
        private void GetNearBlockEffect(Pos p)
        {
            for (int x = -RobotObj.EffectDistFromBlock; x <= RobotObj.EffectDistFromBlock; x++)
                for (int y = -RobotObj.EffectDistFromBlock; y <= RobotObj.EffectDistFromBlock; y++)
                {
                    if (x == 0 && y == 0)
                        continue;
                    var node = this[p.Shift(x, y)];
                    if (node == null)
                        continue;

                    if (node.IsBlock)
                        this[p].SetClosetDist(node.Pos);
                }
        }

        /// <summary>
        /// 设置为障碍点
        /// </summary>
        /// <param name="p"></param>
        public void BlockNode(Pos p)
        {
            if (this[p].IsBlock)
                return;
            this[p].Block();
            SetBlockEffect(p);

        }

        /// <summary>
        /// 设置为非障碍点
        /// </summary>
        /// <param name="p"></param>
        public void UnblockNode(Pos p)
        {
            if (!this[p].IsBlock)
                return;
            this[p].UnBlock();
            ClearBlockEffect(p);
        }

        /// <summary>
        /// 节点是否障碍点
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsNodeBlock(Pos p)
        {
            return this[p].IsBlock;
        }

        /// <summary>
        /// 判断该点是否能抵达
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsNodeCanReach(Pos p)
        {
            return !this[p].IsBlock && this[p].IsMoveable;
        }

        /// <summary>
        /// 按代价值大小插入openlist
        /// </summary>
        /// <param name="n"></param>
        private void InsertToOpen(Node n)
        {
            int index = 0;
            foreach(var pos in openList)
            {
                var _node = this[pos];
                if (n.F < _node.F)
                    break;
                else if (n.F == _node.F && n.H <= _node.H)
                    break;
                index += 1;
            }
            openList.Insert(index, n.Pos);
        }

        /// <summary>
        /// 获取四周可移动点
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private List<Pos> GetMoveable(Pos pos)
        {
            List<Pos> moveablePos = new List<Pos>();

            Func<Pos, Pos> checkBlock = p => {
                if (p == null) return null;
                if (this[p] == null || this[p].IsBlock) return null;
                return p;
            };

            Dictionary<string, Pos> adjacent = new Dictionary<string, Pos>();
            //获取上下左右点
            adjacent.Add("up",checkBlock(pos.Up));
            adjacent.Add("down", checkBlock(pos.Down));
            adjacent.Add("left", checkBlock(pos.Left));
            adjacent.Add("right", checkBlock(pos.Right));

            //获取四个角的点，若上下左右任一点不可移则相邻的角不能移
            adjacent.Add("left_up", (adjacent["up"] != null && adjacent["left"] != null)? checkBlock(pos.LeftUp) : null);
            adjacent.Add("left_down", (adjacent["down"] != null && adjacent["left"] != null) ? checkBlock(pos.LeftDown) : null);
            adjacent.Add("right_up", (adjacent["up"] != null && adjacent["right"] != null) ? checkBlock(pos.RightUp) : null);
            adjacent.Add("right_down", (adjacent["down"] != null && adjacent["right"] != null) ? checkBlock(pos.RightDown) : null);

            foreach(var p in adjacent.Values)
            {
                if (p == null || this[p].IsClosed || !this[p].IsMoveable)
                    continue;
                moveablePos.Add(p);
            }
            return moveablePos;
        }

        /// <summary>
        /// 导向下个节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool NavToNextNode(Node node)
        {
            //关闭本节点
            node.Close();           

            //获取本节点四周可移动节点
            var moveable_pos = GetMoveable(node.Pos);

            foreach(var pos in moveable_pos)
            {
                //是否可以直接到终点
                if(pos == EndPos)
                {
                    this[EndPos].SetFather(node, EndPos);
                    return true;
                }

                
                var _node = this[pos];
                if (!_node.IsCheck)
                {
                    _node.Check();
                }   
                else
                {
                    //如果该点检查过，判断从本点到该点G值是否更小
                    if (!_node.IsNewFather(node))
                        continue;
                    openList.Remove(pos);
                }
                _node.SetFather(node, EndPos);
                InsertToOpen(_node);
            }
            return false;
        }

        /// <summary>
        /// 寻找路径
        /// </summary>
        /// <param name="startPos">起始点</param>
        /// <param name="endPos">终点</param>
        /// <returns></returns>
        public bool FindPath(Pos startPos, Pos endPos)
        {
            StartPos = startPos;
            EndPos = endPos;

            if (StartPos == null || EndPos == null || StartPos == EndPos)
                return false;
            
            #region 启动计时器
            var watch = new Stopwatch();
            watch.Start();
            #endregion

            Reset();
            var _node = this[StartPos];
            while (!NavToNextNode(_node))
            {
                try
                {
                    var pos = openList.ElementAt(0);
                    openList.RemoveAt(0);
                    _node = this[pos];
                }
                catch(Exception e)
                {
                    #region 停止计时器报时
                    watch.Stop();
                    Debug.WriteLine("find time: " + watch.Elapsed);
                    #endregion

                    Console.WriteLine("find path error: " + e);
                    return false;                    
                }
            }

            #region 停止计时器报时
            watch.Stop();
            Debug.WriteLine("find time: " + watch.Elapsed);
            #endregion

            GeneratePath();
            return true;
        }        

        /// <summary>
        /// 获取所有路径点生成路径
        /// </summary>
        private void GeneratePath()
        {
            path.Clear();
            path.Add(EndPos);
            if (EndPos == null)
                return;

            var node = this[EndPos];
            while (!(node.Father == null || node.Father.Pos == StartPos))
            {
                path.Add(node.Father.Pos);
                node = node.Father;
            }
            path.Add(StartPos);
            path.Reverse();
        }
        
        /// <summary>
        /// 删除原始路径中的中间点，保留拐点
        /// </summary>
        /// <returns></returns>
        public List<Pos> SimplePath()
        {
            var simplePath = new List<Pos>();

            var startPos = path[0];
            Pos prePos = null;
            simplePath.Add(startPos);
            
            foreach(var pos in path)
            {
                if (pos == startPos) continue;

                var v = pos - startPos;
                if (v.x != 0 && v.y != 0 && v.absX != v.absY)
                {
                    simplePath.Add(prePos);
                    startPos = prePos;
                }
                prePos = pos;
            }
            simplePath.Add(path.Last());

            return simplePath;
        }
        
        /// <summary>
        /// 平滑原始路径
        /// </summary>
        /// <returns></returns>
        public List<Pos> SmoothPath()
        {
            var smoothPath = new List<Pos>();

            var startPos = path[0];
            Pos prePos = null;
            smoothPath.Add(startPos);

            #region 计时器启动
            var watch = new Stopwatch();
            watch.Start();
            #endregion

            foreach (var pos in path)
            {
                if (pos == startPos) continue;

                var v = pos - startPos;
                //判断是否在同一条直线上，是则可直接通过
                if (v.x != 0 && v.y != 0 && v.absX != v.absY)
                {
                    //判断两点间是否可通过
                    if(!CanWalk(startPos, pos))
                    {
                        //不可通过设置上一个点为起始点，并将该点加到平滑路径
                        smoothPath.Add(prePos);
                        startPos = prePos;
                    }                        
                }
                prePos = pos;
            }
            smoothPath.Add(path.Last());

            #region 计时器结束报时
            watch.Stop();
            Debug.WriteLine("smooth time: " + watch.Elapsed);
            #endregion

            return smoothPath;
        }
        
        /// <summary>
        /// 从图片加载地图
        /// </summary>
        /// <param name="picPath"> 图片路径</param>
        /// <returns></returns>
        public static Map LoadFromPic(string picPath)
        {
            var img = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(picPath);
            var map = new Map(new MapBorder(img.Width, img.Height));

            var watch = new Stopwatch();
            watch.Start();
            //二维图像数组循环  
            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    //读取当前像素的RGB颜色值
                    var curColor = img.GetPixel(x, y);
                    //利用公式计算灰度值（加权平均法）
                    if ((int)(curColor.R * 0.299 + curColor.G * 0.587 + curColor.B * 0.114) == 0)
                        map.BlockNode(new Pos(x, y));
                }
            }
            watch.Stop();
            Debug.WriteLine("construct map time: " + watch.Elapsed);
            return map;
        }

        /// <summary>
        /// 从bin文件加载
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Map LoadFromBin(string fileName)
        {
            Node[,] nodes;
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                nodes = bf.Deserialize(fs) as Node[,];                
            }
            return new Map(nodes);
        }

        /// <summary>
        /// 将节点数据存为bin文件
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, nodes);
            }
        }

        /// <summary>
        /// 判断两点间半径宽矩形范围内是否有障碍点
        /// </summary>
        /// <param name="s">起始点</param>
        /// <param name="e">终点</param>
        /// <returns></returns>
        private bool CanWalk(Pos s, Pos e)
        {
            var r = RobotObj.Size + 1;//误差值
            var v_e2s = e - s;
            var v_s2e = s - e;

            //设置范围
            float minX = (v_e2s.x >= 0 ? s.x : e.x) - r;
            float maxX = (v_e2s.x >= 0 ? e.x : s.x) + r;
            float minY = (v_e2s.y >= 0 ? s.y : e.y) - r;
            float maxY = (v_e2s.y >= 0 ? e.y : s.y) + r;
            
            for (float y = minY; y <= maxY; y++)
            {
                for (float x = minX; x <= maxX; x++)
                {
                    var p = new Pos(x, y);
                    var v2s = p - s;
                    var v2e = p - e;

                    //判断向量是否在矩形范围内
                    if (Vector.Cos(v2s, v_e2s) > 0 && Vector.Cos(v2e, v_s2e) > 0 && v2s.Length * Vector.Sin(v2s, v_e2s) <= r)
                        if (this[p].IsBlock)
                            return false;                    
                }
            }
            return true;
        }

        /// <summary>
        /// 检测某个点机器人半径圆内是否有障碍点，用于模拟导航
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool BlockDetection(float x, float y)
        {
            var r = RobotObj.Size;
            for (float _y = y - r; _y <= y + r; _y++)
                for(float _x = x - r; _x <= x + r; _x++)
                {
                    var v = new Vector(_x - x, _y - y);
                    if(v.Length < r)
                    {
                        var p = new Pos(_x, _y);
                        if (this[p].IsBlock)
                            return true;
                    }
                }
            return false;
        }
    }
}
