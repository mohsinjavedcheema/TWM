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
    public class IndicatorCallValuesCheck
    {
        private const string STRATEGY_TYPE = "IndicatorValStrat";
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

        [Test]
        [Order(1)]
        [Category("Strategy Indicator Call Value Test")]
        public async Task T01_CallAllIndicatorValueTest()
        {
            _strategy.Enabled = true;
            _strategy.IsTemporary = false;

            await _dataCalcContext.ExecuteStrategy(null);

            List<List<double>> correctValues = new List<List<double>>();

            //ema
            var correctEMAValues = Array.ConvertAll(File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                "Data", "SimpleEMA_0407_1307.txt")), i => Convert.ToDouble(i, CultureInfo.InvariantCulture)).ToList();
            
            //sma
            var correctSMAValues = Array.ConvertAll(File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                "Data", "SmaValues_0407_1307.txt")), i => Convert.ToDouble(i, CultureInfo.InvariantCulture)).ToList();
            

            //wma
            var correctWMAValues = Array.ConvertAll(File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Data", "WMA_0407_1307.txt")), i => Convert.ToDouble(i, CultureInfo.InvariantCulture)).ToList();

            //tema
            var correctTEMAValues = Array.ConvertAll(File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Data", "TEMA_0407_1307.txt")), i => Convert.ToDouble(i, CultureInfo.InvariantCulture)).ToList();

            //tma
            var correctTMAValues = Array.ConvertAll(File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Data", "TMA_0407_1307.txt")), i => Convert.ToDouble(i, CultureInfo.InvariantCulture)).ToList();

            //hma
            var correctHMAValues = Array.ConvertAll(File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Data", "HMA_0407_1307.txt")), i => Convert.ToDouble(i, CultureInfo.InvariantCulture)).ToList();


            //macddiff
            var correctMACDValues = Array.ConvertAll(File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Data", "MacdDiff_0407_1307.txt")), i => Convert.ToDouble(i, CultureInfo.InvariantCulture)).ToList();

            correctValues.Add(correctEMAValues);
            correctValues.Add(correctSMAValues);
            correctValues.Add(correctWMAValues);
            correctValues.Add(correctTEMAValues);
            correctValues.Add(correctTMAValues);
            correctValues.Add(correctHMAValues);
            correctValues.Add(correctMACDValues);


            if (_strategy.Tag is List<List<double>> indicatorValues)
            {
                for (int i = 0; i < indicatorValues.Count; i++)
                {
                    
                    if (indicatorValues.Count  != correctValues.Count)
                    {
                        TestContext.WriteLine("Total indicator in test: " + correctValues.Count + ". AD Values Count: " + indicatorValues.Count);
                        Assert.Warn(" Total indicator in test count fail");
                    }

                    //we use minus 1 because in AD live bar is also included
                    if (indicatorValues[i].Count - 1 != correctValues[i].Count)
                    {
                        TestContext.WriteLine("NT Values count: " + correctValues[i].Count + ". AD Values Count: " + indicatorValues[i].Count);
                        Assert.Warn(IndicatorNameInList(i)+" Values Check Failed. Values count don't match.");
                    }

                    var failed = false;
                    

                    for (int w = 0; w < indicatorValues[i].Count - 1; w++)
                    {

                        var v1 = correctValues[i][w];
                        var v2 = indicatorValues[i][w];
                        var dif = Math.Abs(v1 - v2);

                        if (dif > 0.01)
                        {
                            failed = true;
                        }

                        if (failed)
                        {
                            TestContext.WriteLine("Values Check Failed. Values do not match. First indicator fail + " + IndicatorNameInList(i) + " Test stoped.");
                            Assert.Fail("Values Check Failed");
                            break;
                        }
                    }
                }

                
            }
        }

        private string IndicatorNameInList(int index)
        {
            switch (index)
            {
                case 0:
                    return "EMA";
                    
                case 1:
                    return "SMA";
                    
                case 2:
                    return "WMA";
                    
                case 3:
                    return "TEMA";
                    
                case 4:
                    return "TMA";
                    
                case 5:
                    return "HMA";
                    
                case 6:
                    return "MACDDIFF";
                    
                default:
                    return "NONE";
            }
        }
    }
}
