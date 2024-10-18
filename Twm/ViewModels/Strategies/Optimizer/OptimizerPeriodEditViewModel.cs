using System;
using System.Windows;
using Twm.Core.ViewModels;

namespace Twm.ViewModels.Strategies.Optimizer
{
 
    public class OptimizerPeriodEditViewModel : ViewModelBase
    {
        public OptimizerPeriodViewModel Period { get; set; }

        private int _periodWidth;
        public int PeriodWidth
        {
            get { return _periodWidth; }
            set
            {
                if (_periodWidth != value)
                {
                    _periodWidth = value;
                    OnPropertyChanged();
                }
            }

        }

        private int _totalWidth;
        public int TotalWidth
        {
            get { return _totalWidth; }
            set
            {
                if (_periodWidth != value)
                {
                    _totalWidth = value;
                    OnPropertyChanged();
                }
            }

        }



        public bool IsLive
        {
            get { return Period.IsLive; }
            set
            {
                if (Period.IsLive != value)
                {
                    Period.IsLive = value;
                    OnPropertyChanged();
                }
            }

        }



        public OptimizerPeriodEditViewModel PrevPeriod { get; set; }
        public  OptimizerPeriodEditViewModel NextPeriod { get; set; }


        private GridLength _iSStartColumn;
        public GridLength ISStartColumn
        {
            get { return _iSStartColumn; }
            set
            {
                if (_iSStartColumn != value)
                {
                    _iSStartColumn = value;
                    OnPropertyChanged();
                }
            }
        }

        private GridLength _iSEndColumn;
        public GridLength ISEndColumn
        {
            get { return _iSEndColumn; }
            set
            {
                if (_iSEndColumn != value)
                {
                    _iSEndColumn = value;
                    OnPropertyChanged();
                }
            }
        }



        private GridLength _oSStartColumn;
        public GridLength OSStartColumn
        {
            get { return _oSStartColumn; }
            set
            {
                if (_oSStartColumn != value)
                {
                    _oSStartColumn = value;
                    OnPropertyChanged();
                }
            }
        }

        private GridLength _oSEndColumn;
        public GridLength OSEndColumn
        {
            get { return _oSEndColumn; }
            set
            {
                if (_oSEndColumn != value)
                {
                    _oSEndColumn = value;
                    OnPropertyChanged();
                }
            }
        }


