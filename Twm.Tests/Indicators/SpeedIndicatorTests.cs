using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Twm.Chart.Interfaces;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.Core.DataCalc;
using Twm.Core.Enums;
using Twm.Core.Interfaces;
using Twm.Core.Managers;
using Twm.Core.Tests.Enums;
using Twm.Core.ViewModels.DataSeries;
using NUnit.Framework;

namespace Twm.Tests.Indicators
{
    public class SpeedIndicatorTests
    {
        private const string INDICATOR_TYPE = "BlankTest";
        private const string TEST_INSTRUMENT_SYMBOL = "/ES:XCME";
        private const int DATA_SERIES_VALUE = 1;
        private const DataSeriesType DATA_SERIES_TYPE = DataSeriesType.Minute;
        private readonly DateTime PERIOD_START = new DateTime(2022, 06, 16);
        private const int PERIOD_DAYS = 27;

        private const double DiffWarnBenchMark = 1.5;
        private const double DiffFailBenchMark = 10;

        private double BlankExecutionNT = 0.0009972;
        private double ValueAssignmentNT = 0.0009984;
        private double EMACalculatedNT = 0.0029851;
        private double EMACalculatedUsingSMAInputNT = 0.0050268;
        private double EMACalculatedUsingLocalSMAInputNT = 0.0259309;
        private double CalculatedEMAIsInputToSMA = 0.0253769;
        private double MacdDiffSpeedTest = 0.0069819;
        private double WMASpeed = 0.0098515;
        private double TMASpeed = 0.0468749;
        private double TEMASpeed = 0.0169559;
        private double VWMASpeed = 0.0079808;
        private double HMASpeed = 0.0219123;

        private DataCalcContext _dataCalcContext;
        private DataSeriesParamsEmpty _dataSeriesParams;
        private IndicatorBase _indicator;

        private Stopwatch _stopWatch;


        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _stopWatch = new Stopwatch();
            await CreateDataSeriesParams();
            CreateDataCalcContext();
            await LoadCandleData(new CancellationTokenSource().Token);
            CreateTestIndicator(INDICATOR_TYPE);
        }

        [SetUp]
        public void TestSetup()
        {
            _dataCalcContext.CancelCalc();
            _dataCalcContext.CreateToken();
        }

        #region Init

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

        private void CreateTestIndicator(string indicatorName)
        {
            try
            {
                var indicatorType = BuildController.Instance.GetIndicatorType(indicatorName);
                _indicator = _dataCalcContext.CreateIndicator(indicatorType, null, true);
                _dataCalcContext.Indicators.Add(_indicator);
                _indicator.IsTemporary = false;
            }
            catch (Exception e)
            {
                Assert.Fail($"Create indicator {indicatorName} fail: " + e.Message);
            }
        }

        #endregion

        #region Speed Tests

        [Test]
        [Order(1)]
        [Category("Indicator speed test")]
        public async Task T01_BlankExecutionTest()
        {
            _indicator.Tag = IndicatorSpeedSettings.T01_BlankExecution;
            _stopWatch.Start();
            await _dataCalcContext.Execute();
            _stopWatch.Stop();

            var dif = ProcessIndicatorExecutionTime(_indicator, BlankExecutionNT);

            if (dif > DiffWarnBenchMark && dif < DiffFailBenchMark)
            {
                Assert.Warn("Indicator calculation time notice: " + dif);
                return;
            }

            if (dif > DiffFailBenchMark)
                Assert.Fail("Unacceptable indicator calculation time: " + dif);
        }


        [Test]
        [Order(2)]
        [Category("Indicator speed test")]
        public async Task T02_ValueAssignmentTest()
        {
            _indicator.Tag = IndicatorSpeedSettings.T02_ValueAssignmentTest;
            _stopWatch.Start();
            await _dataCalcContext.Execute();
            _stopWatch.Stop();

            var dif = ProcessIndicatorExecutionTime(_indicator, ValueAssignmentNT);

            if (dif > DiffWarnBenchMark && dif < DiffFailBenchMark)
            {
                Assert.Warn("Indicator calculation time notice: " + dif);
                return;
            }

            if (dif > DiffFailBenchMark)
                Assert.Fail("Unacceptable indicator calculation time: " + dif);
        }


