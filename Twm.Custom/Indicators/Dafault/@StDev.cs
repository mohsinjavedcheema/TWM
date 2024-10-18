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
    public class StDev : Indicator
    {
        private const string IndicatorName = "Standard Deviation";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 1)]
        public int Period { get; set; }

        

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> StDeviation { get; set; }

        private Plot _stDev;

        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                Name = IndicatorName + IndicatorVersion;
                Period = 14;

                _stDev = new Plot();

                var secondPane = AddPane();
                AddPanePlot(secondPane, _stDev);
            }
            else if (State == State.Configured)
            {
                StDeviation = new Series<double>();
                _stDev.DataSource = StDeviation;
                AddSeries(StDeviation);

                
            }
        }


        public override void OnBarUpdate()
        {
            var x = Input[0];

            if (CurrentBar < Period)
            {
                StDeviation[0] = 0;
                return;
            }

            var sum = 0d;

            for (int i = 0; i < Period; i++)
            {
                sum += Input[i];
            }

            var average = sum / Period;

            var sumOfDistance = 0d;

            for (int i = 0; i < Period; i++)
            {
                var distance = average - Input[i];
                var square = distance * distance;
                sumOfDistance += square;
            }

            var variance = sumOfDistance / Period;
            StDeviation[0] = Math.Sqrt(variance);

        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public StDev StDev(int period, ScriptOptions options = null)
		{
			return StDev(Input, period, options);
		}

		public StDev StDev(ISeries<double> input, int period, ScriptOptions options = null)
		{
			IEnumerable<StDev> cache = DataCalcContext.GetIndicatorCache<StDev>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<StDev>(input, false, options);
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
		public StDev StDev(int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.StDev(Input, period, options);
		}

		public StDev StDev(ISeries<double> input, int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.StDev(input, period, options);
		}
	}
}

#endregion
