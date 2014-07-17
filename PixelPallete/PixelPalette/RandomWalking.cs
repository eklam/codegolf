using System;
using CSharper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using PixelPalette;

namespace PixelPallete
{
    public class RandomWalking : OrderAlgo
    {
        public override HashSet<Point> GetOrder(Point center, int Width, int Height)
        {
            var points = new HashSet<Point>();

            var dirs = GetCardinals();
            var dir = new int[] { 0, 0 };

            while (points.Count < Width * Height)
            {
                bool inWidthBound = center.X + dir[0] < Width && center.X + dir[0] >= 0;
                bool inHeightBound = center.Y + dir[1] < Height && center.Y + dir[1] >= 0;

                if (inWidthBound && inHeightBound)
                {
                    center.X += dir[0];
                    center.Y += dir[1];

                    points.Add(center);
                }

                dir = dirs.Random(PixelExchanger1.r);
            }

            return points;
        }


    }
}
