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
    public class VolumeInd : Indicator
    {
        private const string IndicatorName = "Volume";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> VolVal { get; set; }

        private Plot _plot;

        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                Name = IndicatorName;
                Version = IndicatorVersion;
                
                _plot = new Plot() { Thickness = 2, Color = Colors.Blue, ChartType = PlotChartType.Bars, Name = "Volume"};
                _plot.PlotColors = new ColorMap(DataCalcContext);
                var secondPane = AddPane();
                AddPanePlot(secondPane, _plot);
            }
            else if (State == State.Configured)
            {
                VolVal = new Series<double>();
                AddSeries(VolVal);
                _plot.DataSource = VolVal;
            }
        }


        public override void OnBarUpdate()
        {
            if (CurrentBar == 1)
            {
                VolVal[0] = Volume[0];
                return;
            }

            VolVal[0] = Volume[0];

            if (Volume[0] >= Volume[1])
            {
                _plot.PlotColors[0] = Brushes.DarkGreen;
            }
            else if (Volume[0] < Volume[1])
            {
                _plot.PlotColors[0] = Brushes.DarkRed;
            }


        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public VolumeInd VolumeInd(ScriptOptions options = null)
		{
			return VolumeInd(Input, options);
		}

		public VolumeInd VolumeInd(ISeries<double> input, ScriptOptions options = null)
		{
			IEnumerable<VolumeInd> cache = DataCalcContext.GetIndicatorCache<VolumeInd>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input))
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<VolumeInd>(input, false, options);
			indicator.ChangeState();
			return indicator;
		}
	}
}
namespace Twm.Custom.Strategies
{
	public partial class Strategy : Twm.Core.DataCalc.StrategyBase
	{
		public VolumeInd VolumeInd(ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.VolumeInd(Input, options);
		}

		public VolumeInd VolumeInd(ISeries<double> input, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.VolumeInd(input, options);
		}
	}
}

#endregion
