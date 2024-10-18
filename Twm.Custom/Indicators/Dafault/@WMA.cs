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
    public class WMA : Indicator
    {
        private const string IndicatorName = "WMA";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 2)]
        public int Period { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> WMAValue { get; set; }


        private int _myPeriod;
        private double _priorSum;
        private double _priorWsum;
        private double _sum;
        private double _wsum;
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

                //_priorSum = 0;
                //_priorWsum = 0;
                //_sum = 0;
                //_wsum = 0;
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
            _plot = new Plot() { Thickness = 1, Color = Colors.Red };
            _plot.ChartType = PlotChartType.Linear;
            _plot.PlotColors = new ColorMap(DataCalcContext);
            AddPanePlot(_plot);
        }

        private void AssignSeries()
        {
            WMAValue = new Series<double>();
            AddSeries(WMAValue);
            _plot.DataSource = WMAValue;
        }

        public override void OnBarUpdate()
        {
            

            _priorWsum = _wsum;
            _priorSum = _sum;
            _myPeriod = Math.Min(CurrentBar, Period);

            _wsum = _priorWsum - (CurrentBar-1 >= Period ? _priorSum : 0) + _myPeriod * Input[0];
            _sum = _priorSum + Input[0] - (CurrentBar-1 >= Period ? Input[Period] : 0);
            var res = _wsum / (0.5 * _myPeriod * (_myPeriod + 1));
            WMAValue[0] = res;

        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public WMA WMA(int period, ScriptOptions options = null)
		{
			return WMA(Input, period, options);
		}

		public WMA WMA(ISeries<double> input, int period, ScriptOptions options = null)
		{
			IEnumerable<WMA> cache = DataCalcContext.GetIndicatorCache<WMA>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<WMA>(input, false, options);
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
		public WMA WMA(int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.WMA(Input, period, options);
		}

		public WMA WMA(ISeries<double> input, int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.WMA(input, period, options);
		}
	}
}

#endregion
