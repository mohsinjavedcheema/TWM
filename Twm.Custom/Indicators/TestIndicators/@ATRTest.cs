using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;
using System.Windows.Media;
using System.Xml.Serialization;
using AlgoDesk.Chart.Classes;
using AlgoDesk.Chart.Enums;
using AlgoDesk.Chart.Interfaces;
using AlgoDesk.Core.Attributes;
using AlgoDesk.Core.Classes;
using AlgoDesk.Core.DataCalc;
using AlgoDesk.Core.Enums;
using AlgoDesk.Custom.Indicators;

namespace AlgoDesk.Custom.Indicators
{
    public class ATRTest : Indicator
    {
        private const string IndicatorName = "ATR";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [AlgoDeskProperty]
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
            _tr = new Plot();

            var secondPane = AddPane();
            AddPanePlot(secondPane, _atr);
            _atr.PlotColors = new ColorMap(this.DataCalcContext);
        }

        private void AssignSeries()
        {
            
            ATRValue = new Series<double>();
            _atr.DataSource = ATRValue;
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

            if (ATRValue[0] > 1)
            {
                _atr.PlotColors[0] = Brushes.Chocolate;
            } else
            
            if (ATRValue[0] <= 1 && ATRValue[0] >= 0.5)
            {
                _atr.PlotColors[0] = Brushes.Fuchsia;
                _atr.PlotColors[1] = Brushes.SeaGreen;
                _atr.PlotColors[2] = Brushes.Blue;

            } else
            
            if (ATRValue[0] >= 0)
            {
                _atr.PlotColors[0] = Brushes.LightCoral;
            }
        }
    }











}

#region AlgoDesk generated code. Neither change nor remove.
namespace AlgoDesk.Custom.Indicators
{
	public partial class Indicator : AlgoDesk.Core.DataCalc.IndicatorBase
	{
		public ATRTest ATRTest(int period, IndicatorOptions options = null)
		{
			return ATRTest(Input, period, options);
		}

		public ATRTest ATRTest(ISeries<double> input, int period, IndicatorOptions options = null)
		{
			List<ATRTest> cache = DataCalcContext.GetIndicatorCache<ATRTest>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<ATRTest>(input, false, options);
			indicator.Period = period;
			indicator.ChangeState();
			return indicator;
		}
	}
}
#endregion
