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
    public class Momentum : Indicator
    {
        private const string IndicatorName = "Momentum";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 2)]
        public int Period { get; set; }



        [Browsable(false)]
        [XmlIgnore]
        public Series<double> MomentumVal { get; set; }


        private Plot _momentum;

        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                Name = IndicatorName + IndicatorVersion;
                Period = 14;

                _momentum = new Plot() { Thickness = 1, Color = Colors.Purple, ChartType = PlotChartType.Linear, Name = "MAX" };
                
                var secondPane = AddPane();
                AddPanePlot(secondPane, _momentum);

            }
            else if (State == State.Configured)
            {

                MomentumVal = new Series<double>();
                AddSeries(MomentumVal);
                _momentum.DataSource = MomentumVal;
            }
        }


        public override void OnBarUpdate()
        {
            MomentumVal[0] = CurrentBar == 1 ? 0 : Input[0] - Input[Math.Min(CurrentBar-1, Period)];
        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public Momentum Momentum(int period, ScriptOptions options = null)
		{
			return Momentum(Input, period, options);
		}

		public Momentum Momentum(ISeries<double> input, int period, ScriptOptions options = null)
		{
			IEnumerable<Momentum> cache = DataCalcContext.GetIndicatorCache<Momentum>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<Momentum>(input, false, options);
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
		public Momentum Momentum(int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.Momentum(Input, period, options);
		}

		public Momentum Momentum(ISeries<double> input, int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.Momentum(input, period, options);
		}
	}
}

#endregion
