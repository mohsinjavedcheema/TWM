using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class VolumeEma : Indicator
    {
        private const string IndicatorName = "Volume EMA";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 2)]
        public int Period { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> VolEmaVal { get; set; }

        private Plot _plot;
        private Indicator _ema;

        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                Name = IndicatorName;
                Version = IndicatorVersion;

                Period = 14;

                _plot = new Plot() { Thickness = 2, Color = Colors.Blue, ChartType = PlotChartType.Linear, Name = "Volume EMA" };
                _plot.PlotColors = new ColorMap(DataCalcContext);
                var secondPane = AddPane();
                AddPanePlot(secondPane, _plot);
            }
            else if (State == State.Configured)
            {
                VolEmaVal = new Series<double>();
                AddSeries(VolEmaVal);
                _plot.DataSource = VolEmaVal;

                var options = new ScriptOptions();
                options.ShowPanes = false;
                options.ShowPlots = false;

                _ema = EMA(Volume, Period, options);
            }
        }


        public override void OnBarUpdate()
        {
            if (CurrentBar < 2)
                return;

            VolEmaVal[0] = _ema[0];

            if (Volume[0] >= Volume[1])
            {
                _plot.PlotColors[0] = Brushes.DarkGreen;
            }
            else if (Volume[0] < Volume[1])
            {
                _plot.PlotColors[0] = Brushes.DarkRed;
            }


        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public VolumeEma VolumeEma(int period, ScriptOptions options = null)
		{
			return VolumeEma(Input, period, options);
		}

		public VolumeEma VolumeEma(ISeries<double> input, int period, ScriptOptions options = null)
		{
			IEnumerable<VolumeEma> cache = DataCalcContext.GetIndicatorCache<VolumeEma>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<VolumeEma>(input, false, options);
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
		public VolumeEma VolumeEma(int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.VolumeEma(Input, period, options);
		}

		public VolumeEma VolumeEma(ISeries<double> input, int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.VolumeEma(input, period, options);
		}
	}
}

#endregion
