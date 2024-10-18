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
    public class TMA : Indicator
    {
        private const string IndicatorName = "TMA";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 2)]
        public int Period { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> TMAValue { get; set; }


        private Plot _plot;
        private int p1;
        private int p2;
        private SMA sma;

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

                p1 = 0;
                p2 = 0;
                if ((Period & 1) == 0)
                {
                    // Even period
                    p1 = Period / 2;
                    p2 = p1 + 1;
                }
                else
                {
                    // Odd period
                    p1 = (Period + 1) / 2;
                    p2 = p1;
                }

                var options = new ScriptOptions();
                options.ShowPanes = false;
                options.ShowPlots = false;

                sma = SMA(SMA(Input, p1, options), p2, options);
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
            TMAValue = new Series<double>();
            AddSeries(TMAValue);
            _plot.DataSource = TMAValue;
        }

        public override void OnBarUpdate()
        {
            TMAValue[0] = sma[0];
        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public TMA TMA(int period, ScriptOptions options = null)
		{
			return TMA(Input, period, options);
		}

		public TMA TMA(ISeries<double> input, int period, ScriptOptions options = null)
		{
			IEnumerable<TMA> cache = DataCalcContext.GetIndicatorCache<TMA>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<TMA>(input, false, options);
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
		public TMA TMA(int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.TMA(Input, period, options);
		}

		public TMA TMA(ISeries<double> input, int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.TMA(input, period, options);
		}
	}
}

#endregion
