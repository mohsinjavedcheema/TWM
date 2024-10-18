using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using Twm.Core.Annotations;
using Twm.Core.Classes;
using Twm.Core.DataProviders.All;
using Twm.Core.DataProviders.Binance;
using Twm.Core.DataProviders.Common;


namespace Twm.ViewModels.Assets
{
    public class AssetsViewModel : DependencyObject, INotifyPropertyChanged
    {
        public ObservableCollection<AssetViewModel> Assets
        {
            get { return (ObservableCollection<AssetViewModel>)GetValue(AssetsProperty); }
            set { SetValue(AssetsProperty, value); }
        }

        public static readonly DependencyProperty AssetsProperty =
            DependencyProperty.Register("Assets", typeof(ObservableCollection<AssetViewModel>),
                typeof(AssetsViewModel),
                new UIPropertyMetadata(null));


        private AssetViewModel _selectedAsset;

        public AssetViewModel SelectedAsset
        {
            get { return _selectedAsset; }
            set
            {
                if (value != _selectedAsset)
                {
                    _selectedAsset = value;
                    OnPropertyChanged();
                }
            }
        }



        private ICollectionView _assetsView;

        public ICollectionView AssetsView
        {
            get { return _assetsView; }
            set
            {
                if (_assetsView != value)
                {
                    _assetsView = value;
                    OnPropertyChanged();
                }
            }
        }


        public ObservableCollection<ConnectionBase> Connections
        {
            get { return (ObservableCollection<ConnectionBase>)GetValue(ConnectionsProperty); }
            set { SetValue(ConnectionsProperty, value); }
        }

        public static readonly DependencyProperty ConnectionsProperty =
            DependencyProperty.Register("Connections", typeof(ObservableCollection<ConnectionBase>),
                typeof(AssetsViewModel),
                new UIPropertyMetadata(null));



        private ConnectionBase _selectedConnection;
        public ConnectionBase SelectedConnection
        {
            get { return _selectedConnection; }
            set
            {
                if (value != _selectedConnection)
                {
                    _selectedConnection = value;
                    OnPropertyChanged();
                    AssetsView.Refresh();
                }
            }
        }




        private readonly object _logLock = new object();

        public AssetsViewModel()
        {
            Assets = new ObservableCollection<AssetViewModel>();
            Assets.CollectionChanged += Assets_CollectionChanged;
            AssetsView = CollectionViewSource.GetDefaultView(Assets);
            AssetsView.Filter += AssetsViewFilter;
            Session.Instance.OnConnectionsUpdated += Instance_OnConnectionsUpdated;
            BindingOperations.EnableCollectionSynchronization(Assets, _logLock);
        }

        private void Instance_OnConnectionsUpdated(object sender, System.EventArgs e)
        {
            FillConnections();
        }

        public void FillConnections()
        {
            var selectedConnectionId = 0;
            if (SelectedConnection != null)
                selectedConnectionId = SelectedConnection.Id;

            Connections = new ObservableCollection<ConnectionBase>();
            Connections.Add(new All());

            var orderedConnections = Session.Instance.ConfiguredConnections.Where(x => x.Id > 0).OrderBy(x => x.Id);
            foreach (var connection in orderedConnections)
            {
                Connections.Add(connection);
            }

            if (selectedConnectionId == 0)
            {
                var currentConnection = Session.Instance.Connections.FirstOrDefault(x => x.Value.IsConnected);

                if (currentConnection.Value != null)
                {
                    SelectedConnection = Connections.FirstOrDefault(x => x.Id == currentConnection.Key);
                }
                else
                {
                    SelectedConnection = Connections.FirstOrDefault();
                }
            }
            else
            {
                SelectedConnection = Connections.FirstOrDefault(x => x.Id == selectedConnectionId);
            }
        }
        private bool AssetsViewFilter(object item)
        {
            var asset = item as AssetViewModel;
            if (asset == null)
                return false;

            if ((SelectedConnection == null || (asset.ConnectionId != SelectedConnection.Id && SelectedConnection.Name != "ALL")))
            {
                return false;
            }

            return true;
        }


        private async void Assets_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            /*if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var assets = e.NewItems.OfType<AssetViewModel>();

                foreach (var assetVm in assets)
                {
                    var chartsVm = App.Charts.Where(x =>
                            x.ChartTrader.Account != null &&
                            x.ChartTrader.Account.Connection.Id == asseTwm.ConnectionId)
                        .ToList();

                    foreach (var charTwm in chartsVm)
                    {
                        if (
                            charTwm.DataCalcContext.CurrentDataSeriesParams.Instrument.Type == asseTwm.Instrument.Type)
                            charTwm.Chart.AssetDraw = new AssetDraw()
                            { Pnl = asseTwm.Pnl, Price = asseTwm.AverageEntryPrice, Qnt = asseTwm.Quantity };
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var assets = e.OldItems.OfType<AssetViewModel>();
                foreach (var assetVm in assets)
                {
                    var chartsVm = App.Charts.Where(x =>
                        x.ChartTrader.Account != null &&
                        x.ChartTrader.Account.Connection.Id == asseTwm.ConnectionId).ToList();
                    foreach (var charTwm in chartsVm)
                    {
                        if (
                            charTwm.DataCalcContext.CurrentDataSeriesParams.Instrument.Type == asseTwm.Instrument.Type)
                            charTwm.Chart.AssetDraw = null;
                    }
                }
            }*/
        }

        public async void UpdateAccountAssets(string accountName, int connectionId)
        {
            /*var accountAssets = Assets.Where(x => x.AccountName == accountName && x.ConnectionId == connectionId);
            foreach (var assetVm in accountAssets)
            {
                var chartsVm = App.Charts.Where(x =>
                    x.ChartTrader.Account != null &&
                    x.ChartTrader.Account.Connection.Id == asseTwm.ConnectionId).ToList();
                foreach (var charTwm in chartsVm)
                {
                    if (charTwm.DataCalcContext.CurrentDataSeriesParams != null &&
                        charTwm.DataCalcContext.CurrentDataSeriesParams.Instrument.Type == asseTwm.Instrument.Type)
                    {
                        if (charTwm.Chart.AssetDraw == null)
                            charTwm.Chart.AssetDraw = new AssetDraw();
                        charTwm.Chart.AssetDraw.Pnl = asseTwm.Pnl;
                        charTwm.Chart.AssetDraw.Price = asseTwm.AverageEntryPrice;
                        charTwm.Chart.AssetDraw.Qnt = asseTwm.Quantity;
                    }
                }
            }*/
        }

       
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}