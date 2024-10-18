using System.Collections.ObjectModel;
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

namespace Twm.ViewModels.Orders
{
    public class OrdersViewModel : DependencyObject, INotifyPropertyChanged
    {
        public ObservableCollection<OrderViewModel> Orders
        {
            get { return (ObservableCollection<OrderViewModel>) GetValue(OrdersProperty); }
            set { SetValue(OrdersProperty, value); }
        }

        public static readonly DependencyProperty OrdersProperty =
            DependencyProperty.Register("Orders", typeof(ObservableCollection<OrderViewModel>),
                typeof(OrdersViewModel),
                new UIPropertyMetadata(null));


        private ICollectionView _ordersView;

        public ICollectionView OrdersView
        {
            get { return _ordersView; }
            set
            {
                if (_ordersView != value)
                {
                    _ordersView = value;
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
                typeof(OrdersViewModel),
                new UIPropertyMetadata(null));



        private OrderViewModel _selectedOrder;
        public OrderViewModel SelectedOrder
        {
            get { return _selectedOrder; }
            set
            {
                if (value != _selectedOrder)
                {
                    _selectedOrder = value;
                    OnPropertyChanged();
                }
            }
        }


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
                    OrdersView.Refresh();
                    
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
                    OrdersView.Refresh();
                }
            }
        }

        public OperationCommand CancelCommand { get; set; }

        private readonly object _logLock = new object();

        public OrdersViewModel()
        {
            Orders = new ObservableCollection<OrderViewModel>();
            OrdersView = CollectionViewSource.GetDefaultView(Orders);
           
            OrdersView.Filter += OrdersViewFilter;
            Session.Instance.OnConnectionsUpdated += Instance_OnConnectionsUpdated;

            BindingOperations.EnableCollectionSynchronization(Orders, _logLock);

            CancelCommand = new OperationCommand(CancelOrder);

            TypesItems = new ObservableCollection<string>() { "ALL", "FUTURE", "SPOT" };
            _selectedType = "ALL";

         
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
        private bool OrdersViewFilter(object item)
        {
            var order = item as OrderViewModel;
            if (order == null)
                return false;



            if ((order.InstrumentType.ToUpper() != SelectedType.ToUpper())&& SelectedType.ToUpper()!="ALL")
            {
                return false;
            }

            if ((SelectedConnection == null || (order.ConnectionId != SelectedConnection.Id && SelectedConnection.Name != "ALL")) )
            {
                return false;
            }

           


            return true;
        }

        private void CancelOrder(object obj)
        {
            if (SelectedOrder.Order.OrderState == Core.Enums.OrderState.Working)
            {
                var account = Session.Instance.GetAccount(SelectedOrder.Order.Instrument.ConnectionId);
                account.CancelOrder(SelectedOrder.Order.Guid);
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