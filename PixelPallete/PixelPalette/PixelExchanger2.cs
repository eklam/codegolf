using CSharper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace PixelPalette
{
    public class PixelExchanger2 : IPixelExchanger
    {
         public readonly static Random r = new Random(0);

        private readonly Bitmap Palette;
        private readonly Bitmap Image;

        private readonly int Width;
        private readonly int Height;

        private readonly Action<PixelPallete.ProgressInfo> ProgressCallback;
        private System.Drawing.Image image1;
        private System.Drawing.Image image2;

        private int Area { get { return Width * Height; } }

        public PixelExchanger2(Bitmap Palette, Bitmap image, Action<PixelPallete.ProgressInfo> progressCallback = null)
        {
            this.Palette = Palette;
            this.Image = image;

            this.ProgressCallback = progressCallback;

            Width = image.Width;
            Height = image.Height;

            if (Area != Palette.Width * Palette.Height)
                throw new ArgumentException("Image and Palette have different areas!");
        }

        public Bitmap DoWork()
        {
            var palette = GetPalette(GetColorMap(Palette));
            var map = GetColorMap(Image);
            var newMap = Go(palette, map);

            var bm = new Bitmap(map.Length, map[0].Length);

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    bm.SetPixel(i, j, newMap[i][j]);
                }
            }

            return bm;
        }

        public Color[][] Go(IEnumerable<Pixel> palette, Color[][] map)
        {
            var centralPoint = new Point(Width / 2, Height / 2);

            //var q = OrderRandomWalking(centralPoint).ToArray();

            Color[][] newMap = new Color[map.Length][];
            for (int i = 0; i < map.Length; i++)
            {
                newMap[i] = new Color[map[i].Length];
            }

            double pointsDone = 0;

            var ps = palette.OrderByDescending(x => ToNumeric(x.Color)).ToList();

            var ss = GetPalette(map).OrderByDescending(x => ToNumeric(x.Color)).ToArray();

            for (int i = 0; i < ss.Length / 2; i++)
            {
                var oPixel1 = ss[i];
                var oPixel2 = ss[ss.Length - i - 1];

                NewMethod(newMap, ps, oPixel1, i, true);
                NewMethod(newMap, ps, oPixel2, ss.Length - i - 1);

                pointsDone++;
                pointsDone++;

                if (ProgressCallback != null)
                {
                    var percent = 100 * (pointsDone / (double)Area);

                    {
                        var oPixel = oPixel1;
                        var progressInfo = new PixelPallete.ProgressInfo(new Pixel(oPixel.X, oPixel.Y, newMap[oPixel.X][oPixel.Y]), (int)percent);

                        ProgressCallback(progressInfo);
                    }
                    {
                        var oPixel = oPixel2;
                        var progressInfo = new PixelPallete.ProgressInfo(new Pixel(oPixel.X, oPixel.Y, newMap[oPixel.X][oPixel.Y]), (int)percent);

                        ProgressCallback(progressInfo);
                    }
                }

            }





            //foreach (var p in q)
            //{
            //    newMap[p.X][p.Y] = Closest(palette, map[p.X][p.Y]);

            //    pointsDone++;

            //    if (ProgressCallback != null)
            //    {
            //        var percent = 100 * (pointsDone / (double)Area);

            //        var progressInfo = new ProgressInfo(new Pixel(p.X, p.Y, newMap[p.X][p.Y]), (int)percent);

            //        ProgressCallback(progressInfo);
            //    }
            //}

            return newMap;
        }

        private void NewMethod(Color[][] newMap, List<Pixel> palette, Pixel targetPixel, int i, bool useRandom = false)
        {

            if (useRandom && r.NextDouble() < 0.7)
            {
                int index = ClosestIndex(palette, targetPixel.Color);

                var aux = palette[i];
                palette[i] = palette[index];
                palette[index] = aux;

            }

            newMap[targetPixel.X][targetPixel.Y] = palette[i].Color;

        }

        private static double ToNumeric(Color c)
        {
            return Math.Pow(c.R / 255d, 2) + Math.Pow(c.G / 255d, 2) + Math.Pow(c.B / 255d, 2);
        }

        private int[][] GetCardinals()
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

        private int ClosestIndex(List<Pixel> array, Color c)
        {
            int closestIndex = -1;

            double bestD = double.MaxValue;


            double[] ds = new double[array.Count];
            Parallel.For(0, array.Count, (i, state) =>
            {
                ds[i] = Distance(array[i].Color, c);

                if (ds[i] <= 0.15)
                {
                    closestIndex = i;
                    state.Break();
                }
                else if (bestD > ds[i])
                {
                    bestD = ds[i];
                    closestIndex = i;
                }
            });

            return closestIndex;
        }

        private double Distance(Color c1, Color c2)
        {
            double r = Math.Abs((double)c1.R / 255d - (double)c2.R / 255d);
            double g = Math.Abs((double)c1.G / 255d - (double)c2.G / 255d);
            double b = Math.Abs((double)c1.B / 255d - (double)c2.B / 255d);
            double hue = 2 * Math.Abs(((double)c1.GetHue() / 360d) - ((double)c2.GetHue() / 360d));

            return (r +
                   g +
                   b +
                   hue)/5d;
        }



        private HashSet<Point> OrderRandomWalking(Point p)
        {
            var points = new HashSet<Point>();

            var dirs = GetCardinals();
            var dir = new int[] { 0, 0 };

            while (points.Count < Width * Height)
            {
                bool inWidthBound = p.X + dir[0] < Width && p.X + dir[0] >= 0;
                bool inHeightBound = p.Y + dir[1] < Height && p.Y + dir[1] >= 0;

                if (inWidthBound && inHeightBound)
                {
                    p.X += dir[0];
                    p.Y += dir[1];

                    points.Add(p);
                }

                dir = dirs.Random(r);
            }

            return points;
        }

        public static Color[][] GetColorMap(Bitmap b1)
        {
            int hight = b1.Height;
            int width = b1.Width;

            Color[][] colorMatrix = new Color[width][];
            for (int i = 0; i < width; i++)
            {
                colorMatrix[i] = new Color[hight];
                for (int j = 0; j < hight; j++)
                {
                    colorMatrix[i][j] = b1.GetPixel(i, j);
                }
            }
            return colorMatrix;
        }

        private IEnumerable<Pixel> GetPalette(Color[][] map)
        {
            var palette = new HashSet<Pixel>();

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    palette.Add(new Pixel(i, j, map[i][j]));
                }
            }

            return palette;
        }
    }
}