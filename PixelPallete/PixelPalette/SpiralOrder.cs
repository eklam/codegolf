using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelPallete
{
    public class SpiralOrder : OrderAlgo
    {
        private HashSet<Point> Spiral(int X, int Y)
        {
            HashSet<Point> incs = new HashSet<Point>();

            int x = 0, y = 0;
            int dx = 0;
            int dy = -1;

            var max = (int)Math.Pow(Math.Max(X, Y), 2);
            for (int i = 0; i < max; i++)
            {
                if ((-X / 2 < x && x <= X / 2) && (-Y / 2 < y && y <= Y / 2))
                {
                    incs.Add(new Point(x, y));
                }
                if (x == y || (x < 0 && x == -y) || (x > 0 && x == 1 - y))
                {
                    int aux = dx;
                    dx = -dy;
                    dy = aux;
                }
                x += dx;
                y += dy;
            }
            return incs;
        }

        public override HashSet<Point> GetOrder(Point center, int Width, int Height)
        {
            HashSet<Point> q = new HashSet<Point>();
            var incs = Spiral(Width, Height);
            foreach (var item in incs)
            {
                bool inWidthBound = center.X + item.X < Width && center.X + item.X >= 0;
                bool inHeightBound = center.Y + item.Y < Height && center.Y + item.Y >= 0;
                if (inWidthBound && inHeightBound)
                    q.Add(new Point(center.X + item.X, center.Y + item.Y));
            }
            return q;
            throw new NotImplementedException();
        }
    }
}