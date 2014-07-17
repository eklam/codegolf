using PixelPalette;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelPallete
{
    public class ProgressInfo
    {
        public readonly Pixel NewPixel;
        public readonly int Percentage;

        public ProgressInfo(Pixel newPixel, int percentage)
        {
            this.NewPixel = newPixel;
            this.Percentage = percentage;
        }
    }
}
