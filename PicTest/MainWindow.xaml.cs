using PathFinder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PicTest
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        const string picPrefix = "t6";
        const string picPath = picPrefix + ".png";
        const string picBin = picPrefix + ".bin";
        private Navigator navigator;
        private Fork end;
        private Robot robot;
        private Thread navThread;

        private List<RoutePath> paths = new List<RoutePath>();
        private bool IsSetingBlock = false;
        private Dictionary<Pos, BlockPoint> tmpBlock = new Dictionary<Pos, BlockPoint>();
        
        public MainWindow()
        {
            InitializeComponent();            
            var imageSource = new BitmapImage(new Uri(picPath, UriKind.RelativeOrAbsolute));
            canvas.MinHeight = canvas.MaxHeight = imageSource.Height;
            canvas.MinWidth = canvas.MaxWidth = imageSource.Width;
            canvas.Background = new ImageBrush(imageSource);

            Map map;
            if (File.Exists(picBin))
            {
                map = Map.LoadFromBin(picBin);
                Debug.WriteLine("load from bin");
            }
            else
            {
                map = Map.LoadFromPic(picPath);
                //map.Save(picBin);
                Debug.WriteLine("load from pic");
            }
            navigator = new Navigator(map, DrawObj, DrawPath, ErasePath);
            end = new Fork(canvas, Brushes.Red);
            robot = new Robot(canvas);
            
        }

        #region 设置起始点终点
        private void SetPoint(object sender, MouseEventArgs e)
        {
            if ((bool)StartType.IsChecked)
                SetStart(sender, e);
            else
                SetEnd(sender, e);
        }
        private void SetStart(object sender, MouseEventArgs e)
        {
            Clear();
            var p = e.GetPosition(sender as Canvas);
            if (navigator.SetStart(p.X, p.Y))
            {
                robot.Drawing(p);
            }
            else
            {
                MessageBox.Show("point can't reach");
            }
        }
        private void SetEnd(object sender, MouseEventArgs e)
        {
            Clear();
            var p = e.GetPosition(sender as Canvas);
            if (navigator.SetEnd(p.X, p.Y))
            {
                end.Drawing(p);
            }
            else
            {
                MessageBox.Show("point can't reach");
            }
        }
        #endregion

        #region 设置障碍点
        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsSetingBlock = true;
        }

        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsSetingBlock = false;
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsSetingBlock)
                return;

            var p = e.GetPosition(sender as Canvas);
            var pos = new Pos(p.X, p.Y);
            if ((bool)SetBlock.IsChecked)
                SetBlockPoint(pos);
            else
            {
                for(int x = pos.x - 5; x <= pos.x + 5; x++)
                    for(int y = pos.y - 5; y <= pos.y + 5; y++)
                        CleanBlockPoint(new Pos(x, y));
            }
                
        }

        private void SetBlockPoint(Pos pos)
        {
            if (tmpBlock.Keys.Contains(pos))
                return;
            if (navigator.map.IsNodeBlock(pos))
                return;
            tmpBlock.Add(pos, new BlockPoint(canvas, pos));
            navigator.map.BlockNode(pos);
        }

        private void CleanBlockPoint(Pos pos)
        {
            if (!tmpBlock.Keys.Contains(pos))
                return;
            if (!navigator.map.IsNodeBlock(pos))
                return;
            tmpBlock[pos].Erase();
            tmpBlock.Remove(pos);
            navigator.map.UnblockNode(pos);
        }
        #endregion

        private void find_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            if (navigator.Find())
            {
                move.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("No path exist");
                move.IsEnabled = false;
            }
        }
        private void move_Click(object sender, RoutedEventArgs e)
        {
            if (navThread != null && navThread.IsAlive)
                return;
            navThread = new Thread(new ThreadStart(navigator.Navigate));
            navThread.Start();
        }

        private void Clear()
        {
            move.IsEnabled = false;
            foreach(var p in paths)
            {
                p.Erase();
            }
            paths.Clear();
        }

        public delegate void DrawDel();
        private void DrawObj(object _, DrawObjEventArgs e)
        {
            Dispatcher.BeginInvoke(new DrawDel(() => {
                robot.Drawing(new Point(e.x, e.y));
            }));                            
        }
        
        private void DrawPath(object _, DrawPathEventArgs e)
        {
            Dispatcher.BeginInvoke(new DrawDel(()=> {
                var path = new RoutePath(canvas, e.color, e.path);
                paths.Add(path);
            }));
        }

        private void ErasePath(object _, EventArgs e)
        {
            Dispatcher.BeginInvoke(new DrawDel(()=> {
                foreach(var p in paths)
                {
                    p.Erase();
                }
                paths.Clear();
            }));
        }
                       
    }

    public class DrawObjEventArgs : EventArgs
    {
        public readonly double x;
        public readonly double y;
        public DrawObjEventArgs(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class DrawPathEventArgs : EventArgs
    {
        public readonly List<Pos> path;
        public readonly Brush color;
        public DrawPathEventArgs(List<Pos> path, Brush color)
        {
            this.path = path;
            this.color = color;
        }
    }
}
