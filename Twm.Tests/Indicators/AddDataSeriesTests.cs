using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
    public class AddDataSeriesTests
    {
        private const string INDICATOR_TYPE = "DataTest";
        private const string TEST_INSTRUMENT_SYMBOL = "/ES:XCME";
        private const int DATA_SERIES_VALUE = 1;
        private const DataSeriesType DATA_SERIES_TYPE = DataSeriesType.Minute;
        private readonly DateTime PERIOD_START = new DateTime(2022, 06, 16);
        private const int PERIOD_DAYS = 27;

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
                var instrument = await Session.Instance.GetInstrument(TEST_INSTRUMENT_SYMBOL);
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
                
                var historicalDataManager = new HistoricalDataManager(null, Path.Combine(Environment.CurrentDirectory, "Twm.Tests", "InputData"));
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

        [Test]
        [Order(1)]
        [Category("Add Data Series Indicator Tests")]
        public async Task T01_NQ1MinAddDataSeriesCheck()
        {
            _indicator.Tag = DataSeriesTestSettings.T01_NQ1MinAddDataSeriesCheck;

            await _dataCalcContext.Execute();

            var fileDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "NQ1minClosePrices_1606_1307.txt");
            string[] arr = File.ReadAllLines(fileDataPath);
            var correctValues = Array.ConvertAll(arr, i => Convert.ToDouble(i, CultureInfo.InvariantCulture)).ToList();

            if (_indicator.Tag is List<double> indicatorValues)
            {
                var failed = false;
                int failCount = 0;
                double maxDiff = 0;
                int firstFail = 0;

                //we use minus 1 because in AD live bar is also included
                if (indicatorValues.Count - 1 != correctValues.Count)
                {
                    TestContext.WriteLine("Values count: " + correctValues.Count + ". AD Values Count: " + indicatorValues.Count);
                    Assert.Fail("Values Check Failed. Values count don't match.");
                }

                for (int i = 0; i < indicatorValues.Count - 1; i++)
                {
                    var v1 = correctValues[i];
                    var v2 = indicatorValues[i];
                    var dif = Math.Abs(correctValues[i] - indicatorValues[i]);

                    if (dif > 0.01)
                    {
                        failed = true;
                        failCount++;

                        if (dif > maxDiff)
                        {
                            maxDiff = dif;
                        }

                        if (firstFail == 0)
                            firstFail = i;
                    }
                }

                if (failed)
                {
                    TestContext.WriteLine("Values Check Failed on " + failCount + " bars out of " + indicatorValues.Count + " bars. " +
                                          "Largest value difference: " + maxDiff + " First bar fail: " + firstFail);

                    Assert.Fail("Values Check Failed. Values do not match.");
                }
            }
        }
    }
}
