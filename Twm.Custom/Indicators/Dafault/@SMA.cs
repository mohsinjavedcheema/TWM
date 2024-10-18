using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using Twm.Chart.Classes;
using Twm.Chart.Controls;
using Twm.Chart.Interfaces;
using Twm.Core.Attributes;
using Twm.Core.DataCalc;
using Twm.Core.Enums;
using Twm.Custom.Indicators.Default;

namespace Twm.Custom.Indicators.Default
{
    public class SMA : Indicator
    {
        private const string IndicatorName = "SMA";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";

        private double _priorSum;
        private double _sum;

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
                Name = IndicatorName;
                Version = IndicatorVersion;

                var MAThickness = 1;
                var MAColor = Colors.Green;
                Period = 14;

                _plot = new Plot(){Thickness = MAThickness, Color = MAColor};
                AddPanePlot(_plot);

            }
            else if (State == State.Configured)
            {
                
                MA = new Series<double>();
                AddSeries(MA);
                _plot.DataSource = MA;
            }
        }


        public override void OnBarUpdate()
        {
            _priorSum = _sum;

            _sum = _priorSum + Input[0] - (CurrentBar-1 >= Period ? Input[Period] : 0);
            MA[0] = _sum / (CurrentBar-1 < Period ? CurrentBar : Period);


        }
        
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public SMA SMA(int period, ScriptOptions options = null)
		{
			return SMA(Input, period, options);
		}

		public SMA SMA(ISeries<double> input, int period, ScriptOptions options = null)
		{
			IEnumerable<SMA> cache = DataCalcContext.GetIndicatorCache<SMA>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<SMA>(input, false, options);
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
		public SMA SMA(int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.SMA(Input, period, options);
		}

		public SMA SMA(ISeries<double> input, int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.SMA(input, period, options);
		}
	}
}

#endregion
