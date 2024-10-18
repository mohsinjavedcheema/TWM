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
using Twm.Core.ViewModels.DataSeries;
using NUnit.Framework;

namespace Twm.Tests.Misc
{
    public class DataTest_1min_NQ_16_06_13_07
    {
        private const string TEST_INSTRUMENT_SYMBOL = "/NQ:XCME";
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


        #endregion

        #region Candle Data Check

        [Test]
        [Order(1)]
        [Category("DataTest_1min_NQ_16_06_13_07")]
        public void NQCandlesClosePriceCheck()
        {

            var fileDataPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Data", "NQ1minClosePrices_1606_1307.txt");
            string[] arr = File.ReadAllLines(fileDataPath);
            var ntValues = Array.ConvertAll(arr, i => Convert.ToDouble(i, CultureInfo.InvariantCulture)).ToList();

            var failed = false;
            int failCount = 0;
            int firstFail = 0;

            //we use minus 1 because in AD live bar is also included
            if (_dataCalcContext.Candles.Count - 1 != ntValues.Count)
            {
                TestContext.WriteLine("Candle close count match fail. NT: " + ntValues.Count + " AD: " + _dataCalcContext.Candles.Count);
                Assert.Warn("Candle close count match fail.");
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

                if (i == _dataCalcContext.Candles.Count - 2)
                {
                    TestContext.WriteLine("Check start date AD: " + _dataCalcContext.Candles[0].t + " end: " +
                                          _dataCalcContext.Candles[i].t);
                }
            }

            if (failed)
            {
                TestContext.WriteLine("Candle close  data match fail. Failed " + failCount + " bars, out of " + _dataCalcContext.Candles.Count + " AD bars. " +
                                      "First bar fail: " + firstFail);

                Assert.Fail("Candle close data match fail.");
            }
        }


        #endregion
    }
}
