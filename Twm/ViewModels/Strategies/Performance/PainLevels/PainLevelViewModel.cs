using System;
using Twm.Core.DataCalc.Performance;
using Twm.Core.Enums;
using Twm.Core.Interfaces;
using Twm.Core.ViewModels;

namespace Twm.ViewModels.Strategies.Performance.RiskLevels
{
    public class RiskLevelViewModel:ViewModelBase, IRiskLevel, ICloneable
    {
        private readonly AnalyticItem _analyticItem;


        public AnalyticalFeature Feature { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value != _isChecked)
                {
                    _isChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get { return _analyticItem.DisplayName.ToString(); }
        }

        private double _value;
        public double Value
        {
            get { return _value; }
            set
            {
                if (value != _value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isPercent;
        public bool IsPercent
        {
            get { return _isPercent; }
            set
            {
                if (value != _isPercent)
                {
                    _isPercent = value;
                    OnPropertyChanged();
                }
            }
        }
        

        private double? _iSValue;
        public double? ISValue
        {
            get { return _iSValue; }
            set
            {
                if (value != _iSValue)
                {
                    _iSValue = value;
                    OnPropertyChanged();
                }
            }
        }


        private double? _oSValue;
        public double? OSValue
        {
            get { return _oSValue; }
            set
            {
                if (value != _oSValue)
                {
                    _oSValue = value;
                    OnPropertyChanged();
                }
            }
        }


        private double? _simValue;
        public double? SimValue
        {
            get { return _simValue; }
            set
            {
                if (value != _simValue)
                {
                    _simValue = value;
                    OnPropertyChanged();
                }
            }
        }



        private double _globalValue;
        public double GlobalValue
        {
            get { return _globalValue; }
            set
            {
                if (value != _globalValue)
                {
                    _globalValue = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isGlobalPercent;
        public bool IsGlobalPercent
        {
            get { return _isGlobalPercent; }
            set
            {
                if (value != _isGlobalPercent)
                {
                    _isGlobalPercent = value;
                    OnPropertyChanged();
                }
            }
        }



        private double _painValue;
        public double PainValue
        {
            get { return _painValue; }
            set
            {
                if (value != _painValue)
                {
                    _painValue = value;
                    OnPropertyChanged();
                }
            }
        }





        private bool _isFalse;
        public bool IsFalse
        {
            get { return _isFalse; }
            set
            {
                if (_isFalse != value)
                {
                    _isFalse = value;
                    OnPropertyChanged();
                }
            }

        }

        public RiskLevelViewModel(AnalyticalFeature type)
        {
            Feature = type;
            _analyticItem = new AnalyticItem(type);
        }



        public void SetPainValue(Summary summary)
        {
            ISValue = summary.GetValue<double>(_analyticItem.Feature);
            
            if (IsPercent)
            {
                var isValue = Math.Abs(ISValue??0);
                PainValue = (isValue/100)*Value;
            }
            else
            {
                PainValue = Value;
            }
        }

        public void SetGlobalPainValue(double value)
        {
            if (IsGlobalPercent)
            {
                PainValue = (Math.Abs(value) / 100) * GlobalValue;
            }
            else
            {
                PainValue = GlobalValue;
            }
        }


        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}