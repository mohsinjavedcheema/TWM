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
    public class RSI : Indicator
    {
        private const string IndicatorName = "RSI";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";

        private Series<double> avgDown;
        private Series<double> avgUp;
        private double constant1;
        private double constant2;
        private double constant3;
        private Series<double> down;
        private SMA smaDown;
        private SMA smaUp;
        private Series<double> up;

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 3)]
        public int Period { get; set; }

        [TwmProperty]
        [Display(Name = "Smooth", GroupName = Parameters, Order = 3)]
        public int Smooth { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> RSIValues { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> AvRSIValues { get; set; }

        private Plot _rsi;
        private Plot _avRsi;

        public override void OnStateChanged()
        {

            if (State == State.SetDefaults)
            {
                SetDefaults();
                CreatePlots();
            }
            else if (State == State.Configured)
            {
                ClearDebug();

                constant1 = 2.0 / (1 + Smooth);
                constant2 = (1 - (2.0 / (1 + Smooth)));
                constant3 = (Period - 1);

                avgUp = new Series<double>();
                AddSeries(avgUp);
                avgDown = new Series<double>();
                AddSeries(avgDown);
                down = new Series<double>();
                AddSeries(down);
                up = new Series<double>();
                AddSeries(up);
                smaDown = SMA(down, Period);
                smaUp = SMA(up, Period);

                AssignSeries();

            }
            
        }

        private void SetDefaults()
        {
            Name = IndicatorName;

            Period = 14;
            Version = IndicatorVersion;
            Smooth = 3;

        }

        private void CreatePlots()
        {
            _rsi = new Plot(Colors.Blue, "RSI", 1);
            var secondPane = AddPane();
            AddPanePlot(secondPane, _rsi);

            _avRsi = new Plot(Colors.Orange, "AvRSI", 1);
            AddPanePlot(secondPane, _avRsi);
        }

        private void AssignSeries()
        {
            RSIValues = new Series<double>();
            _rsi.DataSource = RSIValues;
            AddSeries(RSIValues);

            AvRSIValues = new Series<double>();
            _avRsi.DataSource = AvRSIValues;
            AddSeries(AvRSIValues);
        }


        public override void OnBarUpdate()
        {
            if (CurrentBar == 1)
            {
                down[0] = 0;
                up[0] = 0;

                if (Period < 3)
                    AvRSIValues[0] = 50;

                return;
            }

            double input0 = Input[0];
            double input1 = Input[1];
            down[0] = Math.Max(input1 - input0, 0);
            up[0] = Math.Max(input0 - input1, 0);

            if (CurrentBar + 1 < Period)
            {
                if (CurrentBar + 1 == Period - 1)
                    AvRSIValues[0] = 50;
                return;
            }

            if ((CurrentBar + 1) == Period)
            {
                // First averages
                avgDown[0] = smaDown[0];
                avgUp[0] = smaUp[0];
            }
            else
            {
                // Rest of averages are smoothed
                avgDown[0] = (avgDown[1] * constant3 + down[0]) / Period;
                avgUp[0] = (avgUp[1] * constant3 + up[0]) / Period;
            }

            double avgDown0 = avgDown[0];
            double value0 = avgDown0 == 0 ? 100 : 100 - 100 / (1 + avgUp[0] / avgDown0);
            RSIValues[0] = value0;
            AvRSIValues[0] = constant1 * value0 + constant2 * AvRSIValues[1];
        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public RSI RSI(int period, int smooth, ScriptOptions options = null)
		{
			return RSI(Input, period, smooth, options);
		}

		public RSI RSI(ISeries<double> input, int period, int smooth, ScriptOptions options = null)
		{
			IEnumerable<RSI> cache = DataCalcContext.GetIndicatorCache<RSI>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period && cacheIndicator.Smooth == smooth)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<RSI>(input, false, options);
			indicator.Period = period;
			indicator.Smooth = smooth;
			indicator.ChangeState();
			return indicator;
		}
	}
}
namespace Twm.Custom.Strategies
{
	public partial class Strategy : Twm.Core.DataCalc.StrategyBase
	{
		public RSI RSI(int period, int smooth, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.RSI(Input, period, smooth, options);
		}

		public RSI RSI(ISeries<double> input, int period, int smooth, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.RSI(input, period, smooth, options);
		}
	}
}

#endregion
