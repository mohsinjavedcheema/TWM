using System;
using System.ComponentModel;
using System.Windows.Media;


namespace Twm.Chart.DrawObjects
{
    public class RulerDraw : BaseLineDraw, ICloneable
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
        public double InfoY { get; set; }

        [Browsable(false)]
        public int InfoBarIndex { get; set; }


        [Browsable(false)]
        public bool IsRulerFixed { get; set; }

    }
}