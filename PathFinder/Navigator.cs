using PicTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Media;

namespace PathFinder
{
    public class Navigator
    {
        /// <summary>
        /// 当前位置
        /// </summary>
        private Pos localPos;

        /// <summary>
        /// 目标位置
        /// </summary>
        private Pos targetPos;
        public Map map { get; private set; }
        public List<Pos> SmoothPath { get; private set; }

        #region 画图委托
        private event EventHandler<DrawObjEventArgs> drawObjEvent;
        private event EventHandler<DrawPathEventArgs> drawPathEvent;
        private event EventHandler erasePathEvent;
        #endregion

        public Navigator(Map map, EventHandler<DrawObjEventArgs> e1, EventHandler<DrawPathEventArgs> e2, EventHandler e3)
        {
            this.map = map;
            drawObjEvent += e1;
            drawPathEvent += e2;
            erasePathEvent += e3;
        }

        public bool SetStart(double x, double y)
        {
            var p = new Pos(x, y);
            if (!map.IsNodeCanReach(p))
                return false;
            localPos = p;
            return true;
        }

        public bool SetEnd(double x, double y)
        {
            var p = new Pos(x, y);
            if (!map.IsNodeCanReach(p))
                return false;
            targetPos = p;
            return true;
        }

        public bool Find(bool isConcat = false)
        {
            if (!map.FindPath(localPos, targetPos))
                return false;

            if (isConcat)
            {
                //拼接到原路径，用于重新规划路径
                SmoothPath.AddRange(map.SmoothPath());
                erasePathEvent?.Invoke(null, null);
            }                
            else
                SmoothPath = map.SmoothPath();

            //drawPathEvent?.Invoke(null, new DrawPathEventArgs(OriginPath, Brushes.Green));
            drawPathEvent?.Invoke(null, new DrawPathEventArgs(SmoothPath, Brushes.DeepPink));
            return true;
        }        

        /// <summary>
        /// 模拟移动
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool Move(Pos s, Pos e)
        {
            var v_e2s = e - s;

            //x， y移动方向
            var _x = Vector.Cos(v_e2s, Vector.HorizontalVector) > 0 ? 1 : -1;
            var _y = Vector.Cos(v_e2s, Vector.VerticalVector) > 0 ? 1 : -1;

            //x，y步伐
            var x_step = (_x > 0 ? Vector.Cos(v_e2s, Vector.HorizontalVector) : Vector.Cos(v_e2s, Vector.ReverseHorizontalVector)) * _x;
            var y_step = (_y > 0 ? Vector.Cos(v_e2s, Vector.VerticalVector) : Vector.Cos(v_e2s, Vector.ReverseVerticalVector)) * _y;

            //障碍检测步伐
            var x_dection_step = x_step * RobotObj.Size;
            var y_dection_step = y_step * RobotObj.Size;


            var local_x = (double)s.x;
            var local_y = (double)s.y;
            
            for (int i = 1; i < v_e2s.Length; i++)
            {
                #region 模拟障碍物检测
                var dection_x = (float)(local_x + x_dection_step);
                var dection_y = (float)(local_y + y_dection_step);
                if((Math.Pow(dection_x - s.x, 2) + Math.Pow(dection_y - s.y, 2)) < Math.Pow(v_e2s.Length, 2))
                {
                    if (map.BlockDetection(dection_x, dection_y))
                        return false;
                }
                else
                {
                    if (map.BlockDetection(e.x, e.y))
                        return false;
                }
                #endregion

                local_x += x_step;
                local_y += y_step;
                SetLocation(local_x, local_y);
            }
            SetLocation(e.x, e.y);
            return true;
        }

        /// <summary>
        /// 模拟导航
        /// </summary>
        public void Navigate()
        {
            
            SetLocation(SmoothPath[0].x, SmoothPath[0].y);

            var index = 1;
            while(localPos != map.EndPos)
            {
                //路径最后不是终点，继续规划后续路径
                if(index >= SmoothPath.Count)
                {
                    SmoothPath.RemoveAt(index - 1);
                    if(!Find(true))
                    {
                        SmoothPath.Add(localPos);
                        Debug.WriteLine("no exist path!");
                        break;
                    }
                    continue;
                }

                if(!Move(SmoothPath[index-1], SmoothPath[index]))
                {
                    Debug.WriteLine("detect block");
                    SmoothPath.RemoveRange(index, SmoothPath.Count - index);
                    if (!Find(true))
                    {
                        //检测到障碍重新规划后续路径
                        SmoothPath.Add(localPos);
                        Debug.WriteLine("no exist path!");
                        break;
                    }                        
                }
                index++;
            }
            Debug.WriteLine("nav done!");
        }

        /// <summary>
        /// 设置当前位置
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetLocation(double x, double y)
        {
            localPos = new Pos(x, y);
            drawObjEvent?.Invoke(null, new DrawObjEventArgs(x, y));
            Wait();
        }

        public void Wait()
        {
            Thread.Sleep(20);
        }
    }
}
