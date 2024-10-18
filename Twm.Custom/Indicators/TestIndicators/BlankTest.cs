using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using Twm.Chart.Classes;
using Twm.Chart.Enums;
using Twm.Chart.Interfaces;
using Twm.Core.Attributes;
using Twm.Core.DataCalc;
using Twm.Core.Enums;
using Twm.Core.Tests.Enums;
using Twm.Custom.Indicators.TestSettings;

namespace Twm.Custom.Indicators.TestSettings
{
    public class BlankTest : Indicator
    {
        private const string IndicatorName = "Blank Test";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string TestSetUp = "Test Settings";

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 2)]
        public int Period { get; set; }

        private IndicatorSpeedSettings _speedSettings;
        private IndicatorValueCheckSettings _valueCheckSettings;
        private bool _isSpeedTest;

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Result { get; set; }


        private double constant1;
        private double constant2;
        private Plot _plot;
        private Indicator _sma;
        private Series<double> _macdDiff;
        private Indicator _wma;
        private Indicator _tma;
        private Indicator _tema;
        private Series<double> _vwma;
        private Indicator _hma;

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> CalculatedValues
        { get; set; }

        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                SetDefaultParameterValues();
                CreatePlots();

            }
            else if (State == State.Configured)
            {
                AssignSeries();

                constant1 = 2.0 / (1 + Period);
                constant2 = 1 - (2.0 / (1 + Period));
                barCount = Close.Count - 1;

                _speedSettings = IndicatorSpeedSettings.NotSet;
                _valueCheckSettings = IndicatorValueCheckSettings.NotSet;


                if (Tag is IndicatorSpeedSettings)
                {
                    _speedSettings = (IndicatorSpeedSettings)(Tag);
                    _isSpeedTest = true;

                }
                else if (Tag is IndicatorValueCheckSettings)
                {
                    _valueCheckSettings = (IndicatorValueCheckSettings)Tag;
                }
                else
                {
                    throw new Exception("Invalid test settings: " + Tag);
                }

                if (_speedSettings == IndicatorSpeedSettings.T04_EMACalculatedUsingSMAInput ||
                    _valueCheckSettings == IndicatorValueCheckSettings.T02_EMACalculatedUsingSMAValuesCheck ||
                    _valueCheckSettings == IndicatorValueCheckSettings.T03_SMAValuesCheck)
                {
                    _sma = SMA(Period);
                }

                if (_speedSettings == IndicatorSpeedSettings.T06_CalculatedEMAIsInputToSMA ||
                    _valueCheckSettings == IndicatorValueCheckSettings.T05_CalculatedEMAIsInputToSMAValuesCheck)
                {
                    CalculatedValues = new Series<double>();
                    AddSeries(CalculatedValues);
                    _sma = SMA(CalculatedValues, Period);
                }

                if (_speedSettings == IndicatorSpeedSettings.T07_MacdDiff ||
                    _valueCheckSettings == IndicatorValueCheckSettings.T06_MacdDiffValuesCheck)
                {
                    var options = new ScriptOptions();
                    options.ShowPanes = false;
                    options.ShowPlots = false;

                    var macd = MACD(12, 24, 3, options);
                    _macdDiff = macd.Diff;
                }

                if (_speedSettings == IndicatorSpeedSettings.T08_WMASpeedTest ||
                    _valueCheckSettings == IndicatorValueCheckSettings.T07_WMAValuesCheck)
                {
                    _wma = WMA(Period);
                }

                if (_speedSettings == IndicatorSpeedSettings.T11_TMASpeedTest ||
                    _valueCheckSettings == IndicatorValueCheckSettings.T10_TMAValuesCheck)
                {
                    _tma = TMA(Period);
                }

                if (_speedSettings == IndicatorSpeedSettings.T10_TEMASpeedTest ||
                    _valueCheckSettings == IndicatorValueCheckSettings.T09_TEMAValuesCheck)
                {
                    _tema = TEMA(Period);
                }

                if (_speedSettings == IndicatorSpeedSettings.T09_VWMASpeedTest ||
                    _valueCheckSettings == IndicatorValueCheckSettings.T08_VWMAValuesCheck)
                {
                    _vwma = VWMA(Period).MA;
                }

                if (_speedSettings == IndicatorSpeedSettings.T12_HMASpeedTest ||
                    _valueCheckSettings == IndicatorValueCheckSettings.T11_HMAValuesCheck)
                {
                    _hma = HMA(Period);
                }
            }
        }

        private int barCount;

        private void SetDefaultParameterValues()
        {
            Name = IndicatorName;
            Version = IndicatorVersion;
            Period = 14;
        }

        private void CreatePlots()
        {
            _plot = new Plot() { Thickness = 1, Color = Colors.Blue };
            _plot.ChartType = PlotChartType.Linear;
            _plot.PlotColors = new ColorMap(DataCalcContext);
            AddPanePlot(_plot);
        }

        private void AssignSeries()
        {
            Result = new Series<double>();
            AddSeries(Result);
            _plot.DataSource = Result;
        }

        private DateTime _start;
        private DateTime _end;

        public override void OnBarUpdate()
        {
            if (CurrentBar == 1)
            {
                _start = System.DateTime.Now;
            }
            if (CurrentBar == barCount)
            {
                _end = System.DateTime.Now;

                Tag = (_end - _start).TotalMilliseconds;
            }

            if (_isSpeedTest)
            {
                switch (_speedSettings)
                {
                    case IndicatorSpeedSettings.T01_BlankExecution:
                        T01S_ExecutionTest();
                        break;
                    case IndicatorSpeedSettings.T02_ValueAssignmentTest:
                        T02S_ValueAssignmentTest();
                        break;
                    case IndicatorSpeedSettings.T03_EMACalculated:
                        T03S_SingleEmaCalculatedTest();
                        break;
                    case IndicatorSpeedSettings.T04_EMACalculatedUsingSMAInput:
                        T04S_EMACalculatedUsingSMATest();
                        break;
                    case IndicatorSpeedSettings.T05_EMACalculatedUsingLocalSMA:
                        T05S_EMACalculatedUsingLocalSMATest();
                        break;
                    case IndicatorSpeedSettings.T06_CalculatedEMAIsInputToSMA:
                        T06S_CalculatedEMAIsInputToSMATest();
                        break;
                    case IndicatorSpeedSettings.T07_MacdDiff:
                        T07_MacdDiffSpeedTest();
                        break;
                    case IndicatorSpeedSettings.T08_WMASpeedTest:
                        T08_WMASpeedTest();
                        break;
                    case IndicatorSpeedSettings.T11_TMASpeedTest:
                        T11_TMASpeedTest();
                        break;
                    case IndicatorSpeedSettings.T10_TEMASpeedTest:
                        T10_TEMASpeedTest();
                        break;
                    case IndicatorSpeedSettings.T09_VWMASpeedTest:
                        T09_VWMASpeedTest();
                        break;
                    case IndicatorSpeedSettings.T12_HMASpeedTest:
                        T12_HMASpeedTest();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (_valueCheckSettings)
                {
                    case IndicatorValueCheckSettings.T01_SimpleEMAValuesCheck:
                        T01V_SimpleEMAValuesCheckTest();
                        break;
                    case IndicatorValueCheckSettings.T02_EMACalculatedUsingSMAValuesCheck:
                        T02V_EMACalculatedUsingSMAValuesCheckTest();
                        break;
                    case IndicatorValueCheckSettings.T03_SMAValuesCheck:
                        T03V_SMAValuesCheckTest();
                        break;
                    case IndicatorValueCheckSettings.T04_EMACalculatedUsingLocalSMAValuesCheck:
                        T04V_EMACalculatedUsingLocalSMAValuesCheckTest();
                        break;
                    case IndicatorValueCheckSettings.T05_CalculatedEMAIsInputToSMAValuesCheck:
                        T05V_CalculatedEMAIsInputToSMAValuesCheckTest();
                        break;
                    case IndicatorValueCheckSettings.T06_MacdDiffValuesCheck:
                        T06_MacdDiffValuesCheckTest();
                        break;
                    case IndicatorValueCheckSettings.T07_WMAValuesCheck:
                        T07_WMAValuesCheckTest();
                        break;
                    case IndicatorValueCheckSettings.T10_TMAValuesCheck:
                        T10_TMAValuesCheckTest();
                        break;
                    case IndicatorValueCheckSettings.T09_TEMAValuesCheck:
                        T09_TEMAValuesCheckTest();
                        break;
                    case IndicatorValueCheckSettings.T11_HMAValuesCheck:
                        T11_HMAValuesCheckTest();
                        break;

                    default:
                        break;
                }
            }
        }

        #region Speed Tests

        private void T01S_ExecutionTest()
        {

        }

        private void T02S_ValueAssignmentTest()
        {
            Result[0] = 45.56;
        }

        private void T03S_SingleEmaCalculatedTest()
        {
            Result[0] = (CurrentBar == 1 ? Input[0] : Input[0] * constant1 + constant2 * Result[1]);
        }

        private void T04S_EMACalculatedUsingSMATest()
        {
            Result[0] = (CurrentBar == 1 ? _sma[0] : _sma[0] * constant1 + constant2 * Result[1]);
        }

        private void T05S_EMACalculatedUsingLocalSMATest()
        {
            Result[0] = (CurrentBar == 1 ? SMA(14)[0] : SMA(14)[0] * constant1 + constant2 * Result[1]);
        }

        private void T06S_CalculatedEMAIsInputToSMATest()
        {
            CalculatedValues[0] = (CurrentBar == 1 ? Input[0] : Input[0] * constant1 + constant2 * Result[1]);
            Result[0] = _sma[0];
        }

        private void T07_MacdDiffSpeedTest()
        {
            Result[0] = _macdDiff[0];
        }

        private void T08_WMASpeedTest()
        {
            Result[0] = _wma[0];
        }

        private void T11_TMASpeedTest()
        {
            Result[0] = _tma[0];
        }

        private void T10_TEMASpeedTest()
        {
            Result[0] = _tema[0];

        }

        private void T09_VWMASpeedTest()
        {
            Result[0] = _vwma[0];

        }

        private void T12_HMASpeedTest()
        {
            Result[0] = _hma[0];
        }

        #endregion

        #region Value Check Tests

        List<double> _valuesEma = new List<double>();
        private void T01V_SimpleEMAValuesCheckTest()
        {
            Result[0] = (CurrentBar == 1 ? Input[0] : Input[0] * constant1 + constant2 * Result[1]);
            _valuesEma.Add(Result[0]);

            Tag = _valuesEma;
        }

        List<double> _valuesEmaInputSma = new List<double>();
        private void T02V_EMACalculatedUsingSMAValuesCheckTest()
        {
            Result[0] = (CurrentBar == 1 ? _sma[0] : _sma[0] * constant1 + constant2 * Result[1]);

            _valuesEmaInputSma.Add(Result[0]);
            Tag = _valuesEmaInputSma;
        }

        List<double> _valuesSma = new List<double>();
        private void T03V_SMAValuesCheckTest()
        {
            Result[0] = _sma[0];
            _valuesSma.Add(Result[0]);
            Tag = _valuesSma;
        }

        List<double> _valuesEmaInputLocalSma = new List<double>();
        private void T04V_EMACalculatedUsingLocalSMAValuesCheckTest()
        {
            Result[0] = (CurrentBar == 1 ? SMA(14)[0] : SMA(14)[0] * constant1 + constant2 * Result[1]);
            _valuesEmaInputLocalSma.Add(Result[0]);
            Tag = _valuesEmaInputLocalSma;
        }

        List<double> _valuesCalclEmaIsInputToSma = new List<double>();
        private void T05V_CalculatedEMAIsInputToSMAValuesCheckTest()
        {
            CalculatedValues[0] = (CurrentBar == 1 ? Input[0] : Input[0] * constant1 + constant2 * Result[1]);
            Result[0] = _sma[0];

            _valuesCalclEmaIsInputToSma.Add(Result[0]);
            Tag = _valuesCalclEmaIsInputToSma;
        }

        List<double> _macdDiffValues = new List<double>();
        private void T06_MacdDiffValuesCheckTest()
        {
            Result[0] = _macdDiff[0];

            _macdDiffValues.Add(Result[0]);
            Tag = _macdDiffValues;
        }

        List<double> _wmaValues = new List<double>();
        private void T07_WMAValuesCheckTest()
        {
            Result[0] = _wma[0];

            _wmaValues.Add(Result[0]);
            Tag = _wmaValues;
        }

        List<double> _tmaValues = new List<double>();
        private void T10_TMAValuesCheckTest()
        {
            Result[0] = _tma[0];

            _tmaValues.Add(Result[0]);
            Tag = _tmaValues;

        }

        List<double> _temaValues = new List<double>();
        private void T09_TEMAValuesCheckTest()
        {
            Result[0] = _tema[0];

            _temaValues.Add(Result[0]);
            Tag = _temaValues;

        }

        List<double> _hmaValues = new List<double>();
        private void T11_HMAValuesCheckTest()
        {
            Result[0] = _hma[0];

            _hmaValues.Add(Result[0]);
            Tag = _hmaValues;
        }


        #endregion
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public BlankTest BlankTest(int period, ScriptOptions options = null)
		{
			return BlankTest(Input, period, options);
		}

		public BlankTest BlankTest(ISeries<double> input, int period, ScriptOptions options = null)
		{
			IEnumerable<BlankTest> cache = DataCalcContext.GetIndicatorCache<BlankTest>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<BlankTest>(input, false, options);
			indicator.Period = period;
			indicator.ChangeState();
			return indicator;
		}
	}
}
namespace Twm.Custom.Strategies
{
	public partial class Strategy : Twm.Core.DataCalc.StrategyBase
	{
		public BlankTest BlankTest(int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.BlankTest(Input, period, options);
		}

		public BlankTest BlankTest(ISeries<double> input, int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.BlankTest(input, period, options);
		}
	}
}

#endregion
