using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Twm.Core.ViewModels.DataSeries;
using NUnit.Framework;

namespace Twm.Tests.DataSeriesTests
{
    public class DataTest_1min_ES_04_07_13_07
    {
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
        }

        private async Task CreateDataSeriesParams()
        {
            try
            {
                _dataSeriesParams = new DataSeriesParamsEmpty();
                //var instrument = await Session.Instance.GetInstrument(TEST_INSTRUMENT_SYMBOL);
                var instrument = new Model.Model.Instrument() { Symbol = TEST_INSTRUMENT_SYMBOL, PriceIncrements= "0.25" };
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
                var path = Path.Combine(Environment.CurrentDirectory, "Twm.Tests", "InputData");
                var historicalDataManager = new HistoricalDataManager(null,Path.Combine(Environment.CurrentDirectory, "Twm.Tests","InputData") );
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

        #endregion

        #region Candle Data Check

        [Test]
        [Order(1)]
        [Category("DataTest_1min_ES_04_07_13_07")]
        public void T02_CandlesClosePriceCheck()
        {
           
            var fileDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "ClosePrices_0407_1307.txt");
            string[] arr = File.ReadAllLines(fileDataPath);
            var ntValues = Array.ConvertAll(arr, i => Convert.ToDouble(i, CultureInfo.InvariantCulture)).ToList();

            var failed = false;
            int failCount = 0;
            int firstFail = 0;

            //we use minus 1 because in AD live bar is also included
            if (_dataCalcContext.Candles.Count - 1 != ntValues.Count)
            {
                TestContext.WriteLine("Candle close count match fail. NT: " + ntValues.Count + " AD: " + _dataCalcContext.Candles.Count);
                Assert.Fail("Candle close count match fail.");
            }


            for (int i = 0; i < _dataCalcContext.Candles.Count - 1; i++)
            {
                var close = _dataCalcContext.Candles[i].C;

                if (close != ntValues[i])
                {
                    failCount++;
                    failed = true;

                    if (firstFail == 0)
                        firstFail = i;
                }
            }

            if (failed)
            {
                TestContext.WriteLine("Candle close  data match fail. Failed " + failCount + " bars, out of " + _dataCalcContext.Candles.Count + " AD bars. " +
                                      "First bar fail: " + firstFail);

                Assert.Fail("Candle close data match fail.");
            }
        }

        [Test]
        [Order(2)]
        [Category("DataTest_1min_ES_04_07_13_07")]
        public void T02_CandlesOpenPriceCheck()
        {
            var fileDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "OpenPrices_0407_1307.txt");
            string[] arr = File.ReadAllLines(fileDataPath);
            var ntValues = Array.ConvertAll(arr, i => Convert.ToDouble(i, CultureInfo.InvariantCulture)).ToList();

            var failed = false;
            int failCount = 0;
            int firstFail = 0;

            //we use minus 1 because in AD live bar is also included
            if (_dataCalcContext.Candles.Count - 1 != ntValues.Count)
            {
                TestContext.WriteLine("Candle open count match fail. NT: " + ntValues.Count + " AD: " + _dataCalcContext.Candles.Count);
                Assert.Fail("Candle open count match fail.");
            }


            for (int i = 0; i < _dataCalcContext.Candles.Count - 1; i++)
            {
                var close = _dataCalcContext.Candles[i].O;

                if (close != ntValues[i])
                {
                    failCount++;
                    failed = true;

                    if (firstFail == 0)
                        firstFail = i;
                }
            }

            if (failed)
            {
                TestContext.WriteLine("Candle open data match fail. Failed " + failCount + " bars, out of " + _dataCalcContext.Candles.Count + " AD bars. " +
                                      "First bar fail: " + firstFail);

                Assert.Fail("Candle open data match fail.");
            }
        }

        [Test]
        [Order(3)]
        [Category("DataTest_1min_ES_04_07_13_07")]
        public void T03_CandlesHighPriceCheck()
        {
            var fileDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "HighPrices_0407_1307.txt");
            string[] arr = File.ReadAllLines(fileDataPath);
            var ntValues = Array.ConvertAll(arr, i => Convert.ToDouble(i, CultureInfo.InvariantCulture)).ToList();

            var failed = false;
            int failCount = 0;
            int firstFail = 0;

            //we use minus 1 because in AD live bar is also included
            if (_dataCalcContext.Candles.Count - 1 != ntValues.Count)
            {
                TestContext.WriteLine("Candle high count match fail. NT: " + ntValues.Count + " AD: " + _dataCalcContext.Candles.Count);
                Assert.Fail("Candle high count match fail.");
            }


            for (int i = 0; i < _dataCalcContext.Candles.Count - 1; i++)
            {
                var close = _dataCalcContext.Candles[i].H;

                if (close != ntValues[i])
                {
                    failCount++;
                    failed = true;

                    if (firstFail == 0)
                        firstFail = i;
                }
            }

            if (failed)
            {
                TestContext.WriteLine("Candle high data match fail. Failed " + failCount + " bars, out of " + _dataCalcContext.Candles.Count + " AD bars. " +
                                      "First bar fail: " + firstFail);

                Assert.Fail("Candle high data match fail.");
            }
        }


        [Test]
        [Order(4)]
        [Category("DataTest_1min_ES_04_07_13_07")]
        public void T04_CandlesLowPriceCheck()
        {
            var fileDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "LowPrices_0407_1307.txt");
            string[] arr = File.ReadAllLines(fileDataPath);
            var ntValues = Array.ConvertAll(arr, i => Convert.ToDouble(i, CultureInfo.InvariantCulture)).ToList();

            var failed = false;
            int failCount = 0;
            int firstFail = 0;

            //we use minus 1 because in AD live bar is also included
            if (_dataCalcContext.Candles.Count - 1 != ntValues.Count)
            {
                TestContext.WriteLine("Candle low count match fail. NT: " + ntValues.Count + " AD: " + _dataCalcContext.Candles.Count);
                Assert.Fail("Candle low count match fail.");
            }


            for (int i = 0; i < _dataCalcContext.Candles.Count - 1; i++)
            {
                var close = _dataCalcContext.Candles[i].L;

                if (close != ntValues[i])
                {
                    failCount++;
                    failed = true;

                    if (firstFail == 0)
                        firstFail = i;
                }
            }

            if (failed)
            {
                TestContext.WriteLine("Candle low data match fail. Failed " + failCount + " bars, out of " + _dataCalcContext.Candles.Count + " AD bars. " +
                                      "First bar fail: " + firstFail);

                Assert.Fail("Candle low data match fail.");
            }
        }

        #endregion
    }
}
