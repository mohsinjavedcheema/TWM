using System;
using System.Runtime.Serialization;

namespace Twm.Core.DataCalc.Optimization
{
    [DataContract]
    public class IntegerOptimizerParameter:OptimizerParameter
    {

        private int _min;
        [DataMember]
        public int Min
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

        private int _max;
        [DataMember]
        public int Max
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

        private int _inc;
        [DataMember]
        public int Inc
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


        private int _currentValue;
        public override object CurrentValue
        {
            get { return _currentValue; }
            set { _currentValue = (int)value; }
        }



        private int _defaultValue;

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


        public IntegerOptimizerParameter(string name, Type type, int defaultValue) : base(name, type)
        {
            _defaultValue = defaultValue;
            Min = defaultValue;
            Max = defaultValue;
            Inc = 1;
            TypeNum = 2;
        }


        public override void ResetValue()
        {
            _currentValue = Min;
        }

        public override bool NextVal()
        {
            _currentValue = _currentValue +  Inc;

            if (_currentValue > Max)
            {
                return false;
            }

            return true;
        }

        public override void CopyPreset(OptimizerParameter parameter)
        {
            if (parameter is IntegerOptimizerParameter integerOptimizerParameter)
            {
                integerOptimizerParameter.Min = Min;
                integerOptimizerParameter.Max = Max;
                integerOptimizerParameter.Inc = Inc;
            }
        }
    }
}