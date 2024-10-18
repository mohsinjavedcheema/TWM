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
    public class EMA : Indicator
    {
        private const string IndicatorName = "EMA";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 2)]
        public int Period { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> MA { get; set; }


        private double constant1;
        private double constant2;
        private Plot _plot;

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
            }
        }

        private void SetDefaultParameterValues()
        {
            IsAutoscale = false;
            Name = IndicatorName;
            Version = IndicatorVersion;
            Period = 14;
        }

        private void CreatePlots()
        {
            _plot = new Plot() { Thickness = 1, Color = Colors.Blue };
            _plot.ChartType = PlotChartType.Linear;
            _plot.Color = Colors.Magenta;
            _plot.PlotColors = new ColorMap(DataCalcContext);
            AddPanePlot(_plot);
        }

        private void AssignSeries()
        {
            MA = new Series<double>();
            AddSeries(MA);
            _plot.DataSource = MA;
        }

        private DateTime _start;
        private DateTime _end;

        public override void OnBarUpdate()
        {
            MA[0] = (CurrentBar == 1 ? Input[0] : Input[0] * constant1 + constant2 * MA[1]);
            //MA[0] = Input[0];
            //MA[0] = 45.333;

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
        }

        public override void OnTickUpdate(ICandle candle, ICandle tick)
        {
            if (tick.IsFirstTickOfBar)
            {
                //Print("First tick of bar: " + tick.O + " open time: " + tick.t + " close time: " + tick.ct);
            }
        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public EMA EMA(int period, ScriptOptions options = null)
		{
			return EMA(Input, period, options);
		}

		public EMA EMA(ISeries<double> input, int period, ScriptOptions options = null)
		{
			IEnumerable<EMA> cache = DataCalcContext.GetIndicatorCache<EMA>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<EMA>(input, false, options);
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
		public EMA EMA(int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.EMA(Input, period, options);
		}

		public EMA EMA(ISeries<double> input, int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.EMA(input, period, options);
		}
	}
}

#endregion
