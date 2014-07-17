using CSharper;
using PixelPallete;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PixelPalette
{
    public class Backup : OrderAlgo
    {
        public static void OrderDefault_AddPointRandom(Point pp, int wid, int hei, Queue<Point> nextPoints, HashSet<Point> q)
        {
            int x = pp.X;
            int y = pp.Y;

            var dirs = GetCardinals();

            Point? choosen = null;

            foreach (var dir in dirs.Shuffle(PixelExchanger1.r))
            {
                bool inWidthBound = x + dir[0] < wid && x + dir[0] >= 0;
                bool inHeightBound = y + dir[1] < hei && y + dir[1] >= 0;
                if (inWidthBound && inHeightBound)
                {
                    var np = new Point(x + dir[0], y + dir[1]);

                    if (!q.Contains(np))
                    {
                        if (choosen == null)
                            choosen = np;
                    }

                    if (!nextPoints.Contains(np))
                    {
                        nextPoints.Enqueue(np);
                    }
                }
            }

            if (choosen.HasValue && !q.Contains(choosen.Value))
                q.Add(choosen.Value);
        }

        public override HashSet<Point> GetOrder(Point center, int Width, int Height)
        {
            HashSet<Point> q = new HashSet<Point>();

            var nextPoints = new Queue<Point>();

            nextPoints.Enqueue(center);

            while (nextPoints.Count > 0)
            {
                Point pp = nextPoints.Dequeue();

                OrderDefault_AddPointRandom(pp, Width, Height, nextPoints, q);
            }

            return q;
        }
    }
}