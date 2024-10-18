using Twm.Chart.Classes;
using Twm.Chart.Enums;
using Twm.Core.Attributes;
using Twm.Core.DataCalc;
using Twm.Core.Enums;
using Twm.Custom.Indicators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;
using Twm.Chart.Interfaces;
using Twm.Core.Market;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Twm.Core;


namespace Twm.Custom.Strategies.Default
{
    [CategoryOrder(Setup, 0)]
    [CategoryOrder(DataSeries, 1)]
    [CategoryOrder(GlobalSettings, 2)]
    [CategoryOrder(EntrySettings, 6)]
    [CategoryOrder(Filters, 7)]
    [CategoryOrder(ExitSettings, 8)]
    public class DefaultMACrossOver:Strategy
    {
        #region Constants

        private const string SystemVersion = " V1.0";
        private const string StrategyName = "MA Crossover";
        private const string StrategyGuid = "C8BFB4D3-2C74-469F-8A25-397BFE3DD574";

        private const string Setup = "Setup";
        private const string EntrySettings = "Entry Settings";
        private const string ExitSettings = "Exit Settings";
        private const string Filters = "Filters";
        private const string DataSeries = "Data series";
        private const string GlobalSettings = "Global Settings";

        #endregion

        #region Parameters

        [TwmProperty]
        [Category(EntrySettings)]
        [Display(Name = "Position Size", Order = 1, GroupName = EntrySettings)]
        public double PosSize
        { get; set; }

        [TwmProperty]
        [Category(EntrySettings)]
        [Display(Name = "Execution Series", Order = 1, GroupName = EntrySettings)]
        public int ExecSeries
        { get; set; }

        [TwmProperty]
        [Category(EntrySettings)]
        [Display(Name = "Period Fast", Order = 9, GroupName = EntrySettings)]
        public int PeriodFast
        { get; set; }

        [TwmProperty]
        [Category(EntrySettings)]
        [Display(Name = "Period Slow", Order = 10, GroupName = EntrySettings)]
        public int PeriodSlow
        { get; set; }

        public enum EntryType
        {
            Standard,
            Limit,
            StopMarket
        }

        [TwmProperty]
        [Category(EntrySettings)]
        [Display(Name = "Entry Type", Order = 11, GroupName = EntrySettings)]
        public EntryType MyEntryType
        { get; set; }

        [TwmProperty]
        [Category(EntrySettings)]
        [Display(Name = "Stop Loss", GroupName = ExitSettings, Order = 17)]
        public double StopLoss
        { get; set; }

        [TwmProperty]
        [Category(EntrySettings)]
        [Display(Name = "Profit Target", GroupName = ExitSettings, Order = 18)]
        public double ProfitTarget
        { get; set; }

        #endregion

        #region Values

        
        #endregion

