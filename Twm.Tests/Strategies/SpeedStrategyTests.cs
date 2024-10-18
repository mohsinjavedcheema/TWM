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
using Twm.Core.Interfaces;
using Twm.Core.Managers;
using Twm.Core.Tests.Enums;
using Twm.Core.ViewModels.DataSeries;
using AutoMapper.Configuration.Annotations;
using NUnit.Framework;
using Xceed.Wpf.Toolkit.Core.Input;

namespace Twm.Tests.Strategies
{
    public class SpeedStrategyTests
    {
        private const string STRATEGY_TYPE = "TestStrategy";
        private const string TEST_INSTRUMENT_SYMBOL = "/ES:XCME";
        private const int DATA_SERIES_VALUE = 1;
        private const DataSeriesType DATA_SERIES_TYPE = DataSeriesType.Minute;
        private readonly DateTime PERIOD_START = new DateTime(2022, 06, 16);
        private const int PERIOD_DAYS = 27;

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

        private async Task CreateDataSeriesParams()
        {
            try
            {
                _dataSeriesParams = new DataSeriesParamsEmpty();
                //var instrument = await Session.Instance.GetInstrument(TEST_INSTRUMENT_SYMBOL);
                _dataSeriesParams.Instrument = new Model.Model.Instrument() { Symbol = TEST_INSTRUMENT_SYMBOL, PriceIncrements= "0.25" };
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

        private double BlankStrategyExecutionTime = 0.0019666;
        private double SimpleMACrossOverStrategyExecutionTime = 0.0270258;
        private double MaCrossOverLimitOrderEntries = 0.0206375;
        private double MaCrossOverSLTP = 0.0179518;

        private const double DiffWarnBenchMark = 1.5;
        private const double DiffFailBenchMark = 10;

        [Test]
        [Order(1)]
        [Category("Strategy Speed Test")]
        public async Task T01_BlankExecutionTest()
        {
            _strategy.Tag = StrategySpeedSettings.T01_BlankExecution;

            _strategy.Enabled = true;
            _strategy.IsTemporary = false;

            await _dataCalcContext.ExecuteStrategy(null);

            var dif = ProcessStrategyExecutionTime(_strategy, BlankStrategyExecutionTime);

            if (dif > DiffWarnBenchMark && dif < DiffFailBenchMark)
            {
                Assert.Warn("Strategy calculation time notice: " + dif);
                return;
            }

            if (dif > DiffFailBenchMark)
                Assert.Fail("Unacceptable strategy calculation time: " + dif);
        }

        [Test]
        [Order(2)]
        [Category("Strategy Speed Test")]
        public async Task T02_SimpleMACrossOverStrategyTest()
        {
            _strategy.Tag = StrategySpeedSettings.T02_SimpleMACrossOverStrategy;

            _strategy.Enabled = true;
            _strategy.IsTemporary = false;

            await _dataCalcContext.ExecuteStrategy(null);

            var dif = ProcessStrategyExecutionTime(_strategy, SimpleMACrossOverStrategyExecutionTime);

            if (dif > DiffWarnBenchMark && dif < DiffFailBenchMark)
            {
                Assert.Warn("Strategy calculation time notice: " + dif);
                return;
            }

            if (dif >= DiffFailBenchMark)
                Assert.Fail("Unacceptable strategy calculation time: " + dif);
        }

        [Test]
        [Order(3)]
        [Category("Strategy Speed Test")]
        public async Task T03_MaCrossOverLimitOrderEntriesTest()
        {
            _strategy.Tag = StrategySpeedSettings.T03_MACrossOverLimitOrderEntry;

            _strategy.Enabled = true;
            _strategy.IsTemporary = false;

            await _dataCalcContext.ExecuteStrategy(null);

            var dif = ProcessStrategyExecutionTime(_strategy, MaCrossOverLimitOrderEntries);

            if (dif > DiffWarnBenchMark && dif < DiffFailBenchMark)
            {
                Assert.Warn("Strategy calculation time notice: " + dif);
                return;
            }

            if (dif >= DiffFailBenchMark)
                Assert.Fail("Unacceptable strategy calculation time: " + dif);
        }

        [Test]
        [Order(3)]
        [Category("Strategy Speed Test")]
        public async Task T04_SMACrossOverSLTPTest()
        {
            _strategy.Tag = StrategySpeedSettings.T04_SMACrossOverSLTPTest;

            _strategy.Enabled = true;
            _strategy.IsTemporary = false;

            await _dataCalcContext.ExecuteStrategy(null);

            var dif = ProcessStrategyExecutionTime(_strategy, MaCrossOverSLTP);

            if (dif > DiffWarnBenchMark && dif < DiffFailBenchMark)
            {
                Assert.Warn("Strategy calculation time notice: " + dif);
                return;
            }

            if (dif >= DiffFailBenchMark)
                Assert.Fail("Unacceptable strategy calculation time: " + dif);
        }

        #endregion

        #region Helpers

        private double ProcessStrategyExecutionTime(StrategyBase strategy, double timeSample)
        {
            TestContext.WriteLine("NT OnBarUpdate execution time {0} s", timeSample);

            double dif = double.MaxValue;

            if (strategy.Tag is double onBarUpdateExecutionTime)
            {
                TestContext.WriteLine("Twm OnBarUpdate execution time {0} s",
                    Math.Round(onBarUpdateExecutionTime / 1000, 3));

                //если меньше 1 значит мы быстрее NT

                var executionTimeSec = onBarUpdateExecutionTime / 1000;
                dif = executionTimeSec / timeSample;
                dif = Math.Round(dif, 3);


                TestContext.WriteLine("Dif {0}",
                    dif);
            }

            TestContext.WriteLine("Bars in test {0}", _dataCalcContext.Candles.Count);
            return dif;
        }

        #endregion
    }
}
