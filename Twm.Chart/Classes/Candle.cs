using System;
using Twm.Chart.Interfaces;

namespace Twm.Chart.Classes
{
    public class Candle : ICandle
    {
        public DateTime t { get; set; }

        public DateTime ct { get; set; }

        public double O { get; set; }

        public double H { get; set; }

        public double L { get; set; }

        public double C { get; set; }

        public double V { get; set; }
        public bool IsClosed { get; set; }
        public bool IsFirstTickOfBar { get; set; }

        public bool IsAggVolume { get; set; }

        public Candle(DateTime t, double O, double H, double L, double C, double V, bool isClosed = true, bool isFirstTickOfBar = false, bool isAggVolume = false)
        {
            this.t = t;
            this.O = O;
            this.H = H;
            this.L = L;
            this.C = C;
            this.V = V;
            IsClosed = isClosed;
            IsFirstTickOfBar = isFirstTickOfBar;
            IsAggVolume = isAggVolume;
        }

        public Candle()
        {
        }

        public override string ToString()
        {
            return $"T:{t}; O:{O}; H:{H}; L:{L}; C:{C}; V:{V}";
        }
    }
}