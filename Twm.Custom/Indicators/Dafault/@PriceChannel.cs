using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class PriceChannel : Indicator
    {
        private const string IndicatorName = "Price Channel";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 2)]
        public int Period { get; set; }

        public enum DataType
        {
            HighLow,
            BarBody
        }

        [TwmProperty]
        [Display(Name = "Data Type", GroupName = Parameters, Order = 3)]
        public PriceChannel.DataType MyDataType { get; set; }


        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ChannelHigh { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ChannelLow { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ChannelMiddle { get; set; }

        private Plot _plotChannelHigh;
        private Plot _plotChannelLow;
        private Plot _plotChannelMiddle;

        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                Name = IndicatorName + IndicatorVersion;
                Period = 14;

                _plotChannelHigh = new Plot() { Thickness =1, Color = Colors.Blue, ChartType = PlotChartType.Linear, Name = "Channel High"};
                AddPanePlot(_plotChannelHigh);

                _plotChannelLow = new Plot() { Thickness = 1, Color = Colors.Blue, ChartType = PlotChartType.Linear, Name = "Channel Low" };
                AddPanePlot(_plotChannelLow);

                _plotChannelMiddle = new Plot() { Thickness = 1, Color = Colors.Blue, LineType = PlotLineType.Dashed, Name = "Channel Middle"};
                AddPanePlot(_plotChannelMiddle);
            }
            else if (State == State.Configured)
            {
                ChannelHigh = new Series<double>();
                AddSeries(ChannelHigh);
                _plotChannelHigh.DataSource = ChannelHigh;

                ChannelLow = new Series<double>();
                AddSeries(ChannelLow);
                _plotChannelLow.DataSource = ChannelLow;

                ChannelMiddle = new Series<double>();
                AddSeries(ChannelMiddle);
                _plotChannelMiddle.DataSource = ChannelMiddle;


            }
        }


        public override void OnBarUpdate()
        {
            if (CurrentBar < Period)
            {
                ChannelHigh[0] = Close[0];
                ChannelLow[0] = Close[0];
                ChannelMiddle[0] = Close[0];

                return;
            }

            var highestHigh = 0d;
            var lowestLow = 0d;

            for (int i = 0; i < Period; i++)
            {
                var topValue = High[i];
                var bottomValue = Low[i];

                if (MyDataType == DataType.BarBody)
                {
                    if (Close[i] > Open[i])
                    {
                        topValue = Close[i];
                        bottomValue = Open[i];
                    }
                    else if (Close[i] < Open[i])
                    {
                        topValue = Open[i];
                        bottomValue = Close[i];
                    }
                    else if (Close[i] == Open[i])
                    {
                        topValue = Close[i];
                        bottomValue = Close[i];
                    }
                }

                if (topValue > highestHigh)
                    highestHigh = topValue;

                if (lowestLow == 0)
                    lowestLow = bottomValue;

                if (bottomValue < lowestLow)
                    lowestLow = bottomValue;

            }

            ChannelHigh[0] = highestHigh;
            ChannelLow[0] = lowestLow;

            var range = highestHigh - lowestLow;
            ChannelMiddle[0] = highestHigh - range / 2;
        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public PriceChannel PriceChannel(int period, PriceChannel.DataType myDataType, ScriptOptions options = null)
		{
			return PriceChannel(Input, period, myDataType, options);
		}

		public PriceChannel PriceChannel(ISeries<double> input, int period, PriceChannel.DataType myDataType, ScriptOptions options = null)
		{
			IEnumerable<PriceChannel> cache = DataCalcContext.GetIndicatorCache<PriceChannel>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period && cacheIndicator.MyDataType == myDataType)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<PriceChannel>(input, false, options);
			indicator.Period = period;
			indicator.MyDataType = myDataType;
			indicator.ChangeState();
			return indicator;
		}
	}
}
namespace Twm.Custom.Strategies
{
	public partial class Strategy : Twm.Core.DataCalc.StrategyBase
	{
		public PriceChannel PriceChannel(int period, PriceChannel.DataType myDataType, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.PriceChannel(Input, period, myDataType, options);
		}

		public PriceChannel PriceChannel(ISeries<double> input, int period, PriceChannel.DataType myDataType, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.PriceChannel(input, period, myDataType, options);
		}
	}
}

#endregion
