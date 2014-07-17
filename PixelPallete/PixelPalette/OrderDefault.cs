using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PixelPallete
{
    public class OrderDefault : OrderAlgo
    {
        public override HashSet<Point> GetOrder(Point center, int Width, int Height)
        {
            HashSet<Point> q = new HashSet<Point>();

            var nextPoints = new Queue<Point>();

            nextPoints.Enqueue(new Point(center.X, center.Y));

            while (nextPoints.Count > 0)
            {
                Point pp = nextPoints.Dequeue();

                OrderDefault_AddPoint(pp, Width, Height, nextPoints, q);
            }

            return q;
        }

        public static void OrderDefault_AddPoint(Point pp, int wid, int hei, Queue<Point> nextPoints, HashSet<Point> q)
        {
            int x = pp.X;
            int y = pp.Y;

            var dirs = GetCardinals();

            foreach (var dir in dirs)
            {
                bool inWidthBound = x + dir[0] < wid && x + dir[0] >= 0;
                bool inHeightBound = y + dir[1] < hei && y + dir[1] >= 0;
                if (inWidthBound && inHeightBound)
                {
                    var np = new Point(x + dir[0], y + dir[1]);

                    if (!q.Contains(np))
                    {
                        q.Add(np);
                        nextPoints.Enqueue(np);
                    }
                }
            }
        }
    }
}