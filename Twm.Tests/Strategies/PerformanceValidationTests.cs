using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Twm.Chart.Interfaces;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Performance;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Core.Interfaces;
using Twm.Core.Managers;
using Twm.Core.Tests.Enums;
using Twm.Core.ViewModels.DataSeries;
using AutoMapper.Configuration.Annotations;
using NUnit.Framework;
using Xceed.Wpf.Toolkit.Core.Input;

namespace Twm.Tests.Strategies
{
    public class PerformanceValidationTests
    {
        private const string STRATEGY_TYPE = "TestStrategy";
        private const string TEST_INSTRUMENT_SYMBOL = "/ES:XCME";
        private const int DATA_SERIES_VALUE = 1;
        private const DataSeriesType DATA_SERIES_TYPE = DataSeriesType.Minute;
        private readonly DateTime PERIOD_START = new DateTime(2022, 07, 4);
        private const int PERIOD_DAYS = 9;

        private DataCalcContext _dataCalcContext;
        private DataSeriesParamsEmpty _dataSeriesParams;
        private StrategyBase _strategy;

        #region Init

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            await CreateDataSeriesParams();
            CreateDataCalcContext();
            await LoadCandleData(new CancellationTokenSource().Token);
            CreateTestStrategy(STRATEGY_TYPE);
        }

        /*[SetUp]
        public async Task EachTestSetUp()
        {
            _dataCalcContext.CreateToken();
        }*/



        private async Task CreateDataSeriesParams()
        {
            try
            {
                _dataSeriesParams = new DataSeriesParamsEmpty();
                //var instrument = await Session.Instance.GetInstrument(TEST_INSTRUMENT_SYMBOL);
                _dataSeriesParams.Instrument = new Model.Model.Instrument() { Symbol = TEST_INSTRUMENT_SYMBOL, PriceIncrements= "0.25", Multiplier = 50};
                _dataSeriesParams.DataSeriesValue = DATA_SERIES_VALUE;
                _dataSeriesParams.DataSeriesType = DATA_SERIES_TYPE;
                _dataSeriesParams.PeriodStart = PERIOD_START;
                _dataSeriesParams.DaysToLoad = PERIOD_DAYS;
                _dataSeriesParams.PeriodEnd = _dataSeriesParams.PeriodStart.AddDays(_dataSeriesParams.DaysToLoad);
            }
            catch (Exception e)
            {
                Assert.Fail("Data series params init fail: " + e.Message);

            }
        }

        private void CreateDataCalcContext()
        {

            try
            {
                _dataCalcContext = new DataCalcContext { IsOptimization = true };
                _dataCalcContext.SetParams(_dataSeriesParams);
            }
            catch (Exception e)
            {
                Assert.Fail("DataCalcContext data series params set fail: " + e.Message);

            }
        }

        private async Task LoadCandleData(CancellationToken token)
        {
            try
            {
                var historicalDataManager = new HistoricalDataManager(null, Path.Combine(Environment.CurrentDirectory,"Twm.Tests", "InputData"));
                var historicalCandles = await historicalDataManager.GetDataFromFiles(_dataSeriesParams);

                if (token.IsCancellationRequested)
                    Assert.Fail("DataCalcContext load test data cancel");

                var candleSource = new ObservableCollection<ICandle>();
                foreach (var historicalCandle in historicalCandles)
                {
                    if (token.IsCancellationRequested)
                        Assert.Fail("DataCalcContext load test data cancel");

                    candleSource.Add(Session.Instance.Mapper.Map<IHistoricalCandle, ICandle>(historicalCandle));
                }

                _dataCalcContext.Candles = candleSource;
            }
            catch (Exception e)
            {
                Assert.Fail("DataCalcContext load test data fail: " + e.Message);

            }



        }

        private void CreateTestStrategy(string strategyName)
        {
            try
            {
                var strategyType = BuildController.Instance.GetStrategyType(strategyName);
                _strategy = _dataCalcContext.CreateStrategy(strategyType, null, true);
                _dataCalcContext.Strategies.Add(_strategy);
                _strategy.IsTemporary = true;
            }
            catch (Exception e)
            {
                Assert.Fail($"Create indicator {strategyName} fail: " + e.Message);
            }

        }

        #endregion

        #region Performance Value Check Tests

        private struct PerformanceValue
        {
            public string ValueName { get; set; }
            public double NTValueTotal { get; set; }
            public double ADValueTotal { get; set; }
            public double NTValueLong { get; set; }
            public double ADValueLong { get; set; }
            public double NTValueShort { get; set; }
            public double ADValueShort { get; set; }
        }


        private PerformanceValue CreatePerformanceValue(AnalyticalFeature af, double checkValue)
        {
            var performanceValue = new PerformanceValue()
            {
                ValueName = af.Description(),
                ADValueTotal = _strategy.SystemPerformance.Summary.GetValue(af),
                NTValueTotal = checkValue
            };
            return performanceValue;
        }

