using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelPalette
{
    public class PixelEnhancer
    {
        public class Solution
        {
            protected readonly Color[][] Map;

            public Solution(Color[][] bm)
            {
                Map = bm;
            }

            public Solution(Solution s) : this(s.Map)
            {

            }

            public virtual Color this[int i, int j]
            {
                get
                {
                    return Map[i][j];
                }
            }

            public double Calculate(Solution Goal)
            {
                double difSum = 0;

                for (int i = 0; i < Map.Length; i++)
                {
                    for (int j = 0; j < Map[i].Length; j++)
                    {
                        difSum += PixelEnhancer.distance(this[i, j], Goal[i, j]);
                    }
                }

                this.E = difSum;
                return E;
            }

            public double E { get; protected set; }
        }

        public class NeiSolution : Solution
        {
            public readonly Swap swap;
          
            public NeiSolution(Solution s, Swap swap)
                : base(s)
            {
                this.swap = swap;
                this.E = s.E;
            }

            public override Color this[int i, int j]
            {
                get
                {
                    if (i == swap.i1 && j == swap.j1)
                    {
                        return Map[swap.i2][swap.j2];
                    }
                    else if (i == swap.i2 && j == swap.j2)
                    {
                        return Map[swap.i1][swap.j1];
                    }
                    return Map[i][j];
                }
            }

            internal Solution Consolidate()
            {
                var aux = Map[swap.i1][swap.j1];
                Map[swap.i1][swap.j1] = Map[swap.i2][swap.j2];
                Map[swap.i2][swap.j2] = aux;
                return new Solution(Map);
            }

            public double Recalc(Solution Goal)
            {
                var dSub1 = PixelEnhancer.distance(Map[swap.i1][swap.j1], Goal[swap.i1, swap.j1]);
                var dSub2 = PixelEnhancer.distance(Map[swap.i2][swap.j2], Goal[swap.i2, swap.j2]);

                var dAdd1 = PixelEnhancer.distance(Map[swap.i2][swap.j2], Goal[swap.i1, swap.j1]);
                var dAdd2 = PixelEnhancer.distance(Map[swap.i1][swap.j1], Goal[swap.i2, swap.j2]);

                E -= dSub1;
                E -= dSub2;

                E += dAdd1;
                E += dAdd2;

                return E;
            }
        }

        public class Swap {

            public readonly int i1;
            public readonly int j1;
            
            public readonly int i2;
            public readonly int j2;

            public Swap(int i1, int j1, int i2, int j2)
            {
                this.i1 = i1;
                this.j1 = j1;

                this.i2 = i2;
                this.j2 = j2;
            }
        }

        public class ProgressInfo
        {
            public readonly Swap swap;
            public readonly int Percentage;

            public ProgressInfo(Swap swap, int percentage)
            {
                this.swap = swap;
                this.Percentage = percentage;
            }
        }

        // TODO: Use your centralized Random here
        private Random r = PixelExchanger2.r;

        private Solution Goal;
        private Color[][] newImage;

        private readonly int Width;
        private readonly int Height;

        public PixelEnhancer(Color[][] originalImage, Color[][] newImage)
        {
            this.Goal = new Solution(originalImage);
            this.newImage = newImage;

            Width = newImage.Length;
            Height = newImage[0].Length;
        }

        public Bitmap Enhance(Action<PixelEnhancer.ProgressInfo> callback)
        {
            var better = SimulatedAnnealing(new Solution(newImage), callback);

            Bitmap bm = new Bitmap(Width, Height);

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    bm.SetPixel(i, j, better[i, j]);
                }
            }

            return bm;
        }

        private Solution SimulatedAnnealing(Solution initialSolution, Action<PixelEnhancer.ProgressInfo> callback)
        {
            int kmax = int.MaxValue;

            // Initial state, energy.
            var s = initialSolution;
            var e = s.Calculate(Goal);

            // Initial "best" solution
            var sbest = s;
            var ebest = e;

            // Energy evaluation count.
            int k = 0;

            // While time left & not good enough:
            while (k < kmax/* && e > emax*/)
            {
               // var T = Temperature(k / kmax); // Temperature calculation.
                var snew = Neighbour(s);       // Pick some neighbour.
                var enew = snew.Recalc(Goal);            // Compute its energy.

                // Should we move to it?
                //if (P(e, enew, T) > RandomDouble())
                //{
                //    // Yes, change state.
                //    s = snew.Consolidate();
                //    e = enew;
                //}

                // Is this a new best?
                if (enew < ebest)
                {
                    // Save 'new neighbour' to 'best found'
                    sbest = snew.Consolidate();
                    ebest = enew;
                }

                k = k + 1; // One more evaluation done

                double percent = k / kmax;

                callback(new PixelEnhancer.ProgressInfo(snew.swap, (int)percent));
            }

            return sbest; // Return the best solution found.
        }

        private static double Temperature(double k)
        {
            return k;
        }

        private NeiSolution Neighbour(Solution s)
        {
            var neighbour = new Color[Width][];

            var p1_X = r.Next(Width);
            var p1_Y = r.Next(Height);

            var p2_X = r.Next(Width);
            var p2_Y = r.Next(Height);

            Swap sw = new Swap(
                p1_X,
                p1_Y,
                p2_X,
                p2_Y
                );

            return new NeiSolution(s, sw);
        }

        

        private static double distance(Color c1, Color c2)
        {
            double r = Math.Abs(c1.R/255d - c2.R/255d);
            double g = Math.Abs(c1.G/255d - c2.G/255d);
            double b = Math.Abs(c1.B/255d - c2.B/255d);

            return Math.Pow(r, 2) +
                   Math.Pow(g, 2) +
                   Math.Pow(b, 2);
        }

        private double RandomDouble()
        {
            return r.NextDouble();
        }

        private static double P(double e, double enew, double T)
        {
            return enew < e ? 1 : Math.Exp(-(enew - e) / T);
        }
    }
}
