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
    public class ROC : Indicator
    {
        private const string IndicatorName = "ROC";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 2)]
        public int Period { get; set; }


        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ROCValue { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> TrueRange { get; set; }

        private Plot _roc;

        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                SetDefaultParameterValues();
                CreatePlots();
            }
            else if (State == State.Configured)
            {
                ClearDebug();
                AssignSeries();
            }
        }

        private void SetDefaultParameterValues()
        {
            Name = IndicatorName;
            Period = 14;
        }

        private void CreatePlots()
        {
            _roc = new Plot(Colors.BlueViolet, "ROC",1);
            _roc.ChartType = PlotChartType.Bars;

            var secondPane = AddPane();
            AddPanePlot(secondPane, _roc);
        }

        private void AssignSeries()
        {

            ROCValue = new Series<double>();
            _roc.DataSource = ROCValue;
            AddSeries(ROCValue);
        }


        public override void OnBarUpdate()
        {
            if (CurrentBar <= Period)
                return;

            var change = Input[0] - Input[Period];
            var percentChange = change / Input[Period] * 100;

            ROCValue[0] = percentChange;

            

        }
    }


}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public ROC ROC(int period, ScriptOptions options = null)
		{
			return ROC(Input, period, options);
		}

		public ROC ROC(ISeries<double> input, int period, ScriptOptions options = null)
		{
			IEnumerable<ROC> cache = DataCalcContext.GetIndicatorCache<ROC>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<ROC>(input, false, options);
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
		public ROC ROC(int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.ROC(Input, period, options);
		}

		public ROC ROC(ISeries<double> input, int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.ROC(input, period, options);
		}
	}
}

#endregion
