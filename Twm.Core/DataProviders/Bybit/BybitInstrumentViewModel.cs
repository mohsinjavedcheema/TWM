using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Twm.Chart.Annotations;
using Twm.Core.Classes;
using Twm.Core.DataProviders.Bybit.Enums;
using Twm.Core.DataProviders.Bybit.Models;
using Twm.Core.UI.Windows;


namespace Twm.Core.DataProviders.Bybit
{
    public class BybitInstrumentViewModel : INotifyPropertyChanged
    {

        private BybitInstrument _dataModel;
        public BybitInstrument DataModel
        {
            get { return _dataModel; }
            set
            {
                if (_dataModel != value)
                {
                    _dataModel = value;
                    OnPropertyChanged();
                }
            }
        }

        

        public object ViewObject { get; set; }

        public string Symbol
        {
            get { return DataModel.Symbol; }
            set
            {
                if (DataModel.Symbol != value)
                {
                    DataModel.Symbol = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Base
        {
            get { return DataModel.BaseAsset; }
            set
            {
                if (DataModel.BaseAsset != value)
                {
                    DataModel.BaseAsset = value;
                    OnPropertyChanged();
                }
            }
        }


        public string Quote
        {
            get { return DataModel.QuoteAsset; }
            set
            {
                if (DataModel.QuoteAsset != value)
                {
                    DataModel.QuoteAsset = value;
                    OnPropertyChanged();
                }
            }
        }


       

        public MarketType Type
        {
            get { return DataModel.Type; }
            set
            {
                if (DataModel.Type != value)
                {
                    DataModel.Type = value;
                    OnPropertyChanged();
                }
            }
        }


     

        public Filter[] Filters
        {
            get { return DataModel.Filters; }
            set
            {
                if (DataModel.Filters != value)
                {
                    DataModel.Filters = value;
                    OnPropertyChanged();
                }
            }
        }




        public double TickSize
        {
            get { return DataModel.TickSize; }
            set
            {
                if (DataModel.TickSize != value)
                {
                    DataModel.TickSize = value;
                    OnPropertyChanged();
                }
            }

        }


        public double MinLotSize
        {
            get { return DataModel.MinLotSize; }
            set
            {
                if (DataModel.MinLotSize != value)
                {
                    DataModel.MinLotSize = value;
                    OnPropertyChanged();
                }
            }

        }

        public double Notional
        {
            get { return DataModel.Notional; }
            set
            {
                if (DataModel.Notional != value)
                {
                    DataModel.Notional = value;
                    OnPropertyChanged();
                }
            }

        }


        private string _connectionName;
        public string ConnectionName
        {
            get { return _connectionName; }
            set
            {
                if (_connectionName != value)
                {
                    _connectionName = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                if (_isReadOnly != value)
                {
                    _isReadOnly = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _lastPrice;
        public double LastPrice
        {
            get { return _lastPrice; }
            set
            {
                if (_lastPrice != value)
                {
                    _lastPrice = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _volume;
        public double Vol24
        {
            get { return _volume; }
            set
            {
                if (_volume != value)
                {
                    _volume = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand ViewCommand { get; set; }


        public BybitInstrumentViewModel(BybitInstrument dataModel)
        {
            DataModel = dataModel;
            ViewCommand = new OperationCommand(View);
            ViewObject = DataModel;
            IsReadOnly = true;            
        }


        private async void View(object obj)
        {            
            var instrumentWindow = new InstrumentWindow(this);
            if (instrumentWindow.ShowDialog() == true)
            {

            }
        }        

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}