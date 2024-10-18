using Twm.Core.ViewModels;

namespace Twm.ViewModels.Strategies.Performance.RiskLevels
{
    public class SimSummaryItemViewModel:ViewModelBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
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
    }
}