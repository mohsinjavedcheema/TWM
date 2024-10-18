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
    public class Min : Indicator
    {
        private const string IndicatorName = "MIN";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 2)]
        public int Period { get; set; }



        [Browsable(false)]
        [XmlIgnore]
        public Series<double> MinVal { get; set; }


        private Plot _min;
        private int lastBar;
        private double lastMin;
        private double runningMin;
        private int runningBar;
        private int thisBar;

        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                Name = IndicatorName + IndicatorVersion;
                Period = 14;

                _min = new Plot() {Thickness = 1, Color = Colors.Purple, ChartType = PlotChartType.Linear, Name = "MIN"};
                AddPanePlot(_min);
            }
            else if (State == State.Configured)
            {
                MinVal = new Series<double>();
                AddSeries(MinVal);
                _min.DataSource = MinVal;

                lastBar = 0;
                lastMin = 0;
                runningMin = 0;
                runningBar = 0;
                thisBar = 0;
            }
        }


        public override void OnBarUpdate()
        {
            if (CurrentBar == 1)
            {
                runningMin = Input[0];
                lastMin = Input[0];
                runningBar = 0;
                lastBar = 0;
                thisBar = 0;
                MinVal[0] = Input[0];
                return;
            }

            if (CurrentBar - runningBar > Period || CurrentBar < thisBar)
            {
                runningMin = double.MaxValue;
                for (int barsBack = Math.Min(CurrentBar, Period - 1); barsBack > 0; barsBack--)
                    if (Input[barsBack] <= runningMin)
                    {
                        runningMin = Input[barsBack];
                        runningBar = CurrentBar - barsBack;
                    }
            }

            if (thisBar != CurrentBar)
            {
                lastMin = runningMin;
                lastBar = runningBar;
                thisBar = CurrentBar;
            }

            if (Input[0] <= lastMin)
            {
                runningMin = Input[0];
                runningBar = CurrentBar;
            }
            else
            {
                runningMin = lastMin;
                runningBar = lastBar;
            }

            MinVal[0] = runningMin;
        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public Min Min(int period, ScriptOptions options = null)
		{
			return Min(Input, period, options);
		}

		public Min Min(ISeries<double> input, int period, ScriptOptions options = null)
		{
			IEnumerable<Min> cache = DataCalcContext.GetIndicatorCache<Min>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<Min>(input, false, options);
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
		public Min Min(int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.Min(Input, period, options);
		}

		public Min Min(ISeries<double> input, int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.Min(input, period, options);
		}
	}
}

#endregion
