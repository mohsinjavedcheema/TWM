using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Twm.Chart;
using Twm.Chart.DrawObjects;
using Twm.Core.Classes;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Core.Market;
using Twm.Core.ViewModels;
using Twm.ViewModels.Assets;
using Twm.ViewModels.Orders;
using Twm.ViewModels.Positions;
using Application = System.Windows.Application;

namespace Twm.ViewModels.Accounts
{
    public class AccountViewModel : ViewModelBase
    {
        private readonly Account _account;

        private string[] _updatedProperties = {"Unrealized", "Realized", "TotalPnL", "IsActive"};

        public string Id
        {
            get { return Account.LocalId; }
        }

        public Account Account
        {
            get { return _account; }
        }

        public AccountType AccountType
        {
            get { return _account.AccountType; }
        }

        public string Name
        {
            get { return AccountType.Description(); }
        }

        public string ConnectionName
        {
            get { return Account.Connection.Name; }
        }

        public int ConnectionId
        {
            get { return Account.Connection.Id; }
        }


        public double StartingCapital
        {
            get { return _account.StartingCapital; }
            set
            {
                if (_account.StartingCapital != value)
                {
                    _account.StartingCapital = value;
                    OnPropertyChanged();
                }
            }
        }


        public bool IsActive
        {
            get { return _account.IsActive; }
        }

        public double? Realized
        {
            get { return _account.Realized; }
        }

        public double? Unrealized
        {
            get { return _account.Unrealized; }
        }

        public double? TotalPnl
        {
            get { return _account.TotalPnL; }
        }

        public AccountViewModel(Account account)
        {
            _account = account;
            _account.PropertyChanged += account_PropertyChanged;
            _account.OnPositionsChanged += OnPositionsChanged;
            _account.OnOrdersChanged += OnOrdersChanged;
            _account.OnAssetsChanged += OnAssetsChanged;
        }

        internal class OrderComparer : IComparer<OrderViewModel>
        {
            public int Compare(OrderViewModel p1, OrderViewModel p2)
            {
                if (p1.OrderState == OrderState.Working)
                    return 0;

                return 1;
            }
        }

