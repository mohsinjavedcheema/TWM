using System.Collections.Generic;
using System.Windows;
using Twm.Core.ViewModels;

namespace Twm.ViewModels.Strategies.Optimizer
{
    public class ParameterValuesViewModel:ViewModelBase
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }


        private Dictionary<int, object> _values;

        public Dictionary<int, object> Values
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

        private Dictionary<int, Visibility> _visibilities;

        public Dictionary<int, Visibility> Visibilities
        {
            get { return _visibilities; }
            set
            {
                if (_visibilities != value)
                {
                    _visibilities = value;
                    OnPropertyChanged();
                }
            }
        }

        public ParameterValuesViewModel()
        {
            Visibilities = new Dictionary<int, Visibility>();
            Values = new Dictionary<int, object>();

            Clear();
        }

        public void Clear()
        {
            Visibilities.Clear();
            Values.Clear();
            for (int i = 0; i < 30; i++)
            {
                Visibilities.Add(i, Visibility.Collapsed);
                Values.Add(i, null);
            }
        }

    }
}