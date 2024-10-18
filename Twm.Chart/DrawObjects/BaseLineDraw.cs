using System;
using System.ComponentModel;
using System.Windows.Media;


namespace Twm.Chart.DrawObjects
{
    public class BaseLineDraw : BaseDraw, ICloneable
    {
       

        [Browsable(false)]
        public double StartY { get; set; }

        [Browsable(false)]
        public double EndY { get; set; }

        [Browsable(false)]
        public int BarIndexStart { get; set; }

        [Browsable(false)]
        public int BarIndexEnd { get; set; }

        [Browsable(false)]
        public bool IsAutoScale { get; set; }

        [Browsable(false)]
        public string Owner { get; set; }

        [Browsable(false)]
        public bool IsAdjust { get; set; }

        [Browsable(false)]
        public bool AdjustStart { get; set; }

        [Browsable(false)]
        public bool AdjustEnd { get; set; }

        [Browsable(false)]
        public bool AdjustPoint { get; set; }

        [Browsable(false)]
        public bool IsHorizontal { get; set; }


        [Browsable(false)]
        public bool IsVertical { get; set; }


        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}