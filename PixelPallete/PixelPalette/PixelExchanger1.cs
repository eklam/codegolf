using CSharper;
using PixelPallete;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace PixelPalette
{
    public class PixelExchanger1 : IPixelExchanger
    {
        public static Random r = new Random(0);

        private readonly Bitmap Palette;
        private readonly Bitmap Image;

        private readonly int Width;
        private readonly int Height;

        private readonly OrderAlgo OrderAlgo;

        private readonly Action<ProgressInfo> ProgressCallback;
        private System.Drawing.Image image1;
        private System.Drawing.Image image2;

        private int Area { get { return Width * Height; } }

        public PixelExchanger1(Bitmap Palette, Bitmap image, OrderAlgo OrderAlgo, Action<ProgressInfo> progressCallback = null)
        {
            this.Palette = Palette;
            this.Image = image;

            this.OrderAlgo = OrderAlgo;

            this.ProgressCallback = progressCallback;

            Width = image.Width;
            Height = image.Height;

            if (Area != Palette.Width * Palette.Height)
                throw new ArgumentException("Image and Palette have different areas!");
        }

        public Bitmap DoWork()
        {
            var array = GetColorArray();
            var map = GetColorMap(Image);
            var newMap = Go(array, map);

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

        public Color[][] Go(List<Color> array, Color[][] map)
        {
            var centralPoint = new Point(Width / 2, Height / 2);

            var q = OrderAlgo.GetOrder(centralPoint, Width, Height).ToArray();

            Color[][] newMap = new Color[map.Length][];
            for (int i = 0; i < map.Length; i++)
            {
                newMap[i] = new Color[map[i].Length];
            }

            double pointsDone = 0;

            foreach (var p in q)
            {
                newMap[p.X][p.Y] = Closest(array, map[p.X][p.Y]);

                pointsDone++;

                if (ProgressCallback != null)
                {
                    var percent = 100 * (pointsDone / (double)Area);

                    var progressInfo = new ProgressInfo(new PixelPalette.Pixel(p.X, p.Y, newMap[p.X][p.Y]), (int)percent);

                    ProgressCallback(progressInfo);
                }
            }

            return newMap;
        }

        private Color Closest(List<Color> array, Color c)
        {
            int closestIndex = -1;

            int bestD = int.MaxValue;

            int[] ds = new int[array.Count];
            Parallel.For(0, array.Count, (i, state) =>
            {
                ds[i] = Distance(array[i], c);

                if (ds[i] <= 50)
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

            var closestColor = array[closestIndex];

            array.RemoveAt(closestIndex);

            return closestColor;
        }

        private int Distance(Color c1, Color c2)
        {
            var r = Math.Abs(c1.R - c2.R);
            var g = Math.Abs(c1.G - c2.G);
            var b = Math.Abs(c1.B - c2.B);
            var s = Math.Abs(c1.GetSaturation() - c1.GetSaturation());

            return (int)s + r + g + b;
        }

        private static Color[][] GetColorMap(Bitmap b1)
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

        private List<Color> GetColorArray()
        {
            var map = GetColorMap(Palette);

            List<Color> colors = new List<Color>();

            foreach (var line in map)
            {
                colors.AddRange(line);
            }

            return colors;
        }
    }
}