using System.ComponentModel;
using System.Windows.Media;

namespace Twm.Chart.DrawObjects
{
    public class TextDraw:BaseDraw
    {
        [Browsable(false)]
        public string Id { get; set; }

        [Browsable(false)]        
        public double Y { get; set; }

        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged();
                }
            }
        }



        [DisplayName("TextColor")]
        public Color TextBrushColor
        {
            get { return ((SolidColorBrush)_textBrush).Color; }
            set
            {
                TextBrush = (Brush)(new SolidColorBrush(value)).GetCurrentValueAsFrozen();
                OnPropertyChanged();
                OnPropertyChanged("TextBrush");
            }
        }


        private Brush _textBrush;
        [Browsable(false)]
        public Brush TextBrush
        {
            get { return _textBrush; }
            set
            {
                if (_textBrush != value)
                {
                    _textBrush = value;
                    OnPropertyChanged();
                    OnPropertyChanged("TextBrushColor");
                }
            }
        }

        public string _fontName;
        public string FontName
        {
            get { return _fontName; }
            set
            {
                if (_fontName != value)
                {
                    _fontName = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _textSize;
        [DisplayName("FontSize")]
        public int TextSize
        {
            get { return _textSize; }
            set
            {
                if (_textSize != value)
                {
                    _textSize = value;
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
    }
}