using System;
using System.Runtime.Serialization;

namespace Twm.Core.DataCalc.Optimization
{
    [DataContract]
    public class BoolOptimizerParameter : OptimizerParameter
    {

        private bool _value;
        [DataMember]
        public bool Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isOptimize;
        [DataMember]
        public bool IsOptimize
        {
            get { return _isOptimize; }
            set
            {
                if (_isOptimize != value)
                {
                    _isOptimize = value;
                    OnPropertyChanged();
                    OnPropertyChanged("CombinationCount");
                }
            }
        }

        private bool _currentValue;
        public override object CurrentValue
        {
            get { return _currentValue; }
            set { _currentValue = (bool)value; }
        }


        private bool _defaultValue;

        public override object DefaultValue
        {
            get { return _defaultValue; }
        }

        public override long CombinationCount
        {
            get
            {
                if (!_isOptimize)
                    return 1;
                return 2;
            }

        }

        public BoolOptimizerParameter(string name, Type type, bool defaultValue) : base(name, type)
        {
            _defaultValue = defaultValue;
            TypeNum = 4;
        }
      

        public override void ResetValue()
        {
            if (_isOptimize)
                _currentValue = false;
            else
                _currentValue = Value;
        }

        public override bool NextVal()
        {
            if (_isOptimize)
            {

                if (_currentValue)
                    return false;

                _currentValue = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void CopyPreset(OptimizerParameter parameter)
        {
            if (parameter is BoolOptimizerParameter boolOptimizerParameter)
            {
                boolOptimizerParameter.IsOptimize = IsOptimize;
                boolOptimizerParameter.Value = Value;
            }
        }
    }
}