using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace Twm.Chart.DrawObjects
{
    public class RectDraw : BaseLineDraw
    {

       

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


        [DisplayName("BorderColor")]
        public Color OutlineBrushColor
        {
            get { return ((SolidColorBrush)_outlineBrush).Color; }
            set
            {
                OutlineBrush = (Brush)(new SolidColorBrush(value)).GetCurrentValueAsFrozen();
                OnPropertyChanged();
                OnPropertyChanged("OutlineBrush");
            }
        }


        private Brush _outlineBrush;
        [Browsable(false)]
        public Brush OutlineBrush
        {
            get { return _outlineBrush; }
            set
            {
                if (_outlineBrush != value)
                {
                    _outlineBrush = value;
                    OnPropertyChanged();
                    OnPropertyChanged("OutlineBrushColor");
                }
            }
        }


        [DisplayName("Background")]
        public Color AreaBrushColor
        {
            get { return ((SolidColorBrush)_areaBrush).Color; }
            set
            {
                AreaBrush = (Brush)(new SolidColorBrush(value)).GetCurrentValueAsFrozen();
                OnPropertyChanged();
                OnPropertyChanged("AreaBrush");
            }
        }


        private Brush _areaBrush;
        [Browsable(false)]
        public Brush AreaBrush
        {
            get { return _areaBrush; }
            set
            {
                if (_areaBrush != value)
                {
                    _areaBrush = value;
                    OnPropertyChanged();
                    OnPropertyChanged("AreaBrushColor");
                }
            }
        }

        private double _areaOpacity;
        [DisplayName("Opacity")]
        public double AreaOpacity
        {
            get { return _areaOpacity; }
            set
            {
                if (_areaOpacity != value)
                {
                    _areaOpacity = value;
                    OnPropertyChanged();
                }
            }
        }

        [Browsable(false)]
        public bool YTop { get; set; }


        [DisplayName("DashStyle")]
        public string DashStyleString
        {
            get
            {

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
                switch (value)
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

        public RectDraw()
        {
            DashStyle = System.Windows.Media.DashStyles.Solid;
        }
    }
}