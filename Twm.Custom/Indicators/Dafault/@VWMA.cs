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
    public class VWMA : Indicator
    {
        private const string IndicatorName = "VWMA";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 2)]
        public int Period { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> MA { get; set; }



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

                volSum = new Series<double>();
                AddSeries(volSum);

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

        private double priorVolPriceSum;
        private double volPriceSum;
        private Series<double> volSum;

        public override void OnBarUpdate()
        {
            priorVolPriceSum = volPriceSum;

            double volume0 = Volume[0];
            double volumePeriod = Volume[Math.Min(Period, CurrentBar -1)];

            volPriceSum = priorVolPriceSum + Input[0] * volume0 - (CurrentBar-1 >= Period ? Input[Period] * volumePeriod : 0);
            volSum[0] = volume0 + (CurrentBar-1 > 0 ? volSum[1] : 0) - (CurrentBar-1 >= Period ? volumePeriod : 0);
            MA[0] = volSum[0] == 0 ? volPriceSum : volPriceSum / volSum[0];
            
        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public VWMA VWMA(int period, ScriptOptions options = null)
		{
			return VWMA(Input, period, options);
		}

		public VWMA VWMA(ISeries<double> input, int period, ScriptOptions options = null)
		{
			IEnumerable<VWMA> cache = DataCalcContext.GetIndicatorCache<VWMA>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<VWMA>(input, false, options);
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
		public VWMA VWMA(int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.VWMA(Input, period, options);
		}

		public VWMA VWMA(ISeries<double> input, int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.VWMA(input, period, options);
		}
	}
}

#endregion
