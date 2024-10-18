using Twm.Core.Market;
using Twm.Core.ViewModels;


namespace Twm.ViewModels.Assets
{
    public class AssetViewModel : ViewModelBase
    {
        private Asset _asset;


        
        public string AssetName
        {
            get { return _asset.AssetName; }            
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

        
        public double Balance
        {
            get { return _asset.Balance; }
            
        }

        //   public AccountViewModel Account { get; set; }





        private double _tickSize;
        public AssetViewModel(Asset asset)
        {
            _asset = asset;
            _asset.PropertyChanged += _asset_PropertyChanged;

            
        }

        private void _asset_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Balance")
            {
                OnPropertyChanged("Balance");
            }

        }

        
    }
}