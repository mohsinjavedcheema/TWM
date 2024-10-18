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
    public class Max : Indicator
    {
        private const string IndicatorName = "MAX";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 2)]
        public int Period { get; set; }



        [Browsable(false)]
        [XmlIgnore]
        public Series<double> MaxVal { get; set; }


        private Plot _max;
        private int lastBar;
        private double lastMax;
        private double runningMax;
        private int runningBar;
        private int thisBar;

        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                Name = IndicatorName + IndicatorVersion;
                Period = 14;

                _max = new Plot() { Thickness = 1, Color = Colors.Purple, ChartType = PlotChartType.Linear, Name = "MAX" };
                AddPanePlot(_max);

            }
            else if (State == State.Configured)
            {
                MaxVal = new Series<double>();
                AddSeries(MaxVal);
                _max.DataSource = MaxVal;

                lastBar = 0;
                lastMax = 0;
                runningMax = 0;
                runningBar = 0;
                thisBar = 0;
            }
        }


        public override void OnBarUpdate()
        {
            if (CurrentBar == 1)
            {
                runningMax = Input[0];
                lastMax = Input[0];
                runningBar = 0;
                lastBar = 0;
                thisBar = 0;
                MaxVal[0] = Input[0];
                return;
            }

            if (CurrentBar - runningBar >= Period || CurrentBar < thisBar)
            {
                runningMax = double.MinValue;
                for (int barsBack = Math.Min(CurrentBar, Period - 1); barsBack > 0; barsBack--)
                    if (Input[barsBack] >= runningMax)
                    {
                        runningMax = Input[barsBack];
                        runningBar = CurrentBar - barsBack;
                    }
            }

            if (thisBar != CurrentBar)
            {
                lastMax = runningMax;
                lastBar = runningBar;
                thisBar = CurrentBar;
            }

            if (Input[0] >= lastMax)
            {
                runningMax = Input[0];
                runningBar = CurrentBar;
            }
            else
            {
                runningMax = lastMax;
                runningBar = lastBar;
            }

            MaxVal[0] = runningMax;
        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public Max Max(int period, ScriptOptions options = null)
		{
			return Max(Input, period, options);
		}

		public Max Max(ISeries<double> input, int period, ScriptOptions options = null)
		{
			IEnumerable<Max> cache = DataCalcContext.GetIndicatorCache<Max>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<Max>(input, false, options);
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
		public Max Max(int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.Max(Input, period, options);
		}

		public Max Max(ISeries<double> input, int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.Max(input, period, options);
		}
	}
}

#endregion
