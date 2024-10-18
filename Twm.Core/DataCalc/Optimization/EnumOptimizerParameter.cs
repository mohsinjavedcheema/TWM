using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Markup;
using Newtonsoft.Json;

namespace Twm.Core.DataCalc.Optimization
{
    [DataContract]
    public class EnumOptimizerParameter : OptimizerParameter
    {
        private bool[] _values;

        [DataMember]
        public bool[] Values
        {
            get { return _values; }
            set
            {
                if (_values != value)
                {
                    _values = value;
                    
                    OnPropertyChanged();
                }
            }
        }


        private List<object> _valueList;

        private int _position;

        private Array _enumValues;

        public override object CurrentValue
        {
            get { return _valueList[_position]; }
            set { _valueList[_position] = value; }

        }

        public bool IsUncheckEnabled
        {
            get { return Values.Count(x => x) > 1; }
        }


        private object _defaultValue;

        public override object DefaultValue
        {
            get { return _defaultValue; }
        }

        public override long CombinationCount
        {
            get { return Values.Count(x => x); }
        }

        public EnumOptimizerParameter(string name, Type type, object defaultValue) : base(name, type)
        {
            _defaultValue = defaultValue;
            SetValues();
            _position = -1;
            TypeNum = 3;
        }

        [JsonConstructor]
        public EnumOptimizerParameter(string name, object defaultValue) : base(name)
        {
            _defaultValue = defaultValue;
            _position = -1;
            TypeNum = 3;
        }

        public void SetValues()
        {
            var names = Enum.GetNames(Type);
            _enumValues = Enum.GetValues(Type);
            Values = new bool[names.Length];
            if (!Values.Any(x => x))
                Values[0] = true;
        }

        public override void ResetValue()
        {

            _valueList = new List<object>();
            var index = 0;
            foreach (var boolValue in _values)
            {
                if (boolValue)
                {
                    _valueList.Add(_enumValues.GetValue(index));
                }

                index++;
            }

            if (_valueList != null && _valueList.Count > 0)
            {
                _position = 0;
            }
            else
            {
                _position = -1;
            }
        }

        public override bool NextVal()
        {
            if (_valueList == null || _valueList.Count == 0)
                return false;
            
            _position++;
            if (_position == _valueList.Count)
                return false;


            return true;
        }


        public override void CopyPreset(OptimizerParameter parameter)
        {
            if (parameter is EnumOptimizerParameter enumOptimizerParameter)
            {
                for (int i = 0; i< enumOptimizerParameter.Values.Length; i++)
                {
                    if (i > Values.Length - 1)
                        break;

                    enumOptimizerParameter.Values[i] = Values[i];
                }

                if (!Values.Any(x => x))
                    enumOptimizerParameter.Values[0] = true;
            }
        }

    }
}