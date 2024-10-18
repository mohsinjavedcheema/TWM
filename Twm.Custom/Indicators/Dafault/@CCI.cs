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
    public class CCI : Indicator
    {
        private const string IndicatorName = "CCI";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 2)]
        public int Period { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> CCIValue { get; set; }


        private Plot _cci;
        private SMA _sma;
        private Typical _typical;

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

                CCIValue = new Series<double>();
                _cci.DataSource = CCIValue;
                _cci.PlotColors = new ColorMap(DataCalcContext);
                AddSeries(CCIValue);

                var options = new ScriptOptions();
                options.ShowPanes = false;
                options.ShowPlots = false;

                _typical = Typical(options);
                _sma = SMA(_typical, Period, options);

                Draw.LineHorizontal("UpBoundary", 200, Brushes.LightGray, 1, DashStyles.Dash, _secondPane);
                Draw.LineHorizontal("DownBoundary", -200, Brushes.LightGray, 1, DashStyles.Dash, _secondPane);
                Draw.LineHorizontal("Middle", 0, Brushes.Orchid, 1, DashStyles.Dash, _secondPane);

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
            _cci = new Plot(Colors.Red, "CCI", 1, PlotLineType.Solid, PlotChartType.Linear);

            _secondPane = AddPane();
            AddPanePlot(_secondPane, _cci);
        }


        public override void OnBarUpdate()
        {
            if (CurrentBar == 1)
            {
                CCIValue[0] = 0;
            }
            else
            {
                double mean = 0;
                double sma0 = _sma[0];

                for (int i = Math.Min(CurrentBar-1, Period - 1); i >= 0; i--)
                {
                    mean += Math.Abs(_typical[i] - sma0);
                }

                double div = 0;

                if (mean == 0)
                {
                    div = 1;
                }
                else
                {
                    div = (0.015 * (mean / Math.Min(Period, CurrentBar)));
                }

                CCIValue[0] = (_typical[0] - sma0) / div;

            }
        }
    }

}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public CCI CCI(int period, ScriptOptions options = null)
		{
			return CCI(Input, period, options);
		}

		public CCI CCI(ISeries<double> input, int period, ScriptOptions options = null)
		{
			IEnumerable<CCI> cache = DataCalcContext.GetIndicatorCache<CCI>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<CCI>(input, false, options);
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
		public CCI CCI(int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.CCI(Input, period, options);
		}

		public CCI CCI(ISeries<double> input, int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.CCI(input, period, options);
		}
	}
}

#endregion