        private async void OnOrdersChanged(object sender, OrderChangeEventArgs e)
        {
            if (e.ChangeAction == OrderChangeAction.Add)
            {
                foreach (var order in e.Orders)
                {
                    
                    var orderViewModel = new OrderViewModel(order);
                    orderViewModel.ConnectionId = ConnectionId;
                                   
                    Application.Current.Dispatcher.InvokeAsync(() => { App.Orders.Orders.AddSorted<OrderViewModel>(orderViewModel, new OrderComparer());
                        if (App.Orders.OrdersView.SortDescriptions.Count > 0)
                        {
                            ListSortDirection direction = ListSortDirection.Descending;
                            App.Orders.OrdersView.SortDescriptions.Clear();
                            App.Orders.OrdersView.SortDescriptions.Add(new SortDescription("OrderState", direction));
                        }
                        else
                            App.Orders.OrdersView.SortDescriptions.Add(new SortDescription("OrderState", ListSortDirection.Descending));
                    });


                    if (order.OrderType != OrderType.Market || order.OrderState != OrderState.Filled)
                    {
                        var chartsVm = App.Charts.Where(x =>
                                x.ChartTrader.Account != null && x.ChartTrader.Account.Connection.Id == ConnectionId)
                            .ToList();

                        foreach (var charTwm in chartsVm)
                        {
                            if (charTwm.DataCalcContext.CurrentDataSeriesParams.Instrument.Type == order.Instrument.Type &&
                                charTwm.DataCalcContext.CurrentDataSeriesParams.Instrument.Symbol == order.Instrument.Symbol
                                )
                            {
                                if (order.OrderType != OrderType.Market)
                                {
                                    if (order.OrderState == OrderState.Cancelled)
                                    {

                                    }

                                    charTwm.Chart.OrderDraws.Add(order.Guid,
                                        new OrderDraw()
                                        {
                                            Guid = order.Guid,
                                            OrderAction = order.OrderAction.ToString(),
                                            OrderType = order.OrderType.ToString(),
                                            StopPrice = order.StopPrice,
                                            LimitPrice = order.LimitPrice,
                                            Qnt = order.Quantity
                                        });
                                    charTwm.Chart.RefreshMainPane();
                                }
                            }
                        }
                    }
                }
            }
            else if (e.ChangeAction == OrderChangeAction.Update)
            {
                foreach (var order in e.Orders)
                {
                    if (order.OrderType != OrderType.Market)
                    {
                        var chartsVm = App.Charts.Where(x =>
                                x.ChartTrader.Account != null && x.ChartTrader.Account.Connection.Id == ConnectionId)
                            .ToList();
                        foreach (var charTwm in chartsVm)
                        {
                            if (charTwm.DataCalcContext.CurrentDataSeriesParams.Instrument.Type == order.Instrument.Type &&
                                 charTwm.DataCalcContext.CurrentDataSeriesParams.Instrument.Symbol == order.Instrument.Symbol)
                            {
                                if (charTwm.Chart.OrderDraws.TryGetValue(order.Guid, out var existingOrder))
                                {                                    

                                    if (order.OrderState == OrderState.Filled)
                                    {
                                        charTwm.Chart.RemoveOrder(order.Guid);
                                    }
                                    else if (order.OrderState == OrderState.Cancelled)
                                    {
                                        charTwm.Chart.RemoveOrder(order.Guid);
                                    }
                                    else
                                    {
                                        existingOrder.Qnt = order.Quantity;
                                        existingOrder.StopPrice = order.StopPrice;
                                        existingOrder.LimitPrice = order.LimitPrice;
                                        charTwm.Chart.RefreshMainPane();
                                    }
                                }
                            }
                        }
                    }
                }

                System.Windows.Application.Current.Dispatcher.InvokeAsync(() => {
              
                    if (App.Orders.OrdersView.SortDescriptions.Count > 0)
                    {
                        ListSortDirection direction = ListSortDirection.Descending;
                        App.Orders.OrdersView.SortDescriptions.Clear();
                        App.Orders.OrdersView.SortDescriptions.Add(new SortDescription("OrderState", direction));
                    }
                    else
                        App.Orders.OrdersView.SortDescriptions.Add(new SortDescription("OrderState", ListSortDirection.Descending));
                });
            }
            else if (e.ChangeAction == OrderChangeAction.Remove)
            {

                


                foreach (var order in e.Orders)
                {
                    var orderVm = App.Orders.Orders.FirstOrDefault(x => x.Order.Guid == order.Guid);
                    if (orderVm != null)
                        Application.Current.Dispatcher.InvokeAsync(() => { App.Orders.Orders.Remove(orderVm); });
                    if (order.OrderType != OrderType.Market)
                    {
                        var chartsVm = App.Charts.Where(x =>
                                x.ChartTrader.Account != null && x.ChartTrader.Account.Connection.Id == ConnectionId)
                            .ToList();
                        foreach (var charTwm in chartsVm)
                        {
                            if (
                                charTwm.DataCalcContext.CurrentDataSeriesParams.Instrument.Type == order.Instrument.Type &&
                                 charTwm.DataCalcContext.CurrentDataSeriesParams.Instrument.Symbol == order.Instrument.Symbol)
                            {
                                if (charTwm.Chart.OrderDraws.TryGetValue(order.Guid, out var existingOrder))
                                {

                                    if (order.OrderState == OrderState.Filled)
                                    {
                                        charTwm.Chart.RemoveOrder(order.Guid);
                                    }
                                    else if (order.OrderState == OrderState.Cancelled)
                                    {
                                        charTwm.Chart.RemoveOrder(order.Guid);
                                    }
                                    else
                                    {
                                        existingOrder.Qnt = order.Quantity;
                                        existingOrder.StopPrice = order.StopPrice;
                                        existingOrder.LimitPrice = order.LimitPrice;
                                        charTwm.Chart.RefreshMainPane();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnPositionsChanged(object sender, PositionChangeEventArgs e)
        {
            List<PositionViewModel> accountPositions = new List<PositionViewModel>();
            Application.Current.Dispatcher.Invoke(() =>
            {
                accountPositions = App.Positions.Positions
                    .Where(x => x.AccountName == Name && x.Connection == ConnectionName).ToList();
            });

            if (e.PositionChangeAction == PositionChangeAction.Add)
            {
                foreach (var position in e.Positions)
                {
                    var positionViewModel = new PositionViewModel(position);

                    if (double.TryParse(position.Instrument.PriceIncrements.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture,
                              out var tickSize))
                    {
                        positionViewModel.ValueFormat = "{}{0:0." + "".PadRight(tickSize.GetDecimalCount(), '0') + "}";
                    }
                    
                    positionViewModel.AccountName = Name;
                    positionViewModel.Account = this;
                    positionViewModel.Connection = ConnectionName;
                    positionViewModel.ConnectionId = ConnectionId;
                    Application.Current.Dispatcher.InvokeAsync(
                        () => { App.Positions.Positions.Add(positionViewModel); });
                }
            }

            if (e.PositionChangeAction == PositionChangeAction.Update)
            {
                //
                App.Positions.UpdateAccountPositions(Name, ConnectionId);
            }


            if (e.PositionChangeAction == PositionChangeAction.Remove)
            {
                var positionsForRemove = accountPositions
                    .Where(x => e.Positions.Exists(y => y.Instrument.Symbol == x.Instrument.Symbol)).ToList();
                foreach (var position in positionsForRemove)
                {
                    Application.Current.Dispatcher.InvokeAsync(() => { App.Positions.Positions.Remove(position); });

                    var chartWindows = App.Charts.Where(x =>
                        !x.IsVisible && x.DataCalcContext.CurrentDataSeriesParams.Instrument.Symbol ==
                        position.Instrument.Symbol).Select(x => x.ChartWindow).ToList();

                    for (var i = 0; i < chartWindows.Count; i++)
                    {
                        if (!chartWindows[i].IsSstrategyExists())
                            chartWindows[i].Destroy();
                    }
                }
            }
        }


        private void OnAssetsChanged(object sender, AssetChangeEventArgs e)
        {
            List<AssetViewModel> accountAssets = new List<AssetViewModel>();
            Application.Current.Dispatcher.Invoke(() =>
            {
                accountAssets = App.Assets.Assets
                    .Where(x => x.Connection == ConnectionName).ToList();
            });

            if (e.AssetChangeAction == AssetChangeAction.Add)
            {
                foreach (var asset in e.Assets)
                {
                    var assetViewModel = new AssetViewModel(asset);
                    assetViewModel.Connection = ConnectionName;
                    assetViewModel.ConnectionId = ConnectionId;
                    Application.Current.Dispatcher.InvokeAsync(
                        () => { App.Assets.Assets.Add(assetViewModel); });
                }
            }

            if (e.AssetChangeAction == AssetChangeAction.Update)
            {
                //
                //App.Positions.UpdateAccountPositions(Name, ConnectionId);
            }


            if (e.AssetChangeAction == AssetChangeAction.Remove)
            {
                var assetsForRemove = accountAssets
                    .Where(x => e.Assets.Exists(y => y.AssetName == x.AssetName )).ToList();
                foreach (var asset in assetsForRemove)
                {
                    Application.Current.Dispatcher.InvokeAsync(() => { App.Assets.Assets.Remove(asset); });

                    
                }
            }
        }

        private void account_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_updatedProperties.Contains(e.PropertyName))
            {
                OnPropertyChanged(e.PropertyName);
            }
        }
    }
}