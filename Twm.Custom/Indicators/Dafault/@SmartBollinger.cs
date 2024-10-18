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
    public class SmartBollinger: Indicator
    {
        private const string IndicatorName = "Smart Bollinger";
        private const string IndicatorVersion = " 1.0";

        #region Properties

        public enum MyMaType
        {
            EMA,
            HMA,
            SMA,
            VWMA,
            TMA,
            TEMA,
            WMA
        }

        [TwmProperty]
        [Display(Name = "MA Type", GroupName = "Parameters", Order = 0)]
        public SmartBollinger.MyMaType BollingerMaType
        { get; set; }

        [Range(0, int.MaxValue), TwmProperty]
        [Display(Name = "Multiplier", GroupName = "Params", Order = 1)]
        public double Multiplier
        { get; set; }

        [Range(1, int.MaxValue), TwmProperty]
        [Display(Name = "Period", GroupName = "Params", Order = 2)]
        public int Period
        { get; set; }

        public enum OffsetType
        {
            StDev,
            ATR
        }

        [TwmProperty]
        [Display(Name = "Offset Type", GroupName = "Params", Order = 3)]
        public SmartBollinger.OffsetType MyOffsetType
        { get; set; }

        #endregion

        #region Values

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> MiddleBand { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> TopBand { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> BottomBand { get; set; }

        private Indicator _ma;
        private Series<double> _stDev;
        private Series<double> _atr;


        private Plot _plotMiddle;
        private Plot _plotTop;
        private Plot _plotBottom;

        #endregion

        #region Init

        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                Name = IndicatorName + IndicatorVersion;
                
                Period = 14;
                Multiplier = 1;
                
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
                _plotMiddle.IsAutoscale = true;
                AddSeries(MiddleBand);
                _plotMiddle.DataSource = MiddleBand;

                TopBand = new Series<double>();
                AddSeries(TopBand);
                _plotTop.DataSource = TopBand;
                _plotTop.IsAutoscale = true;

                BottomBand = new Series<double>();
                AddSeries(BottomBand);
                _plotBottom.DataSource = BottomBand;
                _plotBottom.IsAutoscale = true;

                
                SetMa();

                var options = new ScriptOptions { ShowPlots = false, ShowPanes = false};
                _stDev = StDev(Input, Period, options).StDeviation;
                _atr = ATR(Input, Period, options).ATRValue;

                
            }
        }

        private void SetMa()
        {
            var options = new ScriptOptions { ShowPlots = false, ShowPanes = false };

            if (BollingerMaType == MyMaType.EMA)
            {
                _ma = EMA(Input, Period, options);

            }
            if (BollingerMaType == MyMaType.HMA)
            {
                _ma = HMA(Input, Period, options);
            }
            if (BollingerMaType == MyMaType.SMA)
            {
                _ma = SMA(Input, Period, options);
            }
            if (BollingerMaType == MyMaType.TEMA)
            {
                _ma = TEMA(Input, Period, options);
            }
            if (BollingerMaType == MyMaType.TMA)
            {
                _ma = TMA(Input, Period, options);
            }
            if (BollingerMaType == MyMaType.VWMA)
            {
                _ma = VWMA(Input, Period, options);
            }
            if (BollingerMaType == MyMaType.WMA)
            {
                _ma = WMA(Input, Period, options);
            }

        }

        #endregion

        public override void OnBarUpdate()
        {
            var offset = _stDev[0];

            if (MyOffsetType == OffsetType.ATR)
            {
                offset = _atr[0];
            }

            TopBand[0] = _ma[0] + Multiplier * offset;
            MiddleBand[0] = _ma[0];
            BottomBand[0] = _ma[0] - Multiplier * offset;
        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public SmartBollinger SmartBollinger(SmartBollinger.MyMaType bollingerMaType, double multiplier, int period, SmartBollinger.OffsetType myOffsetType, ScriptOptions options = null)
		{
			return SmartBollinger(Input, bollingerMaType, multiplier, period, myOffsetType, options);
		}

		public SmartBollinger SmartBollinger(ISeries<double> input, SmartBollinger.MyMaType bollingerMaType, double multiplier, int period, SmartBollinger.OffsetType myOffsetType, ScriptOptions options = null)
		{
			IEnumerable<SmartBollinger> cache = DataCalcContext.GetIndicatorCache<SmartBollinger>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.BollingerMaType == bollingerMaType && cacheIndicator.Multiplier == multiplier && cacheIndicator.Period == period && cacheIndicator.MyOffsetType == myOffsetType)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<SmartBollinger>(input, false, options);
			indicator.BollingerMaType = bollingerMaType;
			indicator.Multiplier = multiplier;
			indicator.Period = period;
			indicator.MyOffsetType = myOffsetType;
			indicator.ChangeState();
			return indicator;
		}
	}
}
namespace Twm.Custom.Strategies
{
	public partial class Strategy : Twm.Core.DataCalc.StrategyBase
	{
		public SmartBollinger SmartBollinger(SmartBollinger.MyMaType bollingerMaType, double multiplier, int period, SmartBollinger.OffsetType myOffsetType, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.SmartBollinger(Input, bollingerMaType, multiplier, period, myOffsetType, options);
		}

		public SmartBollinger SmartBollinger(ISeries<double> input, SmartBollinger.MyMaType bollingerMaType, double multiplier, int period, SmartBollinger.OffsetType myOffsetType, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.SmartBollinger(input, bollingerMaType, multiplier, period, myOffsetType, options);
		}
	}
}

#endregion
