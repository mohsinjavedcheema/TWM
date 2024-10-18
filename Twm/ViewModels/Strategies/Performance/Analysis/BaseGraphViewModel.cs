using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twm.Core.Interfaces;
using Twm.Core.Market;
using Twm.Core.ViewModels;
using OxyPlot;
using OxyPlot.Axes;


namespace Twm.ViewModels.Strategies.Performance.Analysis
{
    public abstract class BaseGraphViewModel : ViewModelBase
    {
        private ViewResolvingPlotModel _model;

        public ViewResolvingPlotModel Model
        {
            get { return _model; }
            set
            {
                if (_model != value)
                {
                    _model = value;
                    OnPropertyChanged();
                }
            }
        }

        private PlotController _controller;

        public PlotController Controller
        {
            get { return _controller; }
            set
            {
                if (_controller != value)
                {
                    _controller = value;
                    OnPropertyChanged();
                }
            }
        }


        protected LinearAxis TradesAxis;

        protected DateTimeAxis TimeAxis;

        protected LinearAxis ValueAxis;


        

        public double MinValueX { get; set; }

        public double MaxValueX { get; set; }

        public double MinValueY { get; set; }

        public double MaxValueY { get; set; }

        public string XTitle { get; set; }

        public string YTitle { get; set; }

        public object Tag { get; set; }

        public long XStep { get; set; }


        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public Func<double, string> XFormatter { get; set; }




        public virtual List<ITrade> Data { get; set; }

        public AnalysisParameters Params { get; set; }

        public BaseGraphViewModel()
        {
            //Sets the controller to enable show tracker on mouse hover
            Controller = new PlotController();
            Controller.UnbindMouseDown(OxyMouseButton.Left);
            Controller.BindMouseEnter(PlotCommands.HoverSnapTrack);
            
        }


        public abstract void CreatePlotModel();
        public abstract Task<bool> LoadData();


        public void SetMaxMin(Axis axis, double min, double max)
        {
                        
                MaxValueY = max;
                MinValueY = min;
                if (axis != null)
                {
                    axis.Maximum = MaxValueY;
                    axis.AbsoluteMaximum = MaxValueY;

                    axis.Minimum = MinValueY;
                    axis.AbsoluteMinimum = MinValueY;
                }
        }


        protected LinearAxis CreateTradesAxis()
        {
            TradesAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                AbsoluteMinimum = 0,
                MajorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColors.Gray,
                IsZoomEnabled = false
            };
            return TradesAxis;
        }


        protected LinearAxis CreateValueAxis()
        {
            ValueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColors.Gray,
                IsZoomEnabled = false,
                
            };

            return ValueAxis;
        }


        protected DateTimeAxis CreateDateTimeAxis()
        {
            TimeAxis = new DateTimeAxis()
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColors.Gray,
                IsZoomEnabled = false
            };

            return TimeAxis;
        }

     
    }
}