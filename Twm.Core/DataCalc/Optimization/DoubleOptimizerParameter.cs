using System;
using System.Runtime.Serialization;

namespace Twm.Core.DataCalc.Optimization
{
    [DataContract]
    public class DoubleOptimizerParameter:OptimizerParameter
    {

        private double _min;
        [DataMember]
        public double Min
        {
            get { return _min; }
            set
            {
                if (_min != value)
                {
                    _min = value;
                    OnPropertyChanged();
                    OnPropertyChanged("CombinationCount");
                }
            }
        }

        private double _max;
        [DataMember]
        public double Max
        {
            get { return _max; }
            set
            {
                if (_max != value)
                {
                    _max = value;
                    OnPropertyChanged();
                    OnPropertyChanged("CombinationCount");
                }
            }
        }

        private double _inc;
        [DataMember]
        public double Inc
        {
            get { return _inc; }
            set
            {
                if (_inc != value)
                {
                    _inc = value;
                    OnPropertyChanged();
                    OnPropertyChanged("CombinationCount");
                }
            }
        }

        private double _currentValue;
        public override object CurrentValue
        {
            get { return _currentValue; }
            set { _currentValue = (double)value; }
        }


        private double _defaultValue;

        public override object DefaultValue
        {
            get { return _defaultValue; }
        }

        public override long CombinationCount
        {
            get
            {
                if (Inc > 0 && Max >= Min)
                {
                    return (long)((Max - Min) / Inc + 1);
                }

                return 1;
            }

        }


        public DoubleOptimizerParameter(string name, Type type, double defaultValue) : base(name, type)
        {
            _defaultValue = defaultValue;
            Min = defaultValue;
            Max = defaultValue;
            Inc = 1;
            TypeNum = 1;
        }

        public override void ResetValue()
        {
            _currentValue = Min;
        }

        public override bool NextVal()
        {
            _currentValue += Inc;

            if (_currentValue > Max)
            {
                return false;
            }

            return true;
        }

      

        public override void CopyPreset(OptimizerParameter parameter)
        {
            if (parameter is DoubleOptimizerParameter doubleOptimizerParameter)
            {
                doubleOptimizerParameter.Min = Min;
                doubleOptimizerParameter.Max = Max;
                doubleOptimizerParameter.Inc = Inc;
            }
        }

    }
}