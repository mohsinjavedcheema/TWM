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
    public class VolatillitySwitch : Indicator
    {
        private const string IndicatorName = "Volatillity Switch";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "LookBack", GroupName = Parameters, Order = 2)]
        public int LookBack { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> SwitchValue { get; set; }


        private Plot _vSwitch;
        private Series<double> dailyChange;
        private Series<double> stdDev;
        private DateTime _cc;
        private int _cb;
        private StDev _stDev;

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

                SwitchValue = new Series<double>();
                _vSwitch.DataSource = SwitchValue;
                _vSwitch.PlotColors = new ColorMap(DataCalcContext);
                AddSeries(SwitchValue);

                dailyChange = new Series<double>();
                AddSeries(dailyChange);
                stdDev = new Series<double>();
                AddSeries(stdDev);

                var options = new ScriptOptions();
                options.ShowPanes = false;
                options.ShowPlots = false;
                _stDev = StDev(dailyChange, LookBack, options);

                Draw.LineHorizontal("TrendStartLine", 0.5, Brushes.LightGray, 1, DashStyles.Dash, _secondPane);
                Draw.LineHorizontal("ZeroLine", 0, Brushes.LightGray, 1, DashStyles.Dash, _secondPane);
            }
        }

        private void SetDefaultParameterValues()
        {
            Name = IndicatorName;
            LookBack = 21;
        }

        private PaneControl _secondPane;


        private void CreatePlots()
        {
            _vSwitch = new Plot(Colors.Red, "CCI", 1, PlotLineType.Solid, PlotChartType.Linear);

            _secondPane = AddPane();
            AddPanePlot(_secondPane, _vSwitch);
        }


        public override void OnBarUpdate()
        {
            try
            {
                if (CurrentBar < LookBack)
                    return;

                if (Close.Count < 1)
                    return;

                Method1();
                Method2();


            }
            catch (Exception e)
            {
                Print(_cc + "  " + _cb + " VS ONBARUPDATE CRASHED!" + " FilterPerdiod:" + LookBack + " ERROR:" + e.StackTrace);
            }
        }

        private void Method1()
        {
            Method11();
            Method12();
        }

        private void Method11()
        {
            var d1 = M1();
            var d2 = M2();
            dailyChange[0] = (d1) / (d2);

        }

        private double M1()
        {
            return Close[0] - Close[1];
        }

        
        private double Z()
        {

            double y = 0;
            if (Close.Count > 1)
                y = Close[1];

            return y;
        }

        private double M2()
        {
            return (Close[0] + Close[1]) / 2;
        }

        private void Method12()
        {

            stdDev[0] = _stDev[0];
        }

        private void Method2()
        {
            double trueCount = 0;

            for (int x = LookBack; x > 0; x--)
            {
                if (stdDev[x] <= stdDev[0])
                {
                    trueCount++;
                }

            }

            SwitchValue[0] = trueCount / LookBack;

            if (SwitchValue[0] >= .5)
                _vSwitch.PlotColors[0] = Brushes.Red;
            else
                _vSwitch.PlotColors[0] = Brushes.Green;
        }

        
    }

}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public VolatillitySwitch VolatillitySwitch(int lookBack, ScriptOptions options = null)
		{
			return VolatillitySwitch(Input, lookBack, options);
		}

		public VolatillitySwitch VolatillitySwitch(ISeries<double> input, int lookBack, ScriptOptions options = null)
		{
			IEnumerable<VolatillitySwitch> cache = DataCalcContext.GetIndicatorCache<VolatillitySwitch>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.LookBack == lookBack)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<VolatillitySwitch>(input, false, options);
			indicator.LookBack = lookBack;
			indicator.ChangeState();
			return indicator;
		}
	}
}
namespace Twm.Custom.Strategies
{
	public partial class Strategy : Twm.Core.DataCalc.StrategyBase
	{
		public VolatillitySwitch VolatillitySwitch(int lookBack, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.VolatillitySwitch(Input, lookBack, options);
		}

		public VolatillitySwitch VolatillitySwitch(ISeries<double> input, int lookBack, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.VolatillitySwitch(input, lookBack, options);
		}
	}
}

#endregion
