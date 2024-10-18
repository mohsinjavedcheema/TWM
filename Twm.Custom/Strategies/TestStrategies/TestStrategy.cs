using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Serialization;
using Twm.Chart.Classes;
using Twm.Chart.Controls;
using Twm.Chart.Enums;
using Twm.Core.Attributes;
using Twm.Core.DataCalc;
using Twm.Core.Enums;
using Twm.Core.Market;
using Twm.Core.Tests.Enums;
using Twm.Custom.Indicators;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Twm.Custom.Strategies.TestStrategies
{
    [CategoryOrder(Global, 5)]
    [CategoryOrder(Entry, 6)]
    [CategoryOrder(Filters, 7)]
    [CategoryOrder(Exits, 8)]
    public class TestStrategy : Strategy
    {
        private const string SystemVersion = " V1.0";
        private const string StrategyName = "Test Strategy";
        private const string StrategyGuid = "2820CFD1-37C5-4A93-8B42-79018B6746DA";

        private const string Global = "Global";
        private const string Entry = "Entry";
        private const string Filters = "Filter";
        private const string Exits = "Exits";

        #region Parameters

        private StrategySpeedSettings _speedSettings;
        private StrategyValueCheckSettings _valueCheckSettings;

        [TwmProperty]
        [Category(Global)]
        [Display(Name = "Chart Run", GroupName = Global, Order = 0)]
        public bool IsChartRun
        {
            get;
            set;
        }

        [TwmProperty]
        [Category(Global)]
        [Display(Name = "Speed Settings", GroupName = Global, Order = 1)]
        public StrategySpeedSettings SpeedSettings
        {
            get { return _speedSettings; }   // get method
            set { _speedSettings = value; }  // set method
        }

        [TwmProperty]
        [Category(Global)]
        [Display(Name = "Value Settings", GroupName = Global, Order = 2)]
        public StrategyValueCheckSettings ValueSettings
        {
            get { return _valueCheckSettings; }   // get method
            set { _valueCheckSettings = value; }  // set method
        }

        

        private bool _isSpeedTest;
        #endregion

        #region Init

        private Indicator _maFast;
        private Indicator _maSlow;
        private int _barCount;

        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                SetDefaults();
                SetParameters();

            }
            else if (State == State.Configured)
            {
                _barCount = Close.Count - 1;
                

                if (!IsChartRun)
                {
                    _speedSettings = StrategySpeedSettings.NotSet;
                    _valueCheckSettings = StrategyValueCheckSettings.NotSet;

                    if (Tag is StrategySpeedSettings)
                    {
                        _speedSettings = (StrategySpeedSettings)(Tag);
                        _isSpeedTest = true;

                    }
                    else if (Tag is StrategyValueCheckSettings)
                    {
                        _valueCheckSettings = (StrategyValueCheckSettings)Tag;
                    }
                    else
                    {
                        throw new Exception("Invalid test settings: " + Tag);
                    }
                }
                else if (IsChartRun)
                {
                    if (_speedSettings != StrategySpeedSettings.NotSet)
                        _isSpeedTest = true;
                }

                

                InitializeMa();
            }
        }

        private void SetDefaults()
        {
            Name = StrategyName + SystemVersion;
            Guid = new Guid(StrategyGuid);
            Version = SystemVersion;
        }

        private void SetParameters()
        {
            
        }

        private void InitializeMa()
        {
            if (_valueCheckSettings == StrategyValueCheckSettings.T01_SimpleMACrossOverStrategy ||
                _speedSettings == StrategySpeedSettings.T02_SimpleMACrossOverStrategy ||
                _speedSettings == StrategySpeedSettings.T03_MACrossOverLimitOrderEntry ||
                _speedSettings == StrategySpeedSettings.T04_SMACrossOverSLTPTest ||
                _valueCheckSettings == StrategyValueCheckSettings.T02_MACrossOverLimitOrderEntry ||
                _valueCheckSettings == StrategyValueCheckSettings.T03_SMACrossOverSLTPTest)
            {
                var options = new ScriptOptions();
                options.ShowPlots = true;
                options.ShowPanes = false;

                _maFast = EMA(25, options);
                _maSlow = EMA(50, options);
            }

            
        }


        #endregion

        #region Business

        private DateTime _start;
        private DateTime _end;

        public override void OnBarUpdate()
        {
            if (CurrentBar == 1)
            {
                _start = System.DateTime.Now;
            }
            

            if (CurrentBar-1 < 50 * 3)
                return;

            if (_isSpeedTest)
            {
                switch (_speedSettings)
                {
                    case StrategySpeedSettings.T01_BlankExecution:
                        T01S_BlankExecutionTest();
                        break;
                    case StrategySpeedSettings.T02_SimpleMACrossOverStrategy:
                        SimpleMACrossOverStrategyTest();
                        break;
                    case StrategySpeedSettings.T03_MACrossOverLimitOrderEntry:
                        MACrossOverLimitOrderEntryTest();
                        break;
                    case StrategySpeedSettings.T04_SMACrossOverSLTPTest:
                        SMACrossOverSLTPTest();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (_valueCheckSettings)
                {
                    case StrategyValueCheckSettings.T01_SimpleMACrossOverStrategy:
                        SimpleMACrossOverStrategyTest();
                        break;
                    case StrategyValueCheckSettings.T02_MACrossOverLimitOrderEntry:
                        MACrossOverLimitOrderEntryTest();
                        break;
                    case StrategyValueCheckSettings.T03_SMACrossOverSLTPTest:
                        SMACrossOverSLTPTest();
                        break;
                    default:
                        break;
                }
            }


            if (CurrentBar == _barCount)
            {
                _end = System.DateTime.Now;

                Tag = (_end - _start).TotalMilliseconds;
            }

        }

        private void T01S_BlankExecutionTest()
        {

        }

        private void SimpleMACrossOverStrategyTest()
        {
            var enterLong = _maFast[0] > _maSlow[0] && _maFast[1] < _maSlow[1];
            var enterShort = _maFast[0] < _maSlow[0] && _maFast[1] > _maSlow[1];


            if (LastPosition.MarketPosition == MarketPosition.Flat)
            {
                if (enterLong)
                    PlaceMarketEntryOrder(true);

                if (enterShort)
                    PlaceMarketEntryOrder(false);
            }
            else if (LastPosition.MarketPosition == MarketPosition.Long)
            {
                if (enterShort)
                {
                    PlaceMarketExitOrder(true);
                    PlaceMarketEntryOrder(false);
                }
            }
            else if (LastPosition.MarketPosition == MarketPosition.Short)
            {
                if (enterLong)
                {
                    PlaceMarketExitOrder(false);
                    PlaceMarketEntryOrder(true);
                }
            }
        }

        private bool _limitEntryOrderPlaced;

        private void MACrossOverLimitOrderEntryTest()
        {
            var enterLong = _maFast[0] > _maSlow[0] && _maFast[1] < _maSlow[1];
            var enterShort = _maFast[0] < _maSlow[0] && _maFast[1] > _maSlow[1];

            if (_limitEntryOrderPlaced)
            {
                CancelOrder(_limitOrder);
                _limitEntryOrderPlaced = false;
            }

            if (LastPosition.MarketPosition != MarketPosition.Flat)
            {
                _limitEntryOrderPlaced = false;

                if (LastPosition.MarketPosition == MarketPosition.Long)
                {
                    if (enterShort)
                    {
                        PlaceMarketExitOrder(true);
                    }
                }
                else if (LastPosition.MarketPosition == MarketPosition.Short)
                {
                    if (enterLong)
                    {
                        PlaceMarketExitOrder(false);
                    }
                }
            }
            else if (LastPosition.MarketPosition == MarketPosition.Flat)
            {
                if (enterLong && !_limitEntryOrderPlaced)
                {
                    _limitEntryOrderPlaced = true;
                    PlaceLimitOrder(true, Close[0] - TickSize, "LongLimitEntry");
                }

                if (enterShort && !_limitEntryOrderPlaced)
                {
                    _limitEntryOrderPlaced = true;
                    PlaceLimitOrder(false, Close[0] + TickSize, "ShortLimitEntry");
                }
            }
        }

        private void SMACrossOverSLTPTest()
        {
            var enterLong = _maFast[0] > _maSlow[0] && _maFast[1] < _maSlow[1];
            var enterShort = _maFast[0] < _maSlow[0] && _maFast[1] > _maSlow[1];

            if (LastPosition.MarketPosition == MarketPosition.Flat)
            {
                if (enterLong)
                    PlaceMarketEntryOrder(true);

                if (enterShort)
                    PlaceMarketEntryOrder(false);
            }
        }

        #endregion

        #region Orders

        private Order _entryOrder;
        private Order _exitStopOrder;
        private Order _exitProfitOrder;
        private Order _limitOrder;
        private Order _exitMarketOrder;

        private const string EnterLongName = "Long";
        private const string EnterShortName = "Short";
        private const string ExitLongStopName = "Long Stop";
        private const string ExitShortStopName = "Short Stop";
        private const string ExitLongProfitName = "Long Profit";
        private const string ExitShortProfitName = "Short Profit";
        private const string ExitLongMarketName = "Long Market Exit";
        private const string ExitShortMarketName = "Short Market Exit";

        private double _stop;
        private double _profit;

        private void PlaceMarketEntryOrder(bool isLong)
        {
            if (isLong)
            {
                _entryOrder = SubmitOrder(0, OrderAction.Buy,
                    OrderType.Market, 1, 0, 0,0, "",
                    EnterLongName, null);
            }
            if (!isLong)
            {
                _entryOrder = SubmitOrder(0, OrderAction.SellShort,
                    OrderType.Market, 1, 0, 0,0, "",
                    EnterShortName, null);
            }

        }

        private void PlaceMarketExitOrder(bool isLong)
        {
            if (isLong)
            {
                _exitMarketOrder = SubmitOrder(0, OrderAction.Sell,
                    OrderType.Market, LastPosition.Quantity, 0, 0,0, "",
                    ExitLongMarketName);
            }
            if (!isLong)
            {
                _exitMarketOrder = SubmitOrder(0, OrderAction.BuyToCover,
                    OrderType.Market, LastPosition.Quantity, 0, 0,0, "",
                    ExitShortMarketName);
            }
        }

        private void PlaceStopMarketOrder(bool isLong)
        {

            if (isLong)
            {
                _exitStopOrder = SubmitOrder(0, OrderAction.Sell,
                    OrderType.StopMarket, LastPosition.Quantity, 0,0, _stop, "",
                    ExitLongStopName);
            }

            if (!isLong)
            {
                _exitStopOrder = SubmitOrder(0, OrderAction.BuyToCover,
                    OrderType.StopMarket, LastPosition.Quantity, 0,0, _stop, "",
                    ExitShortStopName);
            }

        }

        private void PlaceProfitLimitOrder(bool isLong)
        {

            if (isLong)
            {
                _exitProfitOrder = SubmitOrder(0, OrderAction.Sell,
                    OrderType.Limit, LastPosition.Quantity, _profit, 0,0, "",
                    ExitLongProfitName);
            }

            if (!isLong)
            {
                _exitProfitOrder = SubmitOrder(0, OrderAction.BuyToCover,
                    OrderType.Limit, LastPosition.Quantity, _profit, 0,0, "",
                    ExitShortProfitName);
            }

        }

        private void PlaceLimitOrder(bool isLong, double price, string name)
        {

            if (isLong)
            {
                _limitOrder = SubmitOrder(0, OrderAction.Buy,
                    OrderType.Limit, 1, price, 0,0, "",
                    name);
            }

            if (!isLong)
            {
                _limitOrder = SubmitOrder(0, OrderAction.SellShort,
                    OrderType.Limit, 1, price, 0,0, "",
                    name);
            }

        }


        #endregion

        #region On Order Update

        public override void OnOrderUpdate(Order order)
        {
            if (order.OrderState != OrderState.Filled)
                return;

            if (_valueCheckSettings == StrategyValueCheckSettings.T03_SMACrossOverSLTPTest)
            {
                ManageOrdersMACrossOverSLTPTest(order);
            }
        }

        private void ManageOrdersMACrossOverSLTPTest(Order order)
        {
            if (order.Name == EnterLongName)
            {
                _stop = order.FillPrice - TickSize * 50;
                PlaceStopMarketOrder(true);

                _profit = order.FillPrice + TickSize * 50;
                PlaceProfitLimitOrder(true);

            }

            if (order.Name == EnterShortName)
            {

                _stop = order.FillPrice + TickSize * 50;
                PlaceStopMarketOrder(false);

                _profit = order.FillPrice - TickSize * 50;
                PlaceProfitLimitOrder(false);
            }

            if (order.Name == ExitLongStopName || order.Name == ExitShortStopName)
            {
                if (_exitProfitOrder != null)
                    CancelOrder(_exitProfitOrder);
            }

            if (order.Name == ExitLongProfitName || order.Name == ExitShortProfitName)
            {
                if (_exitStopOrder != null)
                    CancelOrder(_exitStopOrder);
            }

        }


        #endregion


    }
}
