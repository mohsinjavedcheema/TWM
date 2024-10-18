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
    public class HMA : Indicator
    {
        private const string IndicatorName = "HMA";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 2)]
        public int Period { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> HMAValue { get; set; }


        private Plot _plot;
        private Series<double> _diffSeries;
        private WMA _wma1;
        private WMA _wma2;
        private WMA _wmaDiffSeries;

        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                SetDefaultParameterValues();

            }
            else if (State == State.Configured)
            {
                CreatePlots();
                AssignSeries();

                var options = new ScriptOptions();
                options.ShowPanes = false;
                options.ShowPlots = false;

                _diffSeries = new Series<double>();
                AddSeries(_diffSeries);

                _wmaDiffSeries = WMA(_diffSeries, (int)Math.Sqrt(Period), options);

                _wma1 = WMA(Input, Period / 2, options);
                _wma2 = WMA(Input, Period, options);
                
            }
        }

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
            HMAValue = new Series<double>();
            AddSeries(HMAValue);
            _plot.DataSource = HMAValue;
        }

        private DateTime _start;
        private DateTime _end;

        public override void OnBarUpdate()
        {
            if (CurrentBar == 1)
            {
                _start = System.DateTime.Now;
            }
            if (CurrentBar == Close.Count - 1)
            {
                _end = System.DateTime.Now;
                var span = (_end - _start).TotalMilliseconds / 1000;
                Print("RESULTS: -->> Span: " + span + " Start: " + _start.ToString("O") + " End: " + _end.ToString("O") + " Bars" + CurrentBar);
            }

            _diffSeries[0] = 2 * _wma1[0] - _wma2[0];
            HMAValue[0] = _wmaDiffSeries[0];
        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public HMA HMA(int period, ScriptOptions options = null)
		{
			return HMA(Input, period, options);
		}

		public HMA HMA(ISeries<double> input, int period, ScriptOptions options = null)
		{
			IEnumerable<HMA> cache = DataCalcContext.GetIndicatorCache<HMA>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<HMA>(input, false, options);
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
		public HMA HMA(int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.HMA(Input, period, options);
		}

		public HMA HMA(ISeries<double> input, int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.HMA(input, period, options);
		}
	}
}

#endregion
