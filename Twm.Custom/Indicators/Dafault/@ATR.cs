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
    public class ATR : Indicator
    {
        private const string IndicatorName = "ATR";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 2)]
        public int Period { get; set; }

        
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ATRValue { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> TrueRange { get; set; }

        private Plot _atr;
        private Plot _tr;


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
            _atr = new Plot();
            _atr.ChartType = PlotChartType.Bars;
            _tr = new Plot();

            var secondPane = AddPane();
            AddPanePlot(secondPane, _atr);
        }

        private void AssignSeries()
        {
            
            ATRValue = new Series<double>();
            _atr.DataSource = ATRValue;
            _atr.PlotColors = new ColorMap(DataCalcContext);
            AddSeries(ATRValue);

            TrueRange = new Series<double>();
            _tr.DataSource = TrueRange;
            AddSeries(TrueRange);
        }


        public override void OnBarUpdate()
        {
            if (CurrentBar < 2)
                return;

            
            var val1 = Math.Abs(High[0] - Low[0]);
            var val2 = Math.Abs(High[0] - Close[1]);
            var val3 = Math.Abs(Low[0] - Close[1]);


            var trueRange = Math.Max(Math.Max(val1, val2), val3);
            TrueRange[0] = trueRange;


            if (CurrentBar < Period)
            {
                ATRValue[0] = trueRange;

            }
            else if (CurrentBar == Period)
            {
                var trSum = 0d;

                for (int i = 0; i < CurrentBar; i++)
                {
                    trSum += TrueRange[i];

                }

                ATRValue[0] = trSum / Period;
            }
            else if (CurrentBar > Period)
            {
                ATRValue[0] = ((Period - 1) * ATRValue[1] + TrueRange[0])/Period;
            }

            if (Close[0] > Close[1])
            {
                _atr.PlotColors[0] = Brushes.Green;
            }
            if (Close[0] < Close[1])
            {
                _atr.PlotColors[0] = Brushes.Red;
            }
            if (Close[0] == Close[1])
            {
                _atr.PlotColors[0] = Brushes.Gray;
            }
        }
    }











}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public ATR ATR(int period, ScriptOptions options = null)
		{
			return ATR(Input, period, options);
		}

		public ATR ATR(ISeries<double> input, int period, ScriptOptions options = null)
		{
			IEnumerable<ATR> cache = DataCalcContext.GetIndicatorCache<ATR>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<ATR>(input, false, options);
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
		public ATR ATR(int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.ATR(Input, period, options);
		}

		public ATR ATR(ISeries<double> input, int period, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.ATR(input, period, options);
		}
	}
}

#endregion
