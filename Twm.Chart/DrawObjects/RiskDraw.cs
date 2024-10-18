using System;
using System.ComponentModel;
using System.Windows.Media;


namespace Twm.Chart.DrawObjects
{
    public class RiskDraw : BaseLineDraw, ICloneable
    {

        [DisplayName("Color")]
        public Color BrushColor
        {
            get { return ((SolidColorBrush)_brush).Color; }
            set
            {
                Brush = (Brush)(new SolidColorBrush(value)).GetCurrentValueAsFrozen();
                OnPropertyChanged();
                OnPropertyChanged("Brush");
            }
        }


        private Brush _brush;
        [Browsable(false)]
        public Brush Brush
        {
            get { return _brush; }
            set
            {
                if (_brush != value)
                {
                    _brush = value;
                    OnPropertyChanged();
                    OnPropertyChanged("BrushColor");
                }
            }
        }

        [Browsable(false)]
        public DashStyle DashStyle { get; set; }

        private int _width;
        public int Width
        {
            get { return _width; }
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged();
                }
            }
        }

        [Browsable(false)]
        public double RewardY { get; set; }

        [Browsable(false)]
        public int RewardBarIndex { get; set; }


        

        private double _ration;
        public double Ratio
        {
            get { return _ration; }
            set
            {
                if (_ration != value)
                {
                    _ration = value;
                    OnPropertyChanged();
                }
            }
        }


        public void ApplyRatio()
        {
            var delta = (EndY - StartY);
            RewardY = StartY - delta * Ratio;
        }

        public void ApplyRatioInverse()
        {
            var delta = (StartY - RewardY);
            EndY = StartY + delta / Ratio;
        }

       



    }
}