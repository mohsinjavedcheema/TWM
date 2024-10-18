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
using Twm.Chart.Controls;
using Twm.Chart.Enums;
using Twm.Chart.Interfaces;
using Twm.Core.Attributes;
using Twm.Core.Classes;
using Twm.Core.DataCalc;
using Twm.Core.Enums;
using Twm.Custom.Indicators.Default;

namespace Twm.Custom.Indicators.Default
{
    public class StochasticRSI : Indicator
    {
        private const string IndicatorName = "Stochastic RSI";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 2)]
        public int Period { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> StochRSIValue { get; set; }

        
        private Plot _stochRSI;
        private Series<double> _min;
        private Series<double> _max;
        private Series<double> _rsi;

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

                StochRSIValue = new Series<double>();
                _stochRSI.DataSource = StochRSIValue;
                _stochRSI.PlotColors = new ColorMap(DataCalcContext);
                AddSeries(StochRSIValue);

                var options = new ScriptOptions();
                options.ShowPanes = false;
                options.ShowPlots = false;

                _rsi = RSI(Input, Period, 1, options).RSIValues;
                _min = Min(_rsi, Period, options).MinVal;
                _max = Max(_rsi, Period, options).MaxVal;

                Draw.LineHorizontal("UpBoundary", 0.8, Brushes.LightGray, 1, DashStyles.Dash, _secondPane);
                Draw.LineHorizontal("DownBoundary", 0.2, Brushes.LightGray, 1, DashStyles.Dash, _secondPane);

            }
        }

        private void SetDefaultParameterValues()
        {
            Name = IndicatorName;
            Period = 14;
        }

        private PaneControl _secondPane;

        private void CreatePlots()
        {
            _stochRSI = new Plot(Colors.Red, "Stochastic RSI", 1, PlotLineType.Solid, PlotChartType.Linear);

            _secondPane = AddPane();
            AddPanePlot(_secondPane, _stochRSI);
        }


        public override void OnBarUpdate()
        {
            double rsi0 = _rsi[0];
            double rsiL = _min[0];
            double rsiH = _max[0];

            if (rsi0 != rsiL && rsiH != rsiL)
                StochRSIValue[0] = (rsi0 - rsiL) / (rsiH - rsiL);
            else
                StochRSIValue[0] = 0;
        }
    }











}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public StochasticRSI StochasticRSI(int period, ScriptOptions options = null)
		{
			return StochasticRSI(Input, period, options);
		}

		public StochasticRSI StochasticRSI(ISeries<double> input, int period, ScriptOptions options = null)
		{
			IEnumerable<StochasticRSI> cache = DataCalcContext.GetIndicatorCache<StochasticRSI>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<StochasticRSI>(input, false, options);
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
		public StochasticRSI StochasticRSI(int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.StochasticRSI(Input, period, options);
		}

		public StochasticRSI StochasticRSI(ISeries<double> input, int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.StochasticRSI(input, period, options);
		}
	}
}

#endregion
