using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;


namespace Twm.Chart.DrawObjects
{
    public class LineDraw : BaseLineDraw, ICloneable
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


        [DisplayName("DashStyle")]
        public string DashStyleString
        {
            get {

                if (DashStyle == System.Windows.Media.DashStyles.Solid)
                {
                    return "Solid";
                }
                if (DashStyle == System.Windows.Media.DashStyles.Dot)
                {
                    return "Dot";
                }
                if (DashStyle == System.Windows.Media.DashStyles.Dash)
                {
                    return "Dash";
                }
                return "Solid";
            }
            set
            {
                switch(value)
                {
                    case "Solid":
                        DashStyle = System.Windows.Media.DashStyles.Solid;
                        break;
                    case "Dot":
                        DashStyle = System.Windows.Media.DashStyles.Dot;
                        break;
                    case "Dash":
                        DashStyle = System.Windows.Media.DashStyles.Dash;
                        break;
                }
                OnPropertyChanged();
                OnPropertyChanged("DashStyle");
            }
        }


        private DashStyle _dashStyle;
        [Browsable(false)]
        public DashStyle DashStyle 
        {

            get { return _dashStyle; }
            set
            {
                if (_dashStyle != value)
                {
                    _dashStyle = value;
                    OnPropertyChanged();
                    OnPropertyChanged("BrushColor");
                }
            }
        }

        [Browsable(false)]
        public List<string> DashStyles { get; set; } = new List<string>() { "Solid", "Dash", "Dot" };


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

        public LineDraw()
        {
            DashStyle = System.Windows.Media.DashStyles.Solid;
        }

    }
}