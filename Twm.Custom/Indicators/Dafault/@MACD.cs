using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;
using System.Windows.Media;
using System.Xml.Serialization;
using Twm.Chart.Classes;
using Twm.Chart.Enums;
using Twm.Chart.Interfaces;
using Twm.Core.Attributes;
using Twm.Core.Classes;
using Twm.Core.DataCalc;
using Twm.Core.Enums;
using Twm.Custom.Indicators.Default;

namespace Twm.Custom.Indicators.Default
{
    public class MACD : Indicator
    {
        private const string IndicatorName = "MACD";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";



        [TwmProperty]
        [Display(Name = "Period Fast", GroupName = Parameters, Order = 2)]
        public int FastPeriod { get; set; }

        [TwmProperty]
        [Display(Name = "Period Slow", GroupName = Parameters, Order = 3)]
        public int SlowPeriod { get; set; }

        [TwmProperty]
        [Display(Name = "Period Smooth", GroupName = Parameters, Order = 4)]
        public int SmoothPeriod { get; set; }


        [Browsable(false)]
        [XmlIgnore]
        public Series<double> MACDValues { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Average { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Diff { get; set; }


        private Plot _macd;
        private Plot _diff;
        private Plot _average;
        private Series<double> _emaFast;
        private Series<double> _emaSlow;
        private Series<double> _emaSmooth;


        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                SetDefaultParameterValues();
                CreatePlots();
            }
            else if (State == State.Configured)
            {
                ClearDebug();
                AssignSeries();
                CreateRequiredIndicators();
            }
        }

        private void SetDefaultParameterValues()
        {
            Name = IndicatorName;
            FastPeriod = 12;
            SlowPeriod = 26;
            SmoothPeriod = 9;
        }

        private void CreatePlots()
        {
            _macd = new Plot(Colors.Blue, "MACD",1);
            _macd.Name = "MACD";
            _average = new Plot(Colors.Orange, "Average", 1);
            _diff = new Plot(Colors.Blue, "Difference", 2, PlotLineType.Solid, PlotChartType.Bars);
            _average.Name = "Average";
            _diff.Name = "Diff";

            _diff.PlotColors = new ColorMap(DataCalcContext);

            var secondPane = AddPane();
            AddPanePlot(secondPane, _macd);
            AddPanePlot(secondPane, _average);
            AddPanePlot(secondPane, _diff);
        }

        private void AssignSeries()
        {
            MACDValues = new Series<double>();
            _macd.DataSource = MACDValues;
            AddSeries(MACDValues);

            Average = new Series<double>();
            _average.DataSource = Average;
            AddSeries(Average);

            Diff = new Series<double>();
            _diff.DataSource = Diff;
            AddSeries(Diff);
        }

        private void CreateRequiredIndicators()
        {
            var options = new ScriptOptions() {ShowPanes = false, ShowPlots = false};
            _emaFast = EMA(Input, FastPeriod, options).MA;
            _emaSlow = EMA(Input, SlowPeriod, options).MA;
            _emaSmooth = EMA(MACDValues, SmoothPeriod, options).MA;
        }


        public override void OnBarUpdate()
        {
            //if (CurrentBar < SlowPeriod)
            //    return;

            if (CurrentBar == 1)
            {
                Diff[0] = 0;
            }

            MACDValues[0] = _emaFast[0] - _emaSlow[0];
            Average[0] = _emaSmooth[0];
            Diff[0] = MACDValues[0] - Average[0];

            if (Diff[0] > 0)
            {
                _diff.PlotColors[0] = Brushes.Green;
            }
            if (Diff[0] < 0)
            {
                _diff.PlotColors[0] = Brushes.Red;
            }
            

        }
    }











}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public MACD MACD(int fastPeriod, int slowPeriod, int smoothPeriod, ScriptOptions options = null)
		{
			return MACD(Input, fastPeriod, slowPeriod, smoothPeriod, options);
		}

		public MACD MACD(ISeries<double> input, int fastPeriod, int slowPeriod, int smoothPeriod, ScriptOptions options = null)
		{
			IEnumerable<MACD> cache = DataCalcContext.GetIndicatorCache<MACD>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.FastPeriod == fastPeriod && cacheIndicator.SlowPeriod == slowPeriod && cacheIndicator.SmoothPeriod == smoothPeriod)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<MACD>(input, false, options);
			indicator.FastPeriod = fastPeriod;
			indicator.SlowPeriod = slowPeriod;
			indicator.SmoothPeriod = smoothPeriod;
			indicator.ChangeState();
			return indicator;
		}
	}
}
namespace Twm.Custom.Strategies
{
	public partial class Strategy : Twm.Core.DataCalc.StrategyBase
	{
		public MACD MACD(int fastPeriod, int slowPeriod, int smoothPeriod, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.MACD(Input, fastPeriod, slowPeriod, smoothPeriod, options);
		}

		public MACD MACD(ISeries<double> input, int fastPeriod, int slowPeriod, int smoothPeriod, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.MACD(input, fastPeriod, slowPeriod, smoothPeriod, options);
		}
	}
}

#endregion