        [Test]
        [Order(1)]
        [Category("Strategy execution")]
        public async Task T01_MaCrossOverTest()
        {
            _strategy.Tag = StrategyValueCheckSettings.T01_SimpleMACrossOverStrategy;
            _strategy.Enabled = true;
            _strategy.IsTemporary = false;

            await _dataCalcContext.ExecuteStrategy(_dataCalcContext.CancellationTokenSourceCalc);
            await _strategy.SystemPerformance.Calculate(new CancellationToken());

            var performanceValues = new List<PerformanceValue>();
            if (_strategy.SystemPerformance?.Summary != null)
            {
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.NetProfitSum, -1588));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.ProfitFactor, 0.94));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.MaxDrawDown, -10050.00));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.MaxConsLoss, 11));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.MaxConsWins, 11));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.Trades, 158));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.LargestLoosingTrade, -1125));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.LargestWinningTrade, 2825));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.AverageTradeProfit, -10));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.MaxMae, -1350));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.AverageLoosingTrade, -229));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.AverageWinningTrade, 487));
            }
            else
            {
                Assert.Fail("Total performance is empty");
            }

            var isPass = true;
            foreach (var value in performanceValues)
            {
                TestContext.WriteLine("Total Performance value mismatch. Value name: " + value.ValueName +
                                      ". NT: " + value.NTValueTotal + " AD: " + value.ADValueTotal);

                if (value.ADValueTotal != value.NTValueTotal)
                {
                    isPass = false;
                }
            }

            if (!isPass)
                Assert.Fail("Total performance value mismatch");
        }

        [Test]
        [Order(2)]
        [Category("Strategy execution")]
        public async Task T02_MaCrossOverLimitOrderEntriesTest()
        {
            _strategy.Tag = StrategyValueCheckSettings.T02_MACrossOverLimitOrderEntry;
            _strategy.Enabled = true;
            _strategy.IsTemporary = false;

            await _dataCalcContext.ExecuteStrategy(_dataCalcContext.CancellationTokenSourceCalc);
            await _strategy.SystemPerformance.Calculate(new CancellationToken());

            var performanceValues = new List<PerformanceValue>();
            if (_strategy.SystemPerformance?.Summary != null)
            {
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.NetProfitSum, -475));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.ProfitFactor, 0.95));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.MaxDrawDown, -3812.00));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.MaxConsLoss, 7));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.MaxConsWins, 4));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.Trades, 72));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.LargestLoosingTrade, -1112));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.LargestWinningTrade, 1675));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.AverageTradeProfit, -7));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.MaxMae, -1187.5));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.AverageLoosingTrade, -219));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.AverageWinningTrade, 436));
            }
            else
            {
                Assert.Fail("Total performance is empty");
            }

            var isPass = true;
            foreach (var value in performanceValues)
            {
                TestContext.WriteLine("Total Performance value mismatch. Value name: " + value.ValueName +
                                      ". NT: " + value.NTValueTotal + " AD: " + value.ADValueTotal);

                if (value.ADValueTotal != value.NTValueTotal)
                {
                    isPass = false;
                }
            }

            if (!isPass)
                Assert.Fail("Total performance value mismatch");
        }


        [Test]
        [Order(3)]
        [Category("Strategy execution")]
        public async Task T03_SMACrossOverSLTPTest()
        {
            _strategy.Tag = StrategyValueCheckSettings.T03_SMACrossOverSLTPTest;

            _strategy.Enabled = true;
            _strategy.IsTemporary = false;


            await _dataCalcContext.ExecuteStrategy(null);
            await _strategy.SystemPerformance.Calculate(new CancellationToken());

            var performanceValues = new List<PerformanceValue>();
            if (_strategy.SystemPerformance?.Summary != null)
            {
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.NetProfitSum, -1875));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.ProfitFactor, 0.89));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.MaxDrawDown, -4375.00));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.MaxConsLoss, 5));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.MaxConsWins, 4));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.Trades, 53));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.LargestLoosingTrade, -625));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.LargestWinningTrade, 625));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.AverageTradeProfit, -35));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.MaxMae, -612.5));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.AverageLoosingTrade, -625));
                performanceValues.Add(CreatePerformanceValue(AnalyticalFeature.AverageWinningTrade, 625));
            }
            else
            {
                Assert.Fail("Total performance is empty");
            }

            var isPass = true;
            foreach (var value in performanceValues)
            {
                TestContext.WriteLine("Total Performance value mismatch. Value name: " + value.ValueName +
                                      ". NT: " + value.NTValueTotal + " AD: " + value.ADValueTotal);

                if (value.ADValueTotal != value.NTValueTotal)
                {
                    isPass = false;
                }
            }

            if (!isPass)
                Assert.Fail("Total performance value mismatch");
        }

        #endregion
    }
}
