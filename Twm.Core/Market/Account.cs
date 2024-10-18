using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Twm.Chart.Annotations;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.Core.DataCalc;
using Twm.Core.DataProviders.Binance.Models.Response;
using Twm.Core.DataProviders.Common;
using Twm.Core.DataProviders.Common.Request;
using Twm.Core.DataProviders.Common.Response;
using Twm.Core.DataProviders.ExchangeMockUp;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Model.Model;


namespace Twm.Core.Market
{
    public class Account : INotifyPropertyChanged
    {
        public string LocalId { get; set; }


        public AccountType AccountType { get; set; }
        public double StartingCapital { get; set; }

        private bool _isActive;

        public bool IsActive
        {
            get { return _isActive; }

            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                }
            }
        }


        private double? _unrealized;

        public double? Unrealized
        {
            get { return _unrealized; }

            set
            {
                if (_unrealized != value)
                {
                    _unrealized = value;
                    OnPropertyChanged();
                    UpdateTotal();
                }
            }
        }


        private double? _realized;

        public double? Realized
        {
            get { return _realized; }

            set
            {
                if (_realized != value)
                {
                    _realized = value;
                    OnPropertyChanged();
                    UpdateTotal();
                }
            }
        }

        private double? _totalPnl;

        public double? TotalPnL
        {
            get { return _totalPnl; }

            set
            {
                if (_totalPnl != value)
                {
                    _totalPnl = value;
                    OnPropertyChanged();
                    UpdateTotal();
                }
            }
        }


        public bool IsPlayback => AccountType == AccountType.Playback;

        public ConnectionBase Connection { get; set; }

        public ObservableCollection<StrategyBase> Strategies { get; set; }

        public List<Position> Positions { get; set; }

        public List<Asset> Assets { get; set; }

        public List<Order> Orders { get; set; }
        public EventHandler<PositionChangeEventArgs> OnPositionsChanged { get; set; }
        public EventHandler<AssetChangeEventArgs> OnAssetsChanged { get; set; }
        public EventHandler<OrderChangeEventArgs> OnOrdersChanged { get; set; }


        private ExchangeMockUp _exchangeMockUp;

        public Account()
        {
            Strategies = new ObservableCollection<StrategyBase>();
            Positions = new List<Position>();
            Orders = new List<Order>();
            Assets = new List<Asset>();
        }

        public async void Close(Instrument inst)
        {
            Position instPos = null;

            if (Connection.Id == inst.ConnectionId)
            {
                instPos = Positions.FirstOrDefault(i => i.Instrument.Id == inst.Id);
            }
            else
            {
                foreach (var position in Positions)
                {
                    if (position.Instrument.Type == inst.Type)
                    {
                        instPos = position;
                        break;
                    }
                }
            }

            if (instPos == null)
                return;

            if (instPos.MarketPosition == MarketPosition.Short)
            {
                SubmitOrder(OrderAction.BuyToCover, instPos.Quantity, "BuyCover Manual", inst, OrderType.Market, 0, 0, 0);

            }

            if (instPos.MarketPosition == MarketPosition.Long)
            {
                SubmitOrder(OrderAction.Sell, instPos.Quantity, "Sell Manual", inst, OrderType.Market, 0, 0, 0);
            }
        }

        public void Sell(double qnt, Instrument inst)
        {
            SubmitOrder(OrderAction.SellShort, qnt, "SellShort Manual", inst, OrderType.Market, 0, 0, 0);
        }

        public void SellStopMarket(double price, double triggerPrice, double qnt, Instrument inst)
        {
            SubmitOrder(OrderAction.Sell, qnt, "SellStopMarket Manual", inst, OrderType.StopMarket, 0, price, triggerPrice);
        }

        public void SellLimit(double price, double triggerPrice, double qnt, Instrument inst)
        {
            SubmitOrder(OrderAction.Sell, qnt, "SellLimit Manual", inst, OrderType.Limit, price, 0, triggerPrice);
        }


        public void SellStopLimit(double price, double triggerPrice, double qnt, Instrument inst)
        {
            SubmitOrder(OrderAction.Sell, qnt, "SellStopLimit Manual", inst, OrderType.StopLimit, price, price, triggerPrice);
        }



        public void Buy(double qnt, Instrument inst)
        {
            SubmitOrder(OrderAction.Buy, qnt, "Buy Manual", inst, OrderType.Market, 0, 0, 0);
        }

        public void BuyStopMarket(double price, double triggerPrice, double qnt, Instrument inst)
        {
            SubmitOrder(OrderAction.Buy, qnt, "BuyStopMarket Manual", inst, OrderType.StopMarket, 0, price, triggerPrice);
        }

        public void BuyLimit(double price, double triggerPrice, double qnt, Instrument inst)
        {
            SubmitOrder(OrderAction.Buy, qnt, "BuyLimit Manual", inst, OrderType.Limit, price, 0, triggerPrice);
        }

        public void BuyStopLimit(double price, double triggerPrice, double qnt, Instrument inst)
        {
            SubmitOrder(OrderAction.Buy, qnt, "BuyStopLimit Manual", inst, OrderType.StopLimit, price, price, triggerPrice);
        }


        public void CancelOrder(string orderGuid)
        {
            var order = Orders.FirstOrDefault(x => x.Guid == orderGuid);

            if (AccountType == AccountType.LocalPaper)
            {
                if (order != null)
                {
                    Session.Instance.GetMockup(Connection).CancelOrder(order);
                }
            }
            else if (AccountType == AccountType.ServerPaper || AccountType == AccountType.Broker)
            {
                if (order != null)
                {
                    DebugController.Print("ACCOUNT cancelling order " + order.Guid);
                    Connection.Client.CancelOrder(new CancelOrderRequest { Order = order });
                }
            }
        }

        public void ChangeOrderQuantity(string orderGuid, double quantity)
        {
            var order = Orders.FirstOrDefault(x => x.Guid == orderGuid);

            if (AccountType == AccountType.LocalPaper)
            {
                if (order != null)
                {
                    Session.Instance.GetMockup(Connection).ChangeOrderQuantity(order, quantity);
                }
            }
            else if (AccountType == AccountType.ServerPaper || AccountType == AccountType.Broker)
            {
                if (order != null)
                {
                    order.Quantity = quantity;
                    Connection.Client.ChangeOrder(new ChangeOrderRequest { Order = order });
                }
            }
        }

        public void ChangeOrderPrice(string orderGuid, double price)
        {
            var order = Orders.FirstOrDefault(x => x.Guid == orderGuid);

            if (AccountType == AccountType.LocalPaper)
            {
                if (order != null)
                {
                    Session.Instance.GetMockup(Connection).ChangeOrderPrice(order, price);
                }
            }
            else if (AccountType == AccountType.ServerPaper || AccountType == AccountType.Broker)
            {
                if (order != null)
                {
                    if (order.OrderType == OrderType.Limit)
                    {
                        order.LimitPrice = price;
                    }
                    if (order.OrderType == OrderType.StopMarket)
                    {
                        order.StopPrice = price;
                    }
                    if (order.OrderType == OrderType.StopLimit)
                    {
                        order.StopPrice = price;
                        order.LimitPrice = price;
                    }

                    Connection.Client.ChangeOrder(new ChangeOrderRequest { Order = order });
                }
            }

        }

        public Order SubmitOrder(OrderAction orderAction, double qnt, string signalName, Instrument inst,
            OrderType orderType, double limitPrice, double stopPrice, double triggerPrice)
        {
            var order = new Order()
            {
                Name = signalName,
                Quantity = qnt,
                OrderType = orderType,
                OrderAction = orderAction,
                OrderState = OrderState.Initialized,
                Instrument = inst,
                OrderInitDate = DateTime.Now,
                OrderEnvironment = OrderEnvironment.Realtime,
                Guid = Guid.NewGuid().ToString(),
                LimitPrice = limitPrice,
                StopPrice = stopPrice,
                TriggerPrice = triggerPrice
            };



            if (AccountType == AccountType.LocalPaper)
            {
                Session.Instance.GetMockup(Connection).SubmitOrder((Order)order.Clone());

            }
            else if (AccountType == AccountType.ServerPaper || AccountType == AccountType.Broker)
            {
                if (order.Instrument.ConnectionId != Connection.Id)
                {
                    //order.Instrument = await Session.Instance.GetMappedInstrument(inst, Connection.Id);
                }

                if (order.Instrument == null)
                    throw new Exception($"Instrument {inst.Symbol} not mapped for connection {Connection.Name}");

                DebugController.Print("Submitting account order " + order.Name +
                                      " Qnt:" + order.Quantity + " " +
                                      " Guid: " + order.Guid +
                                      " State: " + order.OrderState);

                Connection.Client.SubmitOrder(new SubmitOrderRequest() { Order = order });

            }

            return order;

        }


        public void InitConnection(ConnectionBase connection)
        {
            Connection = connection;
            if (AccountType == AccountType.LocalPaper)
            {
                //
            }
            else if (AccountType == AccountType.ServerPaper)
            {

                SubscribeServer(connection);
                Connection.Client.InitAccount();
                //Server paper trading
            }
            else if (AccountType == AccountType.Broker)
            {
                //Real server trading
                SubscribeServer(connection);
                Connection.Client.InitAccount();
            }
        }


        public void SubscribeServer(ConnectionBase connection)
        {
            if (AccountType == AccountType.LocalPaper)
            {
                _exchangeMockUp = Session.Instance.GetMockup(connection);
                if (_exchangeMockUp != null)
                {
                    _exchangeMockUp.OnPositionChanged += Account_OnPositionChanged;
                    _exchangeMockUp.OnAccountChanged += Account_OnAccountChanged;
                    _exchangeMockUp.OnOrderStatusChanged += _exchangeMockUp_OnOrderStatusChanged;
                }
            }
            else if (AccountType == AccountType.ServerPaper)
            {
                Connection.Client.OrderStatusChanged += serverPaperConnection_OnOrderStatusChanged;
                Connection.Client.PositionChanged += serverPaperConnection_OnPositionChanged;
                Connection.Client.AccountChanged += serverPaperConnection_OnAccountChanged;
                Connection.Client.AssetChanged += serverPaperConnection_OnAssetChanged;


            }
            else if (AccountType == AccountType.Broker)
            {
                //Real server trading
                Connection.Client.OrderStatusChanged += serverPaperConnection_OnOrderStatusChanged;
                Connection.Client.PositionChanged += serverPaperConnection_OnPositionChanged;
                Connection.Client.AccountChanged += serverPaperConnection_OnAccountChanged;
                Connection.Client.AssetChanged += serverPaperConnection_OnAssetChanged;
            }
        }

        public void UnsubscribeServer(ConnectionBase connection)
        {
            if (AccountType == AccountType.LocalPaper)
            {
                _exchangeMockUp = Session.Instance.GetMockup(connection);
                if (_exchangeMockUp != null)
                {
                    _exchangeMockUp.OnPositionChanged -= Account_OnPositionChanged;
                    _exchangeMockUp.OnAccountChanged -= Account_OnAccountChanged;
                    _exchangeMockUp.OnOrderStatusChanged -= _exchangeMockUp_OnOrderStatusChanged;
                }
            }
            else if (AccountType == AccountType.ServerPaper)
            {
                Connection.Client.OrderStatusChanged -= serverPaperConnection_OnOrderStatusChanged;
                Connection.Client.PositionChanged -= serverPaperConnection_OnPositionChanged;
                Connection.Client.AccountChanged -= serverPaperConnection_OnAccountChanged;

            }
            else if (AccountType == AccountType.Broker)
            {
                //Real server trading
                Connection.Client.OrderStatusChanged -= serverPaperConnection_OnOrderStatusChanged;
                Connection.Client.PositionChanged -= serverPaperConnection_OnPositionChanged;
                Connection.Client.AccountChanged -= serverPaperConnection_OnAccountChanged;
            }
        }


        private void serverPaperConnection_OnOrderStatusChanged(object sender, IResponse response)
        {
            if (response is OrderStatusChangedResponse orderResponse)
            {
                var order = Orders.FirstOrDefault(x => x.Guid == orderResponse.Order.Guid);
                if (order == null)
                {
                    order = new Order();
                    Orders.Add(order);
                    orderResponse.Order.CloneTo(order);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        OnOrdersChanged?.Invoke(Orders,
                        new OrderChangeEventArgs(new List<Order> { order }, OrderChangeAction.Add));
                    });

                    DebugController.Print("ACCOUNT ORDER UPDATE ORDER NOT FOUND " +
                                          " Name: " + order.Name +
                                          " Qnt: " + order.Quantity +
                                          " State: " + order.OrderState +
                                          " Guid: " + order.Guid);
                }
                else
                {
                    order.Sid = orderResponse.Order.Sid;
                    order.RealGuid = orderResponse.Order.RealGuid;
                    order.FillPrice = orderResponse.Order.FillPrice;
                    order.OrderFillDate = orderResponse.Order.OrderFillDate;
                    order.Quantity = orderResponse.Order.Quantity;
                    order.LimitPrice = orderResponse.Order.LimitPrice;
                    order.StopPrice = orderResponse.Order.StopPrice;
                    order.RealId = orderResponse.Order.RealId;
                    order.OrderState = orderResponse.Order.OrderState;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        OnOrdersChanged?.Invoke(Orders,
                        new OrderChangeEventArgs(new List<Order> { order }, OrderChangeAction.Update));
                    });

                    DebugController.Print("ACCOUNT ORDER UPDATE ORDER FOUND " +
                                          " Name: " + order.Name +
                                          " Qnt: " + order.Quantity +
                                          " State: " + order.OrderState +
                                          " Guid: " + order.Guid);
                }
            }
        }

        private void serverPaperConnection_OnPositionChanged(object sender, IResponse response)
        {
            if (response is PositionChangedResponse positionResponse)
            {
                var existPosition = Positions.FirstOrDefault(x => x.Instrument.Id == positionResponse.Position.Instrument.Id);
                if (existPosition != null)
                {
                    if (positionResponse.Position.Quantity == 0)
                    {
                        DebugController.Print("ACCOUNT POSITION REMOVE " +
                                              " Symbol: " + existPosition.Instrument.Symbol +
                                              " Qnt: " + existPosition.Quantity +
                                              " MarketPos: " + existPosition.MarketPosition);

                        Positions.Remove(existPosition);
                        OnPositionsChanged?.Invoke(Positions,
                                new PositionChangeEventArgs(new List<Position> { existPosition }, PositionChangeAction.Remove));
                        return;
                    }

                    //Update
                    existPosition.MarketPosition = positionResponse.Position.MarketPosition;
                    existPosition.Quantity = positionResponse.Position.Quantity;
                    if (positionResponse.Position.Unrealized != double.MaxValue &&
                        positionResponse.Position.Unrealized != double.MinValue)
                        existPosition.Unrealized = positionResponse.Position.Unrealized;
                    existPosition.AverageEntryPrice = positionResponse.Position.AverageEntryPrice;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        OnPositionsChanged?.Invoke(Positions,
                        new PositionChangeEventArgs(new List<Position> { existPosition }, PositionChangeAction.Update));
                    });

                    //DebugController.Print("ACCOUNT POSITION UPDATE " +
                    //                      " Symbol: " + existPosition.Instrument.Symbol +
                    //                      " Qnt: " + existPosition.Quantity +
                    //                      " MarketPos: " + existPosition.MarketPosition);

                }
                else
                {
                    if (positionResponse.Position.Instrument == null)
                        return;

                    if (positionResponse.Position.Quantity == 0)
                        return;

                    //Add
                    var pos = positionResponse.Position.CloneTo(new Position());
                    Positions.Add(pos);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        OnPositionsChanged?.Invoke(Positions,
                        new PositionChangeEventArgs(new List<Position> { pos }, PositionChangeAction.Add));
                    });

                    DebugController.Print("ACCOUNT POSITION ADD " +
                                          " Symbol: " + pos.Instrument.Symbol +
                                          " Qnt: " + pos.Quantity +
                                          " MarketPos: " + pos.MarketPosition);
                }
            }
        }


        private void serverPaperConnection_OnAssetChanged(object sender, IResponse response)
        {
            if (response is AssetChangedResponse assetResponse)
            {
                var existAsset = Assets.FirstOrDefault(x => x.AssetName == assetResponse.Asset.AssetName);
                if (existAsset != null)
                {
                    if (assetResponse.Asset.Balance == 0)
                    {
                        DebugController.Print("ACCOUNT ASSET REMOVE " +
                                              " Coin: " + existAsset.AssetName);

                        Assets.Remove(existAsset);
                        OnAssetsChanged?.Invoke(Assets,
                                new AssetChangeEventArgs(new List<Asset> { existAsset }, AssetChangeAction.Remove));
                        return;
                    }

                    //Update
                    existAsset.Balance = assetResponse.Asset.Balance;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        OnAssetsChanged?.Invoke(Assets,
                        new AssetChangeEventArgs(new List<Asset> { existAsset }, AssetChangeAction.Update));
                    });


                }
                else
                {
                    if (assetResponse.Asset.Balance == 0)
                        return;

                    //Add
                    var ass = assetResponse.Asset.CloneTo(new Asset());
                    Assets.Add(ass);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        OnAssetsChanged?.Invoke(Assets,
                        new AssetChangeEventArgs(new List<Asset> { ass }, AssetChangeAction.Add));
                    });

                    DebugController.Print("ACCOUNT ASSET ADD " +
                                          " Coin: " + ass.AssetName
                                        );
                }
            }
        }

        private void serverPaperConnection_OnAccountChanged(object sender, IResponse response)
        {
            if (response is AccountChangedResponse accountResponse)
            {
                if (accountResponse.Realized != null)
                    Realized = accountResponse.Realized;

                if (accountResponse.Unrealized != null)
                    Unrealized = accountResponse.Unrealized;
            }

        }

        private void _exchangeMockUp_OnOrderStatusChanged(object sender, ExchangeOrderStatusEventArgs e)
        {
            var order = Orders.FirstOrDefault(x => x.Guid == e.Order.Guid);
            if (order == null)
            {
                order = new Order();
                e.Order.CloneTo(order);

                Orders.Add(order);
                OnOrdersChanged?.Invoke(Orders,
                    new OrderChangeEventArgs(new List<Order> { order }, OrderChangeAction.Add));
            }
            else
            {
                order.FillPrice = e.Order.FillPrice;
                order.OrderFillDate = e.Order.OrderFillDate;
                order.Quantity = e.Order.Quantity;
                order.LimitPrice = e.Order.LimitPrice;
                order.StopPrice = e.Order.StopPrice;
                order.RealId = e.Order.RealId;
                order.OrderState = e.Order.OrderState;
                OnOrdersChanged?.Invoke(Orders,
                    new OrderChangeEventArgs(new List<Order> { order }, OrderChangeAction.Update));
            }
        }

        private void Account_OnAccountChanged(object sender, AccountEventArgs e)
        {
            Realized = e.Realized;
            Unrealized = e.Unrealized;
        }

        private void Account_OnPositionChanged(object sender, PositionChangeEventArgs e)
        {
            if (e.PositionChangeAction == PositionChangeAction.Update)
            {
                var positionsForRemove = Positions
                    .Where(x => !e.Positions.Exists(y => y.Instrument.Symbol == x.Instrument.Symbol)).ToList();
                foreach (var positionForRemove in positionsForRemove)
                {
                    DebugController.Print("ACCOUNT OnPositionChanged. Position REMOVE." +
                                          " Symbol: " + positionForRemove.Instrument.Symbol +
                                          " MarketPosition: " + positionForRemove.MarketPosition +
                                          " Qnt: " + positionForRemove.Quantity);

                    Positions.Remove(positionForRemove);
                }

                if (positionsForRemove.Any())
                    OnPositionsChanged?.Invoke(Positions,
                        new PositionChangeEventArgs(positionsForRemove, PositionChangeAction.Remove));

                foreach (var position in e.Positions)
                {
                    var existPosition =
                        Positions.FirstOrDefault(x => x.Instrument.Symbol == position.Instrument.Symbol);
                    if (existPosition != null)
                    {
                        //Update
                        existPosition.MarketPosition = position.MarketPosition;
                        existPosition.Quantity = position.Quantity;
                        existPosition.Unrealized = position.Unrealized;
                        existPosition.AverageEntryPrice = position.AverageEntryPrice;
                        OnPositionsChanged?.Invoke(Positions,
                            new PositionChangeEventArgs(new List<Position> { existPosition },
                                PositionChangeAction.Update));

                        DebugController.Print("ACCOUNT OnPositionChanged. Position UPDATE." +
                                              " Symbol: " + existPosition.Instrument.Symbol +
                                              " MarketPosition: " + existPosition.MarketPosition +
                                              " Qnt: " + existPosition.Quantity);
                    }
                    else
                    {
                        //Add
                        var pos = position.CloneTo(new Position());
                        Positions.Add(pos);
                        OnPositionsChanged?.Invoke(Positions,
                            new PositionChangeEventArgs(new List<Position> { pos }, PositionChangeAction.Add));

                        DebugController.Print("ACCOUNT OnPositionChanged. Position ADD." +
                                              " Symbol: " + pos.Instrument.Symbol +
                                              " MarketPosition: " + pos.MarketPosition +
                                              " Qnt: " + pos.Quantity);
                    }
                }
            }
        }

        public override string ToString()
        {
            return $"{AccountType.Description()} - {Connection.Name}";
        }

        private void UpdateTotal()
        {
            TotalPnL = (Realized ?? 0) + (Unrealized ?? 0);
        }

        public void AddPosition(Position position)
        {
            Positions.Add(position);
        }

        public void RemovePosition(Position position)
        {
            Positions.Remove(position);
        }

        public void AddOrder(Order order)
        {
            Orders.Add(order);
        }

        public void Disconnect()
        {
            OnPositionsChanged?.Invoke(Positions,
                           new PositionChangeEventArgs(Positions, PositionChangeAction.Remove));

            Positions.Clear();

            OnOrdersChanged?.Invoke(Orders,
                           new OrderChangeEventArgs(Orders, OrderChangeAction.Remove));

            Orders.Clear();

            OnAssetsChanged?.Invoke(Assets,
                           new AssetChangeEventArgs(Assets, AssetChangeAction.Remove));

            Assets.Clear();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}