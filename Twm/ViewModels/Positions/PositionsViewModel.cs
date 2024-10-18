using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Twm.Chart.DrawObjects;
using Twm.Core.Annotations;
using Twm.Core.Classes;
using Twm.Core.DataProviders.All;
using Twm.Core.DataProviders.Binance;
using Twm.Core.DataProviders.Common;



namespace Twm.ViewModels.Positions
{
    public class PositionsViewModel : DependencyObject, INotifyPropertyChanged
    {
        public ObservableCollection<PositionViewModel> Positions
        {
            get { return (ObservableCollection<PositionViewModel>) GetValue(PositionsProperty); }
            set { SetValue(PositionsProperty, value); }
        }

        public static readonly DependencyProperty PositionsProperty =
            DependencyProperty.Register("Positions", typeof(ObservableCollection<PositionViewModel>),
                typeof(PositionsViewModel),
                new UIPropertyMetadata(null));


        private PositionViewModel _selectedPosition;

        public PositionViewModel SelectedPosition
        {
            get { return _selectedPosition; }
            set
            {
                if (value != _selectedPosition)
                {
                    _selectedPosition = value;
                    OnPropertyChanged();
                }
            }
        }



        private ICollectionView _positionsView;

        public ICollectionView PositionsView
        {
            get { return _positionsView; }
            set
            {
                if (_positionsView != value)
                {
                    _positionsView = value;
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
                typeof(PositionsViewModel),
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
                    PositionsView.Refresh();
                }
            }
        }

        public ObservableCollection<string> TypesItems { get; set; }

        private string _selectedType;
        public string SelectedType
        {
            get { return _selectedType; }
            set
            {
                if (_selectedType != value)
                {
                    _selectedType = value;
                    OnPropertyChanged();
                    PositionsView.Refresh();
                }
            }
        }




        public ICommand CloseCommand { get; set; }

        private readonly object _logLock = new object();

        public PositionsViewModel()
        {
            Positions = new ObservableCollection<PositionViewModel>();
            Positions.CollectionChanged += Positions_CollectionChanged;

            Session.Instance.OnConnectionsUpdated += Instance_OnConnectionsUpdated;

           PositionsView = CollectionViewSource.GetDefaultView(Positions);

            PositionsView.Filter += PositionsViewFilter;

            BindingOperations.EnableCollectionSynchronization(Positions, _logLock);

            TypesItems = new ObservableCollection<string>() { "ALL", "FUTURE", "SPOT" };
            _selectedType = "ALL";



            CloseCommand = new OperationCommand(ClosePosition);
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
        private bool PositionsViewFilter(object item)
        {
            var position = item as PositionViewModel;
            if (position == null)
                return false;



            if ((position.Instrument.Type.ToUpper() != SelectedType.ToUpper()) && SelectedType.ToUpper() != "ALL")
            {
                return false;
            }

            if ((SelectedConnection == null || (position.ConnectionId != SelectedConnection.Id && SelectedConnection.Name != "ALL")))
            {
                return false;
            }




            return true;
        }


        private async void Positions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var positions = e.NewItems.OfType<PositionViewModel>();

                foreach (var positionVm in positions)
                {
                    var chartsVm = App.Charts.Where(x =>
                            x.ChartTrader.Account != null &&
                            x.ChartTrader.Account.Connection.Id == positionVm.ConnectionId)
                        .ToList();

                    foreach (var charTwm in chartsVm)
                    {
                        if (
                            charTwm.DataCalcContext.CurrentDataSeriesParams.Instrument.Type == positionVm.Instrument.Type &&
                            charTwm.DataCalcContext.CurrentDataSeriesParams.Instrument.Symbol == positionVm.Instrument.Symbol
                            )
                            charTwm.Chart.PositionDraw = new PositionDraw()
                                {Pnl = positionVm.Pnl, Price = positionVm.AverageEntryPrice, Qnt = positionVm.Quantity};
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var positions = e.OldItems.OfType<PositionViewModel>();
                foreach (var positionVm in positions)
                {
                    var chartsVm = App.Charts.Where(x =>
                        x.ChartTrader.Account != null &&
                        x.ChartTrader.Account.Connection.Id == positionVm.ConnectionId).ToList();
                    foreach (var charTwm in chartsVm)
                    {
                        if (
                            charTwm.DataCalcContext.CurrentDataSeriesParams.Instrument.Type == positionVm.Instrument.Type
                            && charTwm.DataCalcContext.CurrentDataSeriesParams.Instrument.Symbol == positionVm.Instrument.Symbol
                            )
                            charTwm.Chart.PositionDraw = null;
                    }
                }
            }
        }

        public async void UpdateAccountPositions(string accountName, int connectionId)
        {
            var accountPositions = Positions.Where(x => x.AccountName == accountName && x.ConnectionId == connectionId);
            foreach (var positionVm in accountPositions)
            {
                var chartsVm = App.Charts.Where(x =>
                    x.ChartTrader.Account != null &&
                    x.ChartTrader.Account.Connection.Id == positionVm.ConnectionId).ToList();
                foreach (var charTwm in chartsVm)
                {
                    if (charTwm.DataCalcContext.CurrentDataSeriesParams != null &&                       
                        charTwm.DataCalcContext.CurrentDataSeriesParams.Instrument.Type == positionVm.Instrument.Type &&
                        charTwm.DataCalcContext.CurrentDataSeriesParams.Instrument.Symbol == positionVm.Instrument.Symbol)
                    {
                        if (charTwm.Chart.PositionDraw == null)
                            charTwm.Chart.PositionDraw = new PositionDraw();
                        charTwm.Chart.PositionDraw.Pnl = positionVm.Pnl;
                        charTwm.Chart.PositionDraw.Price = positionVm.AverageEntryPrice;
                        charTwm.Chart.PositionDraw.Qnt = positionVm.Quantity;
                    }
                }
            }
        }

        private void ClosePosition(object parameter)
        {
            SelectedPosition.Account.Account.Close(SelectedPosition.Instrument);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}