        #region Init

        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                SetDefaults();
                SetParameters();
            }
            else if (State == State.Configured)
            {
                ClearDebug();
                AddIndicators();
                
                //AddDataSeries(DataSeriesType.Hour, 1,"DOGEUSDT", "FUTURE","BybitMainnet");
                
            }
        }

        private void SetDefaults()
        {
            IsAutoscale = true;
            Name = StrategyName + SystemVersion;
            Guid = new Guid(StrategyGuid);
            Version = SystemVersion;
            
        }

        
        private void SetParameters()
        {
            PeriodFast = 15;
            PeriodSlow = 50;
            PosSize = 1;
            
        }

        
        private Indicator _maFast;
        private Indicator _maSlow;
        
        private void AddIndicators()
        {
            var options = new ScriptOptions { ShowPlots = true, ShowPanes = false};
            _maFast = EMA(High, PeriodFast);
            _maSlow = EMA(Low, PeriodSlow);
        }

        #endregion

        #region Business

        public override void OnBarUpdate()
        {
            if (CurrentBar < PeriodSlow*3)
                return;

            //Print("Close:" + Closes[1][0] + " Open: " + Opens[1][0] + " High: " + Highs[1][0] + " Low: "+ Lows[1][0] + " Time: " + DateTimes[1][0]);

            SetSignal();
            //ExitModule();
            EntryModule();
           
        }

        private double _lastPrice;
        private bool _marketEntryOrderSubmitted;

        public override void OnTickUpdate(ICandle candle, ICandle tick)
        {
            var close = candle.C;
            var high = candle.H;
            var low = candle.L;
            var open = candle.O;
            var time = candle.t;

            Print("CANDLE. Close: " + close + " High:  " + high + " Low: " + low + " Open: " + open + " Time: " + time);

            //Print(" State: "+ State + " Account Type: " + Account.AccountType);

            if (candle.C > _lastPrice && LastPosition.MarketPosition == MarketPosition.Flat && !_marketEntryOrderSubmitted)
            {
                _marketEntryOrderSubmitted = true;
                PlaceMarketEntryOrder(true);
            }

            _lastPrice = candle.C;

        }

        private bool _enterLong;
        private bool _enterShort;


        private void SetSignal()
        {
            _enterLong = Close[0] > _maFast[0] && _maFast[0] > _maSlow[0] &&  _maFast[1] <= _maSlow[1];
            _enterShort = Close[0] < _maFast[0] && _maFast[0] < _maSlow[0] &&  _maFast[1] >= _maSlow[1];

            if (_enterShort)
                Draw.Arrow(this, ArrowDirection.Down, CurrentBar+"Short",0, High[0], ArrowConnector.End, Brushes.Red);

            if (_enterLong)
                Draw.Arrow(this, ArrowDirection.Up, CurrentBar+"Long",0, Low[0], ArrowConnector.End, Brushes.Green);
            
        }

        
        private void EntryModule()
        {
            if (LastPosition.MarketPosition != MarketPosition.Flat)
                return;

            if (MyEntryType == EntryType.Standard)
            {
                if (_enterLong)
                    PlaceMarketEntryOrder(true);

                if (_enterShort)
                    PlaceMarketEntryOrder(false);
            }

            if (MyEntryType == EntryType.Limit)
            {
                if (_enterLong || _enterShort)
                {
                    if (_entryOrder != null)
                        CancelOrder(_entryOrder);
                }

                //if (_enterLong)
                //    PlaceLimitEntryOrder(true, High[0] + TickSize*50);

                //if (_enterShort)
                //    PlaceLimitEntryOrder(false,  Low[0] - TickSize*50);

                if (_enterLong)
                    PlaceLimitEntryOrder(true, _maSlow[0]);

                if (_enterShort)
                    PlaceLimitEntryOrder(false,  _maSlow[0]);
            }

            if (MyEntryType == EntryType.StopMarket)
            {
                if (_enterLong || _enterShort)
                {
                    if (_entryOrder != null)
                        CancelOrder(_entryOrder);
                }

                //if (_enterLong)
                //    PlaceLimitEntryOrder(true, High[0] + TickSize*50);

                //if (_enterShort)
                //    PlaceLimitEntryOrder(false,  Low[0] - TickSize*50);

                if (_enterLong)
                    PlaceStopMarketEntryOrder(true, Low[0] - TickSize * 300);

                //if (_enterShort)
                //    PlaceStopMarketEntryOrder(false,  High[0] + TickSize*300);
            }
        }

        


        private void ExitModule()
        {
            MaCrossBackExit();
        }


        private void MaCrossBackExit()
        {
            if (LastPosition.MarketPosition == MarketPosition.Flat)
                return;

            if (LastPosition.MarketPosition == MarketPosition.Long)
            {
                if (_maFast[0] < _maSlow[0] &&  _maFast[1] >= _maSlow[1])
                {
                    PlaceMarketExitOrder(true);
                    //MarkBar(true, Brushes.CornflowerBlue);
                }

            }
            if (LastPosition.MarketPosition == MarketPosition.Short)
            {
                if (_maFast[0] > _maSlow[0] &&  _maFast[1] <= _maSlow[1])
                {
                    PlaceMarketExitOrder(false);
                    //MarkBar(true, Brushes.CornflowerBlue);
                }
            }
        }

        private void MarkBar(bool longBar, Brush brush)
        {

            if (longBar)
                Draw.Arrow(this, ArrowDirection.Up, CurrentBar+"Tag1", 0, Low[0], ArrowConnector.End, brush);

            if (!longBar)
                Draw.Arrow(this, ArrowDirection.Down, CurrentBar+"Tag1", 0, High[0], ArrowConnector.End, brush);
        }

        private void ClosePositions()
        {
            if (LastPosition.MarketPosition == MarketPosition.Long)
                PlaceMarketExitOrder(true);

            if (LastPosition.MarketPosition == MarketPosition.Short)
                PlaceMarketExitOrder(false);
        }

        #endregion

        #region Orders

        private Order _entryOrder;
        private Order _exitProfitOrder;
        private Order _exitStopOrder;
        private Order _exitMarketOrder;

        private const string EnterLongName = "Enter Long";
        private const string EnterShortName = "Enter Short";
        private const string ExitLongStopName = "Stop Long";
        private const string ExitShortStopName = "Stop Short";
        private const string ExitLongProfitName = "PT-Long";
        private const string ExitShortProfitName = "PT-Short";
        private const string MarketCloseLong = "Exit Long Market";
        private const string MarketCloseShort = "Exit Short Market";


        private void PlaceMarketEntryOrder(bool isLong)
        {
            
            if (isLong)
            {
                _entryOrder = SubmitOrder(ExecSeries, OrderAction.Buy, OrderType.Market, PosSize,
                    0, 0,0, "", EnterLongName);

            }
            if (!isLong)
            {
                _entryOrder = SubmitOrder(ExecSeries, OrderAction.SellShort, OrderType.Market, PosSize,
                    0, 0,0, "", EnterShortName);

            }

            

        }

        private void PlaceLimitEntryOrder(bool isLong, double price)
        {
            
            if (isLong)
            {
                _entryOrder = SubmitOrder(ExecSeries, OrderAction.Buy, OrderType.Limit, PosSize,
                    price, 0,0, "", EnterLongName);

            }
            if (!isLong)
            {
                _entryOrder = SubmitOrder(ExecSeries, OrderAction.SellShort, OrderType.Limit, PosSize,
                    price, 0,0, "", EnterShortName);

            }

        }

        private void PlaceStopMarketEntryOrder(bool isLong, double price)
        {
            
            if (isLong)
            {
                _entryOrder = SubmitOrder(ExecSeries, OrderAction.Buy, OrderType.StopMarket, PosSize,
                    0, 0, price, "", EnterLongName);

            }
            if (!isLong)
            {
                _entryOrder = SubmitOrder(ExecSeries, OrderAction.SellShort, OrderType.StopMarket, PosSize,
                    0, 0, price, "", EnterShortName);

            }

        }

        private void PlaceStopLimitOrder(bool isLong, double price, double triggerPrice)
        {
            
            if (isLong)
            {
                _entryOrder = SubmitOrder(ExecSeries, OrderAction.Buy, OrderType.StopMarket, PosSize,
                    price, 0, triggerPrice, "", EnterLongName);

            }
            if (!isLong)
            {
                _entryOrder = SubmitOrder(ExecSeries, OrderAction.SellShort, OrderType.StopMarket, PosSize,
                    price, 0, triggerPrice, "", EnterShortName);

            }

        }

        private void PlaceMarketExitOrder(bool isLong)
        {
            if (isLong)
            {
                _exitMarketOrder = SubmitOrder(ExecSeries, OrderAction.Sell, OrderType.Market, LastPosition.Quantity,
                    0, 0,0, "", MarketCloseLong);
            }
            else if (!isLong)
            {
                _exitMarketOrder = SubmitOrder(ExecSeries, OrderAction.BuyToCover, OrderType.Market, LastPosition.Quantity,
                    0, 0,0, "", MarketCloseShort);
            }
        }

        private double _stop;
        private double _profit;

        private void PlaceStopExitOrder(bool isStopLong)
        {


            if (isStopLong)
            {
                _exitStopOrder = SubmitOrder(ExecSeries, OrderAction.Sell,
                    OrderType.StopMarket, LastPosition.Quantity, 0, _stop,0, "", ExitLongStopName);

            }

            if (!isStopLong)
            {
                _exitStopOrder = SubmitOrder(ExecSeries, OrderAction.BuyToCover,
                    OrderType.StopMarket, LastPosition.Quantity, 0, _stop,0, "", ExitShortStopName);

            }
        }

        private void PlaceLimitProfitExitOrder(bool isProfitOrderLong)
        {

            if (isProfitOrderLong)
            {
                _exitProfitOrder = SubmitOrder(ExecSeries, OrderAction.Sell,
                    OrderType.Limit, PosSize, _profit, 0,0, "", ExitLongProfitName);

            }

            if (!isProfitOrderLong)
            {
                _exitProfitOrder = SubmitOrder(ExecSeries, OrderAction.BuyToCover,
                    OrderType.Limit, PosSize, _profit, 0,0, "", ExitShortProfitName);

            }
        }


        #endregion

        #region Execution Events

        
        public override void OnOrderUpdate(Order order)
        {
            //Print("On order update: " + order.Name + 
            //      " Qnt: " + order.Quantity + 
            //      " Price: " + order.FillPrice+ 
            //      " Guid: "+ order.Guid + 
            //      " State: "+ order.OrderState);

            //if (order.OrderState != OrderState.Filled)
            //    return;

            //Print("On order update order filled");

            if (order.OrderState != OrderState.Filled)
                return;

            if (State == State.Historical)
                ManageOrders(order);
        }

        public override void OnExecutionUpdate(Order order)
        {
            Print("On execution update: "+ order.Name + 
                  " Qnt: " + order.Quantity + 
                  " Price: " + order.FillPrice + 
                  " Guid: "+ order.Guid +
                  " State: "+ order.OrderState);

            if (order.OrderState != OrderState.Filled)
                return;

            Print("On execution update order filled");

            if (State == State.RealTime)
                ManageOrders(order);
        }

        
        private void ManageOrders(Order order)
        {
            var orderName = order.Name;


            if (orderName == EnterLongName || orderName == EnterShortName)
            {
               // _marketEntryOrderSubmitted = false;

                if (orderName == EnterLongName)
                {
                    _profit = order.FillPrice + ProfitTarget;
                    _stop = order.FillPrice - StopLoss;

                    PlaceStopExitOrder(true);
                    PlaceLimitProfitExitOrder(true);
                }
                if (orderName == EnterShortName)
                {
                    _profit = order.FillPrice - ProfitTarget;
                    _stop = order.FillPrice + StopLoss;

                    PlaceStopExitOrder(false);
                    PlaceLimitProfitExitOrder(false);
                }
            }

            if (orderName == ExitLongStopName || orderName == ExitShortStopName)
            {
                if (_exitProfitOrder != null)
                    CancelOrder(_exitProfitOrder);

                if (_exitProfitOrder != null)
                    CancelOrder(_exitProfitOrder);
            }

            if (orderName == ExitLongProfitName || orderName == ExitShortProfitName)
            {
                if (_exitStopOrder != null)
                    CancelOrder(_exitStopOrder);

                if (_exitStopOrder != null)
                    CancelOrder(_exitStopOrder);
            }

            if (orderName == MarketCloseLong || orderName == MarketCloseShort)
            {
                
                if (_exitStopOrder != null)
                    CancelOrder(_exitStopOrder);

                if (_exitProfitOrder != null)
                    CancelOrder(_exitProfitOrder);
            }


        }

        private void LogMe(string text)
        {
            Print(DateTime[0] + text);
        }


        #endregion
    }
}
