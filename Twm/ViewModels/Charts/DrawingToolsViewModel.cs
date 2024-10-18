using System;
using System.Windows.Media;
using Twm.Core.Classes;
using Twm.Core.DataCalc;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.ViewModels;
using Twm.Model.Model;

namespace Twm.ViewModels.Charts
{
    public class DrawingToolsViewModel : ViewModelBase
    {


        public Draw Draw { get; set; }

        private IConnection _connection;

        public IConnection Connection
        {
            get { return _connection; }
            set
            {
                if (_connection != value)
                {
                    _connection = value;
                    OnPropertyChanged();
                }
            }
        }

       

        private Chart.Classes.Chart _chart;
        public Chart.Classes.Chart Chart
        {
            get { return _chart; }
            set
            {
                if (_chart != value)
                {
                    _chart = value;
                    OnPropertyChanged();
                }
            }
        }


        private Color _color;

        public Color Color
        {
            get { return _color; }
            set
            {
                if (_color != value)
                {
                    _color = value;
                    Chart.DrawingColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _width;
        public int Width
        {
            get { return _width; }
            set
            {
                if (_width != value)
                {
                    _width = value;
                    Chart.DrawingWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _size;
        public int Size
        {
            get { return _size; }
            set
            {
                if (_size != value)
                {
                    _size = value;
                    Chart.DrawingSize = value;
                    OnPropertyChanged();
                }
            }
        }



        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    Chart.DrawingText = value;
                    OnPropertyChanged();
                }
            }
        }



        public void Close()
        {
          

        }


        private Instrument _instrument;
        public Instrument Instrument
        {
            get { return _instrument; }
            set
            {
                if (_instrument != value)
                {
                    _instrument = value;
                    OnPropertyChanged();
                }
            }
        }

      

        public OperationCommand ClearCommand { get; set; }

        public OperationCommand DeleteCommand { get; set; }

        public DrawingToolsViewModel()
        {
            Draw = new Draw();
            ClearCommand = new OperationCommand(Clear);
            

        }


        private void Clear(object parameter)
        {
            Chart.ClearDrawingObjects();
        }
        

    }
}