        [Test]
        [Order(3)]
        [Category("Indicator speed test")]
        public async Task T03_SingleEmaCalculatedTest()
        {
            _indicator.Tag = IndicatorSpeedSettings.T03_EMACalculated;
            _stopWatch.Start();
            await _dataCalcContext.Execute();
            _stopWatch.Stop();

            var dif = ProcessIndicatorExecutionTime(_indicator, EMACalculatedNT);

            if (dif > DiffWarnBenchMark && dif < DiffFailBenchMark)
            {
                Assert.Warn("Indicator calculation time notice: " + dif);
                return;
            }

            if (dif > DiffFailBenchMark)
                Assert.Fail("Unacceptable indicator calculation time: " + dif);
        }

        [Test]
        [Order(4)]
        [Category("Indicator speed test")]
        public async Task T04_EMACalculatedUsingSMATest()
        {
            _indicator.Tag = IndicatorSpeedSettings.T04_EMACalculatedUsingSMAInput;
            _stopWatch.Start();
            await _dataCalcContext.Execute();
            _stopWatch.Stop();

            var dif = ProcessIndicatorExecutionTime(_indicator, EMACalculatedUsingSMAInputNT);

            if (dif > DiffWarnBenchMark && dif < DiffFailBenchMark)
            {
                Assert.Warn("Indicator calculation time notice: " + dif);
                return;
            }

            if (dif > DiffFailBenchMark)
                Assert.Fail("Unacceptable indicator calculation time: " + dif);
        }


        [Test]
        [Order(5)]
        [Category("Indicator speed test")]
        public async Task T05_EMACalculatedUsingLocalSMATest()
        {
            _indicator.Tag = IndicatorSpeedSettings.T05_EMACalculatedUsingLocalSMA;
            _stopWatch.Start();
            await _dataCalcContext.Execute();
            _stopWatch.Stop();

            var dif = ProcessIndicatorExecutionTime(_indicator, EMACalculatedUsingLocalSMAInputNT);

            if (dif > DiffWarnBenchMark && dif < DiffFailBenchMark)
            {
                Assert.Warn("Indicator calculation time notice: " + dif);
                return;
            }

            if (dif > DiffFailBenchMark)
                Assert.Fail("Unacceptable indicator calculation time: " + dif);
        }


        [Test]
        [Order(6)]
        [Category("Indicator speed test")]
        public async Task T06_CalculatedEMAIsInputToSMATest()
        {
            _indicator.Tag = IndicatorSpeedSettings.T06_CalculatedEMAIsInputToSMA;
            _stopWatch.Start();
            await _dataCalcContext.Execute();
            _stopWatch.Stop();

            var dif = ProcessIndicatorExecutionTime(_indicator, CalculatedEMAIsInputToSMA);

            if (dif > DiffWarnBenchMark && dif < DiffFailBenchMark)
            {
                Assert.Warn("Indicator calculation time notice: " + dif);
                return;
            }

            if (dif > DiffFailBenchMark)
                Assert.Fail("Unacceptable indicator calculation time: " + dif);
        }

        [Test]
        [Order(7)]
        [Category("Indicator speed test")]
        public async Task T07_MacdDiffSpeedTest()
        {
            _indicator.Tag = IndicatorSpeedSettings.T07_MacdDiff;
            _stopWatch.Start();
            await _dataCalcContext.Execute();
            _stopWatch.Stop();

            var dif = ProcessIndicatorExecutionTime(_indicator, MacdDiffSpeedTest);

            if (dif > DiffWarnBenchMark && dif < DiffFailBenchMark)
            {
                Assert.Warn("Indicator calculation time notice: " + dif);
                return;
            }

            if (dif > DiffFailBenchMark)
                Assert.Fail("Unacceptable indicator calculation time: " + dif);
        }

