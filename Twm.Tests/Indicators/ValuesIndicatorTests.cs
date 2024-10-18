using System;
using System.Collections.Generic;
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
    public class ValuesIndicatorTests
    {
        private const string INDICATOR_TYPE = "BlankTest";
        private const string TEST_INSTRUMENT_SYMBOL = "/ES:XCME";
        private const int DATA_SERIES_VALUE = 1;
        private const DataSeriesType DATA_SERIES_TYPE = DataSeriesType.Minute;
        private readonly DateTime PERIOD_START = new DateTime(2022, 07, 4);
        private const int PERIOD_DAYS = 9;

        private DataCalcContext _dataCalcContext;
        private DataSeriesParamsEmpty _dataSeriesParams;
        private IndicatorBase _indicator;

        #region Init

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            await CreateDataSeriesParams();
            CreateDataCalcContext();
            await LoadCandleData(new CancellationTokenSource().Token);
            CreateTestIndicator(INDICATOR_TYPE);
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

        #region Value Check Tests

        [Test]
        [Order(1)]
        [Category("Indicator value check")]
        public async Task T01_SimpleEmaValuesCheck()
        {
            _indicator.Tag = IndicatorValueCheckSettings.T01_SimpleEMAValuesCheck;
            
            await _dataCalcContext.Execute();

            var fileDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "SimpleEMA_0407_1307.txt");
            string[] arr = File.ReadAllLines(fileDataPath);
            var correctValues = Array.ConvertAll(arr, i => Convert.ToDouble(i, CultureInfo.InvariantCulture)).ToList();

            if (_indicator.Tag is List<double> indicatorValues)
            {
                var failed = false;
                int failCount = 0;
                double maxDiff = 0;
                int firstFail = 0;

                //we use minus 1 because in AD live bar is also included
                if (indicatorValues.Count-1 != correctValues.Count)
                {
                    TestContext.WriteLine("NT Values count: " + correctValues.Count + ". AD Values Count: " + indicatorValues.Count);
                    Assert.Fail("EMA Values Check Failed. Values count don't match.");
                }

                for (int i = 0; i < indicatorValues.Count-1; i++)
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
                    TestContext.WriteLine("EMA Values Check Failed on " + failCount + " bars out of "+ indicatorValues.Count + " bars. " +
                                          "Largest value difference: "+ maxDiff + " First bar fail: "+ firstFail);

                    Assert.Fail("EMA Values Check Failed. Values do not match.");
                }
            }
        }


        [Test]
        [Order(2)]
        [Category("Indicator value check")]
        public async Task T02_EMACalculatedUsingSMAValuesCheck()
        {
            _indicator.Tag = IndicatorValueCheckSettings.T02_EMACalculatedUsingSMAValuesCheck;

            await _dataCalcContext.Execute();

            
            var fileDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "EmaInputSma_0407_1307.txt");
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
                    TestContext.WriteLine("NT Values count: " + correctValues.Count + ". AD Values Count: " + indicatorValues.Count);
                    Assert.Warn("SMA Values Check Failed. Values count don't match.");
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
                    TestContext.WriteLine("SMA Values Check Failed on " + failCount + " bars out of " + indicatorValues.Count + " bars. " +
                                          "Largest value difference: " + maxDiff + " First bar fail: " + firstFail);

                    Assert.Fail("SMA Values Check Failed. Values do not match.");
                }
            }
        }

        [Test]
        [Order(3)]
        [Category("Indicator value check")]
        public async Task T03_SMAValuesCheck()
        {
            _indicator.Tag = IndicatorValueCheckSettings.T03_SMAValuesCheck;

            await _dataCalcContext.Execute();

            var fileDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "SmaValues_0407_1307.txt");
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
                    TestContext.WriteLine("NT Values count: " + correctValues.Count + ". AD Values Count: " + indicatorValues.Count);
                    Assert.Warn("SMA Values Check Failed. Values count don't match.");
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
                    TestContext.WriteLine("SMA Values Check Failed on " + failCount + " bars out of " + indicatorValues.Count + " bars. " +
                                          "Largest value difference: " + maxDiff + " First bar fail: " + firstFail);

                    Assert.Fail("SMA Values Check Failed. Values do not match.");
                }
            }
        }

        [Test]
        [Order(4)]
        [Category("Indicator value check")]
        public async Task T04_EmaUsingLocalSmaValuesCheck()
        {
            _indicator.Tag = IndicatorValueCheckSettings.T04_EMACalculatedUsingLocalSMAValuesCheck;

            await _dataCalcContext.Execute();

            var fileDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "EmaLocalSma_0407_1307.txt");
            
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
                    TestContext.WriteLine("NT Values count: " + correctValues.Count + ". AD Values Count: " + indicatorValues.Count);
                    Assert.Fail("EMA Values Check Failed. Values count don't match.");
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
                    TestContext.WriteLine("EMA Values Check Failed on " + failCount + " bars out of " + indicatorValues.Count + " bars. " +
                                          "Largest value difference: " + maxDiff + " First bar fail: " + firstFail);

                    Assert.Fail("EMA Values Check Failed. Values do not match.");
                }
            }
        }

        [Test]
        [Order(5)]
        [Category("Indicator value check")]
        public async Task T05_CalculatedEmaIsInputToSmaValuesCheck()
        {
            _indicator.Tag = IndicatorValueCheckSettings.T05_CalculatedEMAIsInputToSMAValuesCheck;

            await _dataCalcContext.Execute();

            var fileDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "CalculatedEmaInputToSma_0407-1307.txt");
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
                    TestContext.WriteLine("NT Values count: " + correctValues.Count + ". AD Values Count: " + indicatorValues.Count);
                    Assert.Fail("EMA Values Check Failed. Values count don't match.");
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
                    TestContext.WriteLine("EMA Values Check Failed on " + failCount + " bars out of " + indicatorValues.Count + " bars. " +
                                          "Largest value difference: " + maxDiff + " First bar fail: " + firstFail);

                    Assert.Fail("EMA Values Check Failed. Values do not match.");
                }
            }
        }


        [Test]
        [Order(6)]
        [Category("Indicator value check")]
        public async Task T06_MacdDiffValuesCheckTest()
        {
            _indicator.Tag = IndicatorValueCheckSettings.T06_MacdDiffValuesCheck;

            await _dataCalcContext.Execute();

            var fileDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "MacdDiff_0407_1307.txt");
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
                    TestContext.WriteLine("NT Values count: " + correctValues.Count + ". AD Values Count: " + indicatorValues.Count);
                    Assert.Fail("MACD DIFF Values Check Failed. Values count don't match.");
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
                    TestContext.WriteLine("MACD DIFF Values Check Failed on " + failCount + " bars out of " + indicatorValues.Count + " bars. " +
                                          "Largest value difference: " + maxDiff + " First bar fail: " + firstFail);

                    Assert.Fail("MACD DIFF Values Check Failed. Values do not match.");
                }
            }
        }


        [Test]
        [Order(7)]
        [Category("Indicator value check")]
        public async Task T07_WMAValuesCheckTest()
        {
            _indicator.Tag = IndicatorValueCheckSettings.T07_WMAValuesCheck;

            await _dataCalcContext.Execute();

            var fileDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "WMA_0407_1307.txt");
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
                    TestContext.WriteLine("NT Values count: " + correctValues.Count + ". AD Values Count: " + indicatorValues.Count);
                    Assert.Fail("WMA Values Check Failed. Values count don't match.");
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
                    TestContext.WriteLine("WMA Values Check Failed on " + failCount + " bars out of " + indicatorValues.Count + " bars. " +
                                          "Largest value difference: " + maxDiff + " First bar fail: " + firstFail);

                    Assert.Fail("WMA Values Check Failed. Values do not match.");
                }
            }
        }


        [Test]
        [Order(9)]
        [Category("Indicator value check")]
        public async Task T09_TEMAValuesCheckTest()
        {
            _indicator.Tag = IndicatorValueCheckSettings.T09_TEMAValuesCheck;

            await _dataCalcContext.Execute();

            var fileDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "TEMA_0407_1307.txt");
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
                    TestContext.WriteLine("NT Values count: " + correctValues.Count + ". AD Values Count: " + indicatorValues.Count);
                    Assert.Fail("TEMA Values Check Failed. Values count don't match.");
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
                    TestContext.WriteLine("TEMA Values Check Failed on " + failCount + " bars out of " + indicatorValues.Count + " bars. " +
                                          "Largest value difference: " + maxDiff + " First bar fail: " + firstFail);

                    Assert.Fail("TEMA Values Check Failed. Values do not match.");
                }
            }
        }

        [Test]
        [Order(10)]
        [Category("Indicator value check")]
        public async Task T10_TMAValuesCheckTest()
        {
            _indicator.Tag = IndicatorValueCheckSettings.T10_TMAValuesCheck;

            await _dataCalcContext.Execute();

            var fileDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "TMA_0407_1307.txt");
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
                    TestContext.WriteLine("NT Values count: " + correctValues.Count + ". AD Values Count: " + indicatorValues.Count);
                    Assert.Fail("TMA Values Check Failed. Values count don't match.");
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
                    TestContext.WriteLine("TMA Values Check Failed on " + failCount + " bars out of " + indicatorValues.Count + " bars. " +
                                          "Largest value difference: " + maxDiff + " First bar fail: " + firstFail);

                    Assert.Fail("TMA Values Check Failed. Values do not match.");
                }
            }
        }


        

        [Test]
        [Order(11)]
        [Category("Indicator value check")]
        public async Task T11_HMAValuesCheck()
        {
            _indicator.Tag = IndicatorValueCheckSettings.T11_HMAValuesCheck;

            await _dataCalcContext.Execute();

            var fileDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "HMA_0407_1307.txt");
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
                    TestContext.WriteLine("NT Values count: " + correctValues.Count + ". AD Values Count: " + indicatorValues.Count);
                    Assert.Fail("HMA Values Check Failed. Values count don't match.");
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
                    TestContext.WriteLine("HMA Values Check Failed on " + failCount + " bars out of " + indicatorValues.Count + " bars. " +
                                          "Largest value difference: " + maxDiff + " First bar fail: " + firstFail);

                    Assert.Fail("HMA Values Check Failed. Values do not match.");
                }
            }
        }


        #endregion
    }
}
