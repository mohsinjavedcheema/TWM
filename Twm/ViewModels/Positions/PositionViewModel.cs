using System.Globalization;
using Twm.Chart;
using Twm.Core.Helpers;
using Twm.Core.Market;
using Twm.Core.ViewModels;
using Twm.Model.Model;
using Twm.ViewModels.Accounts;


namespace Twm.ViewModels.Positions
{
    public class PositionViewModel : ViewModelBase
    {
        private Position _position;


        public Position Position
        {
            get { return _position; }
            set
            {
                if (value != _position)
                {
                    _position = value;
                    OnPropertyChanged();
                    OnPropertyChanged("Instrument");
                    OnPropertyChanged("Type");
                    OnPropertyChanged("Name");
                    OnPropertyChanged("Quantity");
                    OnPropertyChanged("Pnl");
                    OnPropertyChanged("AverageEntryPrice");
                }
            }
        }

        /*public string Instrument
        {
            get { return _position.Instrument.Symbol; }
        }*/

        public Instrument Instrument
        {
            get { return _position.Instrument; }
        }


        public string Type
        {
            get { return _position.MarketPosition.Description(); }
        }

        public string Name
        {
            get { return _position.Name; }
        }

        public double Quantity
        {
            get { return _position.Quantity; }
        }

        public double? Pnl
        {
            get { return _position.Unrealized; }
        }

        public double AverageEntryPrice
        {
            get {                
                return _position.AverageEntryPrice; }
        }

        public string AverageEntryPriceStr
        {
            get
            {
                return _position.AverageEntryPrice.ToString("0." + "".PadRight(_tickSize.GetDecimalCount(), '0'));
            }
        }


        private string _accountName;
        public string AccountName
        {
            get { return _accountName; }
            set
            {
                if (_accountName!= value)
                {
                    _accountName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _connection;
        public string Connection
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

        private int _connectionId;
        public int ConnectionId
        {
            get { return _connectionId; }
            set
            {
                if (_connectionId != value)
                {
                    _connectionId = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _strategyName;
        public string StrategyName
        {
            get { return _strategyName; }
            set
            {
                if (_strategyName != value)
                {
                    _strategyName = value;
                    OnPropertyChanged();
                }
            }
        }

        public AccountViewModel Account { get; set; }


        

        private string _valueFormat;
        public string ValueFormat
        {
            get { return _valueFormat; }
            set
            {
                if (_valueFormat != value)
                {
                    _valueFormat = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _tickSize;
        public PositionViewModel(Position position)
        {
            _position = position;
            _position.PropertyChanged += account_PropertyChanged;

            if (double.TryParse(position.Instrument.PriceIncrements.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture,
                             out var tickSize))
            {
                _tickSize = tickSize;
            }
        }

        private void account_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Unrealized")
            {
                OnPropertyChanged("Pnl");
            }
            if (e.PropertyName == "Name")
            {
                OnPropertyChanged("Name");
            }

            if (e.PropertyName == "AverageEntryPrice")
            {
                OnPropertyChanged("AverageEntryPrice");
                OnPropertyChanged("AverageEntryPriceStr");
            }

        }

        
    }
}