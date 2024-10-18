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
    public class TEMA : Indicator
    {
        private const string IndicatorName = "TEMA";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 2)]
        public int Period { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> MA { get; set; }

        private EMA ema1;
        private EMA ema2;
        private EMA ema3;


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

                var options = new ScriptOptions();
                options.ShowPanes = false;
                options.ShowPlots = false;

                ema1 = EMA(Input, Period, options);
                ema2 = EMA(ema1, Period, options);
                ema3 = EMA(ema2, Period, options);
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
            MA = new Series<double>();
            AddSeries(MA);
            _plot.DataSource = MA;
        }

        public override void OnBarUpdate()
        {
            MA[0] = 3 * ema1[0] - 3 * ema2[0] + ema3[0];
        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public TEMA TEMA(int period, ScriptOptions options = null)
		{
			return TEMA(Input, period, options);
		}

		public TEMA TEMA(ISeries<double> input, int period, ScriptOptions options = null)
		{
			IEnumerable<TEMA> cache = DataCalcContext.GetIndicatorCache<TEMA>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<TEMA>(input, false, options);
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
		public TEMA TEMA(int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.TEMA(Input, period, options);
		}

		public TEMA TEMA(ISeries<double> input, int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.TEMA(input, period, options);
		}
	}
}

#endregion
