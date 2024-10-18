using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms.VisualStyles;
using Twm.Chart.Interfaces;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Core.Market;
using Twm.Model.Model;


namespace Twm.Core.DataProviders.ExchangeMockUp
{
    public class ExchangeMockUp
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);
        private readonly Dictionary<string, Order> _realOrderBook;
        
        private int _realOrderId;
        

        public event EventHandler<ExchangeOrderStatusEventArgs> OnOrderStatusChanged;

        private double _accountRealizedPnl; 
        private double _accountUnrealizedPnl;

        public event EventHandler<PositionChangeEventArgs> OnPositionChanged;
        public event EventHandler<AccountEventArgs> OnAccountChanged;

        public Account Account { get; set; }
        

        public ExchangeMockUp()
        {
            Account = new Account();
            _realOrderBook = new Dictionary<string, Order>();
            _accountRealizedPnl = 0;
            _accountUnrealizedPnl = 0;
        }


        public async  void SubmitOrder(Order order)
        {
           
            order.OrderState = OrderState.Accepted;
            
            await _semaphore.WaitAsync();
            try
            {
                order.RealId = _realOrderId++;
                
                _realOrderBook.Add(order.Guid, order);
                Account.Orders.Add(order);

            }
            finally
            {
                _semaphore.Release(1);
            }

            
            ExecutionUpdate(order);
            order.OrderState = OrderState.Working;

            ExecutionUpdate(order);
        }

        private bool _logMock = true;

        //private void LogMockUp(string text)
        //{
        //    if (!_logMock)
        //        return;

        //    LogController.Print("MOCK-UP: "+  text);
        //}

        public async void CancelOrder(Order order, bool isRemove = false)
        {
            if (_realOrderBook == null)
                return;

            order.OrderState = OrderState.Cancelled;
            await _semaphore.WaitAsync();
            try
            {
                
               if (_realOrderBook.TryGetValue(order.Guid, out var existing))
               {
                   existing.OrderState = OrderState.Cancelled;
                    if (isRemove) 
                    {
                       
                        _realOrderBook.Remove(existing.Guid);
                        Account.Orders.Remove(existing);
                    }
               }
               
            }
            finally
            {
                _semaphore.Release(1);
            }
            if (!isRemove)
                ExecutionUpdate(order);

        }

        //public async void UpdateOrder(Order order)
        //{
        //    if (_realOrderBook == null)
        //        return;

        //    order.OrderState = OrderState.ChangeSubmitted;
        //    await _semaphore.WaitAsync();

        //    try
        //    {
        //        if (_realOrderBook.TryGetValue(order.Guid, out var existing))
        //        {
        //            existing.OrderState = OrderState.Working;
        //            existing.Quantity = order.Quantity;
        //            existing.


        //            _realOrderBook.FirstOrDefault(existing)
        //            Account.Orders.Remove(existing);

        //            ExecutionUpdate(order);
        //        }

        //    }
        //    finally
        //    {
        //        _semaphore.Release(1);
        //    }

        //}

        public async void OnTickUpdate(ICandle candle, ICandle tick, string symbol)
        {
                List<Order> workingOrders;
                await _semaphore.WaitAsync();
                try
                {
                    workingOrders = _realOrderBook.Values
                        .Where(a => a.OrderState == OrderState.Working && a.Instrument.Symbol == symbol).ToList();
                }
                finally
                {
                    _semaphore.Release(1);
                }

                foreach (var workingOrder in workingOrders)
                {
                    switch (workingOrder.OrderType)
                    {
                        case OrderType.Market:
                            FillOrder(workingOrder, tick);
                            break;

                        case OrderType.StopMarket:
                            var order = workingOrder;

                            if (order.OrderAction == OrderAction.Sell || order.OrderAction == OrderAction.SellShort)
                            {
                                if (tick.C <= order.StopPrice)
                                {
                                    FillOrder(order, tick);

                                }
                            }

                            if (order.OrderAction == OrderAction.Buy || order.OrderAction == OrderAction.BuyToCover)
                            {
                                if (tick.C >= order.StopPrice)
                                {
                                    FillOrder(order, tick);
                                }
                            }
                            break;

                        case OrderType.Limit:

                            
                            if (workingOrder.OrderAction == OrderAction.Sell || workingOrder.OrderAction == OrderAction.SellShort)
                            {
                                if (tick.C >= workingOrder.LimitPrice)
                                {
                                    FillOrder(workingOrder, tick);
                                }
                            }

                            if (workingOrder.OrderAction == OrderAction.Buy || workingOrder.OrderAction == OrderAction.BuyToCover)
                            {
                                if (tick.C <= workingOrder.LimitPrice)
                                {
                                    FillOrder(workingOrder, tick);
                                }
                            }


                            break;
                    }
                }

                _accountUnrealizedPnl = CalculateAccountUnrealized(tick, symbol);
                OnAccountChanged?.Invoke(this, new AccountEventArgs(_accountRealizedPnl, _accountUnrealizedPnl));

        }

        private void FillOrder(Order order, ICandle tick)
        {
            order.FillPrice = tick.C;
            order.OrderFillDate = tick.t;
            order.OrderState = OrderState.Filled;
            ExecutionUpdate(order);
            HandleFill(order);
        }

        private double CalculateAccountUnrealized(ICandle tick, string symbol)
        {
            var accountUnrealized = 0d;
            

            for (int j = 0; j < Account.Positions.Count; j++)
            {
                if (Account.Positions[j].Instrument.Symbol != symbol)
                    continue;

                var positionUnrealized = 0d;

                for (int i = 0; i < Account.Positions[j].Trades.Count; i++)
                {
                    var entryPrice = Account.Positions[j].Trades[i].EntryPrice;
                    var qnt = Account.Positions[j].Trades[i].EntryQuantity;
                    var isLong = Account.Positions[j].MarketPosition == MarketPosition.Long;
                    var multiplier = Account.Positions[j].Instrument.Multiplier != null ? Account.Positions[j].Instrument.Multiplier : 1;
                    var tradePnlUsd = 0d;

                    if (isLong)
                    {
                        tradePnlUsd = (tick.C - entryPrice) * (double)multiplier * qnt;
                        
                    }
                    if (!isLong)
                    {
                        tradePnlUsd = (entryPrice - tick.C) * (double)multiplier * qnt;
                        
                    }

                    positionUnrealized += tradePnlUsd;
                }

                Account.Positions[j].Unrealized = positionUnrealized;
            }

            var positions = new List<Position>();
            foreach (var position in Account.Positions)
            {
                position.AverageEntryPrice = position.Trades.Sum(x => x.EntryPrice) / position.Trades.Count;
                positions.Add(position.CloneTo(new Position()));
                accountUnrealized += (double)position.Unrealized;
            }

            OnPositionChanged?.Invoke(this, new PositionChangeEventArgs(positions, PositionChangeAction.Update));

            return accountUnrealized;
        }

        private void HandleFill(Order order)
        {
            var pos = Account.Positions.FirstOrDefault(i => i.Instrument.Symbol == order.Instrument.Symbol);

            if (pos == null)
            {
                 CreateNewPosition(order);
                
            }
            else
            {
                var multiplier = pos.Instrument.Multiplier != null ? pos.Instrument.Multiplier : 1;
                var posSize = pos.Trades.Sum(q => q.EntryQuantity);

                if (pos.MarketPosition == MarketPosition.Long)
                {
                    if (order.OrderAction == OrderAction.Buy || order.OrderAction == OrderAction.BuyToCover)
                    {
                        pos.AverageEntryPrice = (pos.Quantity * pos.AverageEntryPrice + (order.Quantity * order.FillPrice)) / pos.Quantity + order.Quantity;
                        pos.Trades.Add(new Trade() { EntryPrice = order.FillPrice, EntryQuantity = order.Quantity });
                        pos.Quantity = pos.Trades.Sum(x => x.EntryQuantity);
                    }
                    if (order.OrderAction == OrderAction.Sell || order.OrderAction == OrderAction.SellShort)
                    {
                        if (posSize - order.Quantity == 0)
                        {
                            HandleFullClose(pos, order, (double)multiplier, true);
                        }
                        else
                        {
                            HandlePartialCloseAndOverfill(pos, order, (double)multiplier, true);
                        }
                    }
                }
                else if (pos.MarketPosition == MarketPosition.Short)
                {
                    if (order.OrderAction == OrderAction.Sell || order.OrderAction == OrderAction.SellShort)
                    {
                        pos.AverageEntryPrice = (pos.Quantity * pos.AverageEntryPrice + (order.Quantity * order.FillPrice)) / pos.Quantity + order.Quantity;
                        pos.Trades.Add(new Trade() { EntryPrice = order.FillPrice, EntryQuantity = order.Quantity });
                        pos.Quantity = pos.Trades.Sum(x => x.EntryQuantity);
                    }
                    if (order.OrderAction == OrderAction.Buy || order.OrderAction == OrderAction.BuyToCover)
                    {
                        if (posSize - order.Quantity == 0)
                        {
                            HandleFullClose(pos, order, (double)multiplier, false);
                        }
                        else
                        {
                            HandlePartialCloseAndOverfill(pos, order, (double)multiplier, false);
                        }
                    }

                }

            }

            
            var positions = new List<Position>();
            foreach (var position in Account.Positions)
            {
                position.AverageEntryPrice = position.Trades.Sum(x => x.EntryPrice) / position.Trades.Count;
                positions.Add(position.CloneTo(new Position()));
            }

            OnPositionChanged?.Invoke(this, new PositionChangeEventArgs(positions, PositionChangeAction.Update));
        }

        private void HandleFullClose(Position pos, Order order, double multiplier, bool isLong)
        {
            for (int i = 0; i < pos.Trades.Count; i++)
            {
                var exitPrice = order.FillPrice;
                var entryPrice = pos.Trades[i].EntryPrice;
                var tradeQnt = pos.Trades[i].EntryQuantity;
                var tradeProfit = exitPrice - entryPrice;

                if (!isLong)
                {
                    tradeProfit = entryPrice - exitPrice;
                }

                _accountRealizedPnl += tradeProfit * tradeQnt * multiplier;
            }

            Account.Positions.Remove(pos);
        }

        private void HandlePartialCloseAndOverfill(Position pos, Order order, double multiplier, bool isLong)
        {
            
            var closingQnt = order.Quantity;
            int removeRange = 0;

            for (int i = 0; i < pos.Trades.Count; i++)
            {
                var exitPrice = order.FillPrice;
                var entryPrice = pos.Trades[i].EntryPrice;
                var tradeQnt = pos.Trades[i].EntryQuantity;
                var tradeProfit = exitPrice - entryPrice;

                if (!isLong)
                {
                    tradeProfit = entryPrice - exitPrice;
                }

                if (tradeQnt - closingQnt <= 0)
                {
                    closingQnt -= tradeQnt;
                    _accountRealizedPnl += tradeProfit * tradeQnt * multiplier;
                    pos.Quantity -= tradeQnt;
                    removeRange++;


                }
                else if (tradeQnt - closingQnt > 0)
                {
                    pos.Trades[i].EntryQuantity -= closingQnt;
                    pos.Quantity -= closingQnt;
                    _accountRealizedPnl += tradeProfit * closingQnt * multiplier;
                    closingQnt = 0;
                }
            }

            if (pos.Quantity == 0)
            {
                Account.Positions.Remove(pos);
            }
            else
            {
                pos.Trades.RemoveRange(0, removeRange);

                var sum = 0d;

                for (int i = 0; i < pos.Trades.Count; i++)
                {
                    sum += pos.Trades[i].EntryPrice * pos.Trades[i].EntryQuantity;
                }

                pos.AverageEntryPrice = sum / pos.Trades.Sum(t => t.EntryQuantity);
            }

            var type  = MarketPosition.Short;

            if (!isLong)
            {
                type = MarketPosition.Long;
            }

            if (closingQnt > 0)
            {
                var newPos = new Position()
                {
                    Instrument = order.Instrument,
                    MarketPosition = type,
                    EntryDate = order.OrderFillDate??DateTime.MinValue,
                    Trades = new List<Trade>(),
                    Quantity = closingQnt,
                    AverageEntryPrice = order.FillPrice
                };

                newPos.Trades.Add(new Trade() { EntryPrice = order.FillPrice, EntryQuantity = closingQnt });

                Account.Positions.Add(newPos);
            }
            
        }

        private Position CreateNewPosition(Order order)
        {
            var marketPos =  MarketPosition.Flat;

            if (order.OrderAction == OrderAction.SellShort)
            {
                marketPos = MarketPosition.Short;
            }
            else if (order.OrderAction == OrderAction.Buy)
            {
                marketPos = MarketPosition.Long;
            }
            else
            {
                return null;
            }

            var pos = new Position()
            {
                Instrument = order.Instrument,
                MarketPosition = marketPos,
                EntryDate = order.OrderFillDate ?? DateTime.MinValue,
                Trades = new List<Trade>(),
                Quantity = order.Quantity,
                AverageEntryPrice = order.FillPrice
            };

            pos.Trades.Add(new Trade(){EntryPrice = order.FillPrice, EntryQuantity = order.Quantity });

            
            Account.Positions.Add(pos);

            return pos;
        }

        public async void ChangeOrderQuantity(Order order, double quantity)
        {
            if (_realOrderBook == null)
                return;

            order.OrderState = OrderState.ChangeSubmitted;
            ExecutionUpdate(order);

            await _semaphore.WaitAsync();

            try
            {
                if (_realOrderBook.TryGetValue(order.Guid, out var existing))
                {
                    existing.OrderState = OrderState.Working;
                    existing.Quantity = quantity;
                    ExecutionUpdate(existing);
                }

            }
            finally
            {
                _semaphore.Release(1);
            }
        }

        public async void ChangeOrderPrice(Order order, double price)
        {
            
            if (_realOrderBook == null)
                return;

            order.OrderState = OrderState.ChangeSubmitted;
            ExecutionUpdate(order);

            await _semaphore.WaitAsync();

            try
            {
                if (_realOrderBook.TryGetValue(order.Guid, out var existing))
                {
                    
                    if (order.OrderType == OrderType.Limit)
                    {
                        existing.LimitPrice = price;
                    }
                    if (order.OrderType == OrderType.StopMarket)
                    {
                        existing.StopPrice = price;
                    }

                    existing.OrderState = OrderState.Working;
                    ExecutionUpdate(existing);
                }

            }
            finally
            {
                _semaphore.Release(1);
            }

        }

        private void ExecutionUpdate(Order order)
        {
            OnOrderStatusChanged?.Invoke(this, new ExchangeOrderStatusEventArgs((Order)order.Clone()));
            //OnOrderStatusChanged?.BeginInvoke(this, new ExchangeOrderStatusEventArgs((Order)order.Clone()), null, null);
        }

        public async void Clear()
        {
            await _semaphore.WaitAsync();
            try
            {
                _realOrderBook.Clear();
            }
            finally
            {
                _semaphore.Release(1);
            }
        }


       
    }

    public class ExchangeOrderStatusEventArgs : EventArgs
    {
        public Order Order { get; set; }

        public ExchangeOrderStatusEventArgs(Order order)
        {
            Order = order;
        }
    }

   

   


    public class AccountEventArgs : EventArgs
    {
        public double Realized { get; set; }

        public double Unrealized { get; set; }

        public AccountEventArgs(double realized, double unrealized)
        {
            Realized = realized;
            Unrealized = unrealized;
        }
    }

    
}