using CSharper;
using PixelPallete;
using System;

using System;

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace PixelPalette
{
    public partial class Form1 : Form
    {
        private Random r = new System.Random();

        public Form1()
        {
            InitializeComponent();
        }

        private string PalettePath;
        private string imagePath;

        private void lblPalette_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PalettePath = openFileDialog.FileName;
                lblPalette.Image = Image.FromFile(PalettePath);
            }

            TryToGo();
        }

        private void lblImage_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                imagePath = openFileDialog.FileName;

                Image img = Image.FromFile(imagePath);

                lblImage.Image = GetResizedImage(img, lblImage.ClientRectangle);

                this.Update();
            }
            TryToGo();
        }

        public static Image GetResizedImage(Image img, Rectangle rect)
        {
            Bitmap b = new Bitmap(rect.Width, rect.Height);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(img, 0, 0, rect.Width, rect.Height);
            g.Dispose();

            try
            {
                return (Image)b.Clone();
            }
            finally
            {
                b.Dispose();
                b = null;
                g = null;
            }
        }

        private void TryToGo()
        {
            if (string.IsNullOrWhiteSpace(PalettePath)) return;
            if (string.IsNullOrWhiteSpace(imagePath)) return;

            var originalImage = Bitmap.FromFile(imagePath);

            var newImage = new Bitmap(originalImage.Width, originalImage.Height);
            for (int i = 0; i < newImage.Width; i++)
            {
                for (int j = 0; j < newImage.Height; j++)
                {
                    newImage.SetPixel(i, j, Color.White);
                }
            }

            pictureBox1.Image = newImage;

            if (cmbVersion.SelectedText == "1")
            {
                PE = new PixelExchanger1(
                  (Bitmap)Bitmap.FromFile(PalettePath),
                  (Bitmap)Bitmap.FromFile(imagePath),
                  new RandomWalking(),
                  peProgress);
            }
            else
            {
                PE = new PixelExchanger2(
                    (Bitmap)Bitmap.FromFile(PalettePath),
                    (Bitmap)Bitmap.FromFile(imagePath),
                    peProgress);
            }

            backgroundWorker1.RunWorkerAsync();
        }

        private DateTime lastTime = DateTime.MinValue;

        private List<Pixel> equis = new List<Pixel>();
        private List<PixelEnhancer.Swap> swaps = new List<PixelEnhancer.Swap>();

        private void peProgress(ProgressInfo progressInfo)
        {
            equis.Add(progressInfo.NewPixel);

            if (DateTime.Now - lastTime > new TimeSpan(0, 0, 0, 0, 500))
            {
                backgroundWorker1.ReportProgress(progressInfo.Percentage, equis.Select(x => x).ToArray());

                equis.Clear();

                lastTime = DateTime.Now;
            }
        }

        private void peProgress(PixelEnhancer.ProgressInfo progressInfo)
        {
            swaps.Add(progressInfo.swap);

            if (DateTime.Now - lastTime > new TimeSpan(0, 0, 0, 0, 500))
            {
                backgroundWorker1.ReportProgress(progressInfo.Percentage, swaps.Select(x => x).ToArray());

                swaps.Clear();

                lastTime = DateTime.Now;
            }
        }

        private Bitmap resultBitmap = null;

        private IPixelExchanger PE = null;

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            resultBitmap = PE.DoWork();

            pictureBox1.Image = resultBitmap;
        }

        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            var bm = pictureBox1.Image as Bitmap;

            if (e.UserState is IEnumerable<Pixel>)
            {
                var pixels = e.UserState as IEnumerable<Pixel>;

                foreach (Pixel x in pixels)
                {
                    bm.SetPixel(x.X, x.Y, x.Color);
                }

                pictureBox1.Image = bm;

                progressBar.Value = e.ProgressPercentage;
            }
            else if (e.UserState is IEnumerable<PixelEnhancer.Swap>)
            {
                var swaps = e.UserState as IEnumerable<PixelEnhancer.Swap>;

                foreach (PixelPalette.PixelEnhancer.Swap s in swaps)
                {
                    var aux = bm.GetPixel(s.i1, s.j1);
                    bm.SetPixel(s.i1, s.j1, bm.GetPixel(s.i2, s.j2));
                    bm.SetPixel(s.i2, s.j2, aux);
                }

                pictureBox1.Image = bm;

                progressBar.Value = e.ProgressPercentage;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            var str = DateTime.Now.ToShortTimeString().Replace(":", "");

            pictureBox1.Image = resultBitmap;

            resultBitmap.Save(@"C:\Users\rmalke\Documents\Visual Studio 2013\Projects\PixelPalette\PixelPalette\src\" + str + ".png");
        }
    }
}