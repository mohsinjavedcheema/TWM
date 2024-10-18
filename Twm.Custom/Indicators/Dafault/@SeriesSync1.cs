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
    public class SeriesSync1: Indicator
    {
        private const string IndicatorName = "Series Sync 1";
        private const string IndicatorVersion = " 1.0";

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> SuperSync { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> CalculatedSeries { get; set; }

        private Plot _superSync;
        private Plot _calculatedSeries;
        private PaneControl _secondPane;
        
        private HMA _hma;
        private TMA _tma;
        private MACD _macd;

        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                Name = IndicatorName;

                _superSync = new Plot(Colors.Red, "SuperSync", 1, PlotLineType.Dashed, PlotChartType.Bars);
                _secondPane = AddPane();

                _calculatedSeries = new Plot(Colors.Transparent, "Calculated Series", 1, PlotLineType.Dashed, PlotChartType.Bars);
                AddPanePlot(_secondPane, _superSync);
                AddPanePlot(_secondPane, _calculatedSeries);
            }
            else if (State == State.Configured)
            {

                
                SuperSync = new Series<double>();
                _superSync.DataSource = SuperSync;
                _superSync.PlotColors = new ColorMap(DataCalcContext);
                AddSeries(SuperSync);

                CalculatedSeries = new Series<double>();
                _calculatedSeries.DataSource = CalculatedSeries;
                _calculatedSeries.PlotColors = new ColorMap(DataCalcContext);
                AddSeries(CalculatedSeries);

                var options = new ScriptOptions();
                options.ShowPanes = false;
                options.ShowPlots = false;

                _hma = HMA(100, options);
                _tma = TMA(100, options);
                _macd = MACD(CalculatedSeries, 15, 30 ,2, options);

            }
        }

        public override void OnBarUpdate()
        {
            CalculatedSeries[0] = _hma[0] - _tma[0];
            SuperSync[0] = _macd[0];
        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public SeriesSync1 SeriesSync1(ScriptOptions options = null)
		{
			return SeriesSync1(Input, options);
		}

		public SeriesSync1 SeriesSync1(ISeries<double> input, ScriptOptions options = null)
		{
			IEnumerable<SeriesSync1> cache = DataCalcContext.GetIndicatorCache<SeriesSync1>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input))
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<SeriesSync1>(input, false, options);
			indicator.ChangeState();
			return indicator;
		}
	}
}
namespace Twm.Custom.Strategies
{
	public partial class Strategy : Twm.Core.DataCalc.StrategyBase
	{
		public SeriesSync1 SeriesSync1(ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.SeriesSync1(Input, options);
		}

		public SeriesSync1 SeriesSync1(ISeries<double> input, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.SeriesSync1(input, options);
		}
	}
}

#endregion
