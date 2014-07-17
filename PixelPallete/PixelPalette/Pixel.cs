using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelPalette
{
    public class Pixel
    {
        public readonly int X;
        public readonly int Y;
        public Color Color;

        public Pixel(int x, int y, Color color)
        {
            this.X = x;
            this.Y = y;
            this.Color = color;
        }
    }
}
