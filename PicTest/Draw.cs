using PathFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PicTest
{
    class BlockPoint
    {
        private Path path;
        private Brush color = Brushes.Gray;
        private Canvas canvas;

        public BlockPoint(Canvas canvas, Point p)
        {
            this.canvas = canvas;
            Drawing(p);
        }
        public BlockPoint(Canvas canvas, Pos p):this(canvas, new Point(p.x, p.y)) { }
        
        private Path GetPath(Point center)
        {
            EllipseGeometry ellipseGeometry = new EllipseGeometry(center, 1, 1);

            Path path = new Path();
            path.Stroke = color;
            path.StrokeThickness = 1;
            path.Data = ellipseGeometry;
            return path;
        }

        private void Drawing(Point center)
        {
            Erase();
            path = GetPath(center);
            canvas.Children.Add(path);
        }
        
        public void Erase()
        {
            if (path != null)
                canvas.Children.Remove(path);
        }
        
    }

    class Fork
    {
        private Path p1;
        private Path p2;
        private Brush color;
        private Canvas canvas;
        private int size;
        public Fork(Canvas canvas, Brush color, int size=2)
        {
            this.canvas = canvas;
            this.color = color;
            this.size = size;
        }
        private Path GetPath(Point p, int x, int y)
        {
            LineGeometry lineGeometry = new LineGeometry();
            lineGeometry.StartPoint = new Point(p.X - x, p.Y - y);
            lineGeometry.EndPoint = new Point(p.X + x, p.Y + y);

            Path path = new Path();
            path.Stroke = color;
            path.StrokeThickness = 2;
            path.Data = lineGeometry;
            return path;
        }

        public void Drawing(Point center)
        {
            Erase();
            p1 = GetPath(center, size, size);
            p2 = GetPath(center, size, -size);
            canvas.Children.Add(p1);
            canvas.Children.Add(p2);
        }
        
        private void Erase()
        {
            if(p1 != null)
                canvas.Children.Remove(p1);
            if(p2 != null)
                canvas.Children.Remove(p2);
        }
    }

    class RoutePath
    {
        private List<Path> paths;
        private Brush color;
        private Canvas canvas;
        public RoutePath(Canvas canvas, Brush color)
        {
            this.canvas = canvas;
            this.color = color;
            paths = new List<Path>();
        }

        public RoutePath(Canvas canvas, Brush color, List<Pos> p):this(canvas, color)
        {
            Drawing(p);
        }

        private Path GetPath(Pos s, Pos e)
        {
            LineGeometry lineGeometry = new LineGeometry();
            lineGeometry.StartPoint = new Point(s.x, s.y);
            lineGeometry.EndPoint = new Point(e.x, e.y);

            Path path = new Path();
            path.Stroke = color;
            path.StrokeThickness = 2;
            path.Data = lineGeometry;
            return path;
        }

        public void Drawing(List<Pos> pos)
        {

            Erase();
            paths.Clear();
            for(int i=1; i<pos.Count; i++)
            {
                var p = GetPath(pos[i - 1], pos[i]);
                paths.Add(p);
                canvas.Children.Add(p);
            }
        }

        public void Erase()
        {
            foreach(var path in paths)
            {
                canvas.Children.Remove(path);
            }
        }
    }

    class Circle
    {
        private Path path;
        private Brush color;
        private Canvas canvas;
        private double size;

        public Circle(Canvas canvas, Brush color, double size = 2)
        {
            this.canvas = canvas;
            this.color = color;
            this.size = size;
        }

        private Path GetPath(Point center)
        {
            EllipseGeometry ellipseGeometry = new EllipseGeometry(center, size, size);

            Path path = new Path();
            path.Stroke = color;
            path.StrokeThickness = 1;
            path.Data = ellipseGeometry;
            return path;
        }

        public void Drawing(Point center)
        {
            Erase();
            path = GetPath(center);
            canvas.Children.Add(path);
        }

        private void Erase()
        {
            if (path != null)
                canvas.Children.Remove(path);
        }
    }

    class Robot
    {
        private Circle c;
        private Fork f;

        public Robot(Canvas canvas)
        {
            f = new Fork(canvas, Brushes.Blue);
            c = new Circle(canvas, Brushes.Aqua, RobotObj.Size);
        }

        public void Drawing(Point center)
        {
            f.Drawing(center);
            c.Drawing(center);
        }
        
    }
}
