using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using Twm.Chart.Classes;
using Twm.Chart.Enums;
using Twm.Chart.Interfaces;
using Twm.Core.Attributes;
using Twm.Core.DataCalc;
using Twm.Core.Enums;
using Twm.Custom.Indicators.Default;

namespace Twm.Custom.Indicators.Default
{
    public class BollingerBands : Indicator
    {
        private const string IndicatorName = "Bollinger Bands";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 2)]
        public int Period { get; set; }

        [TwmProperty]
        [Display(Name = "St Dev Mult", GroupName = Parameters, Order = 3)]
        public double StDevMultiplier { get; set; }


        [Browsable(false)]
        [XmlIgnore]
        public Series<double> MiddleBand { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> TopBand { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> BottomBand { get; set; }

        private Series<double> _sma;
        private Series<double> _stDev;


        private Plot _plotMiddle;
        private Plot _plotTop;
        private Plot _plotBottom;

        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                Name = IndicatorName + IndicatorVersion;
                
                Period = 14;
                StDevMultiplier = 1;

                _plotMiddle = new Plot(){Name="Middle"};
                _plotMiddle.Color = Colors.Magenta;
                _plotMiddle.Thickness = 1;
                AddPanePlot(_plotMiddle);

                _plotTop = new Plot(){Name="Top"};
                _plotTop.Color = Colors.Blue;
                _plotTop.LineType = PlotLineType.Dashed;
                _plotTop.Thickness = 1;
                AddPanePlot(_plotTop);

                _plotBottom = new Plot(){Name= "Bottom"};
                _plotBottom.Color = Colors.Blue;
                _plotBottom.LineType = PlotLineType.Dashed;
                _plotBottom.Thickness = 1;
                AddPanePlot(_plotBottom);

                

            }
            else if (State == State.Configured)
            {
                MiddleBand = new Series<double>();
                AddSeries(MiddleBand);
                _plotMiddle.DataSource = MiddleBand;

                TopBand = new Series<double>();
                AddSeries(TopBand);
                _plotTop.DataSource = TopBand;

                BottomBand = new Series<double>();
                AddSeries(BottomBand);
                _plotBottom.DataSource = BottomBand;

                var io1 = new ScriptOptions { ShowPlots = false, ShowPanes = false };
                _sma = SMA(Input, Period, io1).MA;

                var io2 = new ScriptOptions { ShowPlots = false, ShowPanes = false};
                _stDev = StDev(Input, Period, io2).StDeviation;
            }
        }


        public override void OnBarUpdate()
        {
            if (CurrentBar < Period)
            {
                MiddleBand[0] = _sma[0];
                TopBand[0] = _sma[0];
                BottomBand[0] = _sma[0];

                return;
            }

            //Print(DateTime[0] + "  " + _stDev[0]);

            MiddleBand[0] = _sma[0]; 
            TopBand[0] = MiddleBand[0] + _stDev[0] * StDevMultiplier;
            BottomBand[0] = MiddleBand[0] - _stDev[0] * StDevMultiplier;
        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public BollingerBands BollingerBands(int period, double stDevMultiplier, ScriptOptions options = null)
		{
			return BollingerBands(Input, period, stDevMultiplier, options);
		}

		public BollingerBands BollingerBands(ISeries<double> input, int period, double stDevMultiplier, ScriptOptions options = null)
		{
			IEnumerable<BollingerBands> cache = DataCalcContext.GetIndicatorCache<BollingerBands>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period && cacheIndicator.StDevMultiplier == stDevMultiplier)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<BollingerBands>(input, false, options);
			indicator.Period = period;
			indicator.StDevMultiplier = stDevMultiplier;
			indicator.ChangeState();
			return indicator;
		}
	}
}
namespace Twm.Custom.Strategies
{
	public partial class Strategy : Twm.Core.DataCalc.StrategyBase
	{
		public BollingerBands BollingerBands(int period, double stDevMultiplier, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.BollingerBands(Input, period, stDevMultiplier, options);
		}

		public BollingerBands BollingerBands(ISeries<double> input, int period, double stDevMultiplier, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.BollingerBands(input, period, stDevMultiplier, options);
		}
	}
}

#endregion