        [Test]
        [Order(8)]
        [Category("Indicator speed test")]
        public async Task T08_WMASpeedTest()
        {
            _indicator.Tag = IndicatorSpeedSettings.T08_WMASpeedTest;
            _stopWatch.Start();
            await _dataCalcContext.Execute();
            _stopWatch.Stop();

            var dif = ProcessIndicatorExecutionTime(_indicator, WMASpeed);

            if (dif > DiffWarnBenchMark && dif < DiffFailBenchMark)
            {
                Assert.Warn("Indicator calculation time notice: " + dif);
                return;
            }

            if (dif > DiffFailBenchMark)
                Assert.Fail("Unacceptable indicator calculation time: " + dif);
        }

        [Test]
        [Order(9)]
        [Category("Indicator speed test")]
        public async Task T09_VWMASpeedTest()
        {
            _indicator.Tag = IndicatorSpeedSettings.T09_VWMASpeedTest;
            _stopWatch.Start();
            await _dataCalcContext.Execute();
            _stopWatch.Stop();

            var dif = ProcessIndicatorExecutionTime(_indicator, VWMASpeed);

            if (dif > DiffWarnBenchMark && dif < DiffFailBenchMark)
            {
                Assert.Warn("Indicator calculation time notice: " + dif);
                return;
            }

            if (dif > DiffFailBenchMark)
                Assert.Fail("Unacceptable indicator calculation time: " + dif);
        }

        [Test]
        [Order(10)]
        [Category("Indicator speed test")]
        public async Task T10_TEMASpeedTest()
        {
            _indicator.Tag = IndicatorSpeedSettings.T10_TEMASpeedTest;
            _stopWatch.Start();
            await _dataCalcContext.Execute();
            _stopWatch.Stop();

            var dif = ProcessIndicatorExecutionTime(_indicator, TEMASpeed);

            if (dif > DiffWarnBenchMark && dif < DiffFailBenchMark)
            {
                Assert.Warn("Indicator calculation time notice: " + dif);
                return;
            }

            if (dif > DiffFailBenchMark)
                Assert.Fail("Unacceptable indicator calculation time: " + dif);
        }


        [Test]
        [Order(11)]
        [Category("Indicator speed test")]
        public async Task T11_TMASpeedTest()
        {
            _indicator.Tag = IndicatorSpeedSettings.T11_TMASpeedTest;
            _stopWatch.Start();
            await _dataCalcContext.Execute();
            _stopWatch.Stop();

            var dif = ProcessIndicatorExecutionTime(_indicator, TMASpeed);

            if (dif > DiffWarnBenchMark && dif < DiffFailBenchMark)
            {
                Assert.Warn("Indicator calculation time notice: " + dif);
                return;
            }

            if (dif > DiffFailBenchMark)
                Assert.Fail("Unacceptable indicator calculation time: " + dif);
        }

        

        [Test]
        [Order(12)]
        [Category("Indicator speed test")]
        public async Task T12_HMASpeedTest()
        {
            _indicator.Tag = IndicatorSpeedSettings.T12_HMASpeedTest;
            _stopWatch.Start();
            await _dataCalcContext.Execute();
            _stopWatch.Stop();

            var dif = ProcessIndicatorExecutionTime(_indicator, HMASpeed);

            if (dif > DiffWarnBenchMark && dif < DiffFailBenchMark)
            {
                Assert.Warn("Indicator calculation time notice: " + dif);
                return;
            }

            if (dif > DiffFailBenchMark)
                Assert.Fail("Unacceptable indicator calculation time: " + dif);
        }




        #endregion

        #region Helpers

        private double ProcessIndicatorExecutionTime(IndicatorBase indicator, double timeSample)
        {
            TestContext.WriteLine("NT OnBarUpdate execution time {0} s", timeSample);

            double dif = double.MaxValue;

            if (indicator.Tag is double onBarUpdateExecutionTime)
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

            TestContext.WriteLine("Twm All Execution time {0} s",
                _stopWatch.ElapsedMilliseconds / 1000.0);

            TestContext.WriteLine("Bars in test {0}", _dataCalcContext.Candles.Count);
            return dif;
        }

        #endregion
    }
}