        public int Number
        {
            get { return Period.Number; }
            set
            {
                if (value != Period.Number)
                {
                    Period.Number = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime ISStartDate
        {
            get { return Period.ISStartDate; }
            set
            {
                if (value != Period.ISStartDate)
                {
                    Period.ISStartDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged("ISDays");
                    OnPropertyChanged("ISPercent");
                    OnPropertyChanged("OSPercent");
                    OnPropertyChanged("TotalDays");
                    CalcColumnWidth();
                }
            }
        }

        public DateTime ISEndDate
        {
            get { return Period.ISEndDate; }
            set
            {
                if (value != Period.ISEndDate)
                {
                    Period.ISEndDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged("OSDays");
                    OnPropertyChanged("ISDays");
                    OnPropertyChanged("OSStartDate");
                    OnPropertyChanged("ISPercent");
                    OnPropertyChanged("OSPercent");
                    CalcColumnWidth();
                }
            }
        }


        public DateTime OSStartDate
        {
            get { return Period.OSStartDate; }
            set
            {
                if (value != Period.OSStartDate)
                {
                    Period.OSStartDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged("OSDays");
                    OnPropertyChanged("ISDays");
                    OnPropertyChanged("ISEndDate");
                    OnPropertyChanged("ISPercent");
                    OnPropertyChanged("OSPercent");
                    CalcColumnWidth();
                }
            }
        }

        public DateTime OSEndDate
        {
            get { return Period.OSEndDate; }
            set
            {
                if (value != Period.OSEndDate)
                {
                    Period.OSEndDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged("OSDays");
                    OnPropertyChanged("DisplayName");
                    OnPropertyChanged("ISPercent");
                    OnPropertyChanged("OSPercent");
                    OnPropertyChanged("TotalDays");
                    CalcColumnWidth();
                }
            }
        }


        public int ISDays
        {
            get { return Period.ISDays; }
            set
            {
                if (value != Period.ISDays)
                {
                    Period.ISDays = value;
                    OnPropertyChanged();
                    OnPropertyChanged("ISStartDate");
                    OnPropertyChanged("ISPercent");
                    OnPropertyChanged("OSPercent");
                    OnPropertyChanged("TotalDays");
                    CalcColumnWidth();
                }
            }
        }

   
        public int OSDays
        {
            get { return Period.OSDays; }
            set
            {
                if (value != Period.OSDays)
                {
                    Period.OSDays = value;
                    OnPropertyChanged();
                    OnPropertyChanged("ISEndDate");
                    OnPropertyChanged("OSStartDate");
                    OnPropertyChanged("ISDays");
                    OnPropertyChanged("ISPercent");
                    OnPropertyChanged("OSPercent");
                    CalcColumnWidth();
                }
            }
        }



        public int ISPercent
        {
            get { return Period.ISPercent; }
            set
            {
                if (value != Period.ISPercent)
                {
                    Period.ISPercent = value;
                    OnPropertyChanged();
                    OnPropertyChanged("OSPercent");
                    OnPropertyChanged("OSDays");
                    OnPropertyChanged("ISDays");
                    OnPropertyChanged("ISEndDate");
                    OnPropertyChanged("OSStartDate");
                    CalcColumnWidth();
                }
            }
        }


        public int OSPercent
        {
            get { return Period.OSPercent; }
            set
            {
                if (value != Period.OSPercent)
                {
                    Period.OSPercent = value;
                    OnPropertyChanged();
                    OnPropertyChanged("ISPercent");
                    OnPropertyChanged("OSDays");
                    OnPropertyChanged("ISDays");
                    OnPropertyChanged("ISEndDate");
                    OnPropertyChanged("OSStartDate");
                    CalcColumnWidth();
                }
            }
        }


        public int TotalDays
        {
            get { return Period.TotalDays; }
            set
            {
                if (value != Period.TotalDays)
                {
                    Period.TotalDays = value;
                    OnPropertyChanged();
                    OnPropertyChanged();
                    OnPropertyChanged("ISDays");
                    OnPropertyChanged("ISStartDate");
                    OnPropertyChanged("ISPercent");
                    OnPropertyChanged("OSPercent");
                    PeriodWidth = CalcPeriodWidth(value);
                    TotalWidth = PeriodWidth + 30 + 50;
                    CalcColumnWidth();
                }
            }
        }




        private int _isWidth;
        public int ISWidth
        {
            get { return _isWidth; }
            set
            {
                if (_isWidth != value)
                {
                    _isWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _osWidth;
        public int OSWidth
        {
            get { return _osWidth; }
            set
            {
                if (_osWidth != value)
                {
                    _osWidth = value;
                    OnPropertyChanged();
                }
            }
        }


        public Thickness RightOffset
        {
            get
            {
                if (NextPeriod == null)
                {
                    return  new Thickness(0);
                }


                var nextPeriod = NextPeriod;
                var offset = 0;
                while (nextPeriod != null)
                {
                    offset += nextPeriod.OSWidth;
                    nextPeriod = nextPeriod.NextPeriod;
                }

                if (PrevPeriod != null)

                {
                    PrevPeriod.UpdateProperty("RightOffset");
                }
                

                return new Thickness(0,0, offset, 0);


            }
        }

        
        public OptimizerPeriodEditViewModel(OptimizerPeriodViewModel optimizerPeriodViewModel)
        {
            Period = optimizerPeriodViewModel;
            var totalDays = (OSEndDate - ISStartDate).TotalDays;
            PeriodWidth = CalcPeriodWidth(totalDays);
            TotalWidth = PeriodWidth + 30 + 50;
            CalcColumnWidth();
        }

        private int CalcPeriodWidth(double totalDays)
        {
            double k = 1.0;


            if (totalDays <= 20)
            {
                k = 30;
            }
            else if (totalDays <= 40)
            {
                k = 16;
            }
            else if (totalDays <= 60)
            {
                k = 12;
            }
            else  if (totalDays <= 70)
            {
                k = 8;
            }
            else if (totalDays <= 80)
            {
                k = 7;
            }
            else if (totalDays <= 100)
            {
                k = 5;
            }
            else if (totalDays <= 180)
            {
                k = 4;
            }
            else if (totalDays <= 366)
            {
                k = 2.5;
            }
            else if (totalDays <= (366 * 2))
            {
                k = 1.5;
            }

            return (int) (totalDays * k);
        }

        private void CalcColumnWidth()
        {
            var totalDays = (OSEndDate - ISStartDate).TotalDays;

            var isDays = (ISEndDate - ISStartDate).TotalDays;

            ISWidth = (int) ((isDays * PeriodWidth) / totalDays);
            OSWidth = PeriodWidth - ISWidth;
            if (ISWidth < 100)
            {
                ISWidth = 100;
                OSWidth = PeriodWidth - ISWidth;
            }
            else if (OSWidth < 100)
            {
                OSWidth = 100;
                ISWidth = PeriodWidth - OSWidth;
            }


            ISStartColumn = new GridLength(ISWidth - 80, GridUnitType.Pixel);
            ISEndColumn = new GridLength(80, GridUnitType.Pixel);
            OSStartColumn = new GridLength(80, GridUnitType.Pixel);
            OSEndColumn = new GridLength(OSWidth - 80, GridUnitType.Pixel);

            if (PrevPeriod != null)
            {
                PrevPeriod.UpdateProperty("RightOffset");
            }

        }

    }
}