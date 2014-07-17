using CSharper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace PixelPallete
{
    public abstract class OrderAlgo
    {
        public abstract HashSet<Point> GetOrder(Point center, int Width, int Height);

        protected static int[][] GetCardinals()
        {
            int[] nn = new int[] { -1, +0 };
            // int[] ne = new int[] { -1, +1 };
            int[] ee = new int[] { +0, +1 };
            // int[] se = new int[] { +1, +1 };
            int[] ss = new int[] { +1, +0 };
            // int[] sw = new int[] { +1, -1 };
            int[] ww = new int[] { +0, -1 };
            // int[] nw = new int[] { -1, -1 };

            var dirs = new List<int[]>();

            dirs.Add(nn);
            // dirs.Add(ne);
            dirs.Add(ee);
            // dirs.Add(se);
            dirs.Add(ss);
            // dirs.Add(sw);
            dirs.Add(ww);
            // dirs.Add(nw);

            return dirs.ToArray();
        }
    }

    
}
