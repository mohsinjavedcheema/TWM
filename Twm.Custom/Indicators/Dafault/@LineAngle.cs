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
    public class LineAngle : Indicator
    {
        private const string IndicatorName = "Line Angle";
        private const string IndicatorVersion = " 1.0";


        private const string Parameters = "Parameters";

        [TwmProperty]
        [Display(Name = "Look Back Bars", GroupName = "Parameters", Order = 0)]
        public int LookBackBars
        { get; set; }


        [TwmProperty]
        [Display(Name = "Entry Angle", GroupName = "Parameters", Order = 3)]
        public int EntryAngle
        { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Line { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> EntrySignalApi { get; set; }

        private Plot _plot;
        private Plot _signalPlot;

        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                SetDefaults();
                CreatePlots();

            }
            else if (State == State.Configured)
            {
                
                AssignSeries();
                _aboveMaCount = 0;
                _belowMaCount = 0;
            }
        }

        private void SetDefaults()
        {
            Name = IndicatorName + IndicatorVersion;
            Version = IndicatorVersion;

            LookBackBars = 3;
            EntryAngle = 20;
        }

        private void CreatePlots()
        {
            _plot = new Plot() { Thickness = 1, Color = Colors.Blue, Name = "Line" };
            _plot.ChartType = PlotChartType.Linear;
            _plot.PlotColors = new ColorMap(DataCalcContext);
            AddPanePlot(_plot);

            _signalPlot = new Plot() { Thickness = 1, Color = Colors.Transparent, Name = "Signal" };
            AddPanePlot(_signalPlot);
        }

        private void AssignSeries()
        {
            Line = new Series<double>();
            EntrySignalApi = new Series<double>();
            
            AddSeries(Line);
            AddSeries(EntrySignalApi);

            _plot.DataSource = Line;
            _signalPlot.DataSource = EntrySignalApi;
        }

        private int _aboveMaCount;
        private int _belowMaCount;

        public override void OnBarUpdate()
        {
            if (CurrentBar < LookBackBars)
                return;

            Line[0] = Input[0];
            EntrySignalApi[0] = 0;

            if (Close[0] > Input[0])
            {
                _aboveMaCount++;
                _belowMaCount = 0;

                var deltaY = GetDeltaY(_aboveMaCount - 1);

                if (deltaY > 0 && _aboveMaCount > LookBackBars)
                {
                    var degrees = GetDegrees(_aboveMaCount - 1, deltaY);

                    if (degrees > EntryAngle)
                    {
                        _plot.PlotColors[0] = Brushes.ForestGreen;
                        EntrySignalApi[0] = 1;
                    }
                    else if (degrees < EntryAngle)
                    {
                        _plot.PlotColors[0] = Brushes.DimGray;
                    }

                }
                else
                {
                    _plot.PlotColors[0] = Brushes.DimGray;
                }


            }
            else if (Close[0] < Input[0])
            {
                _belowMaCount++;
                _aboveMaCount = 0;

                var deltaY = GetDeltaY(_belowMaCount - 1);

                if (deltaY < 0 && _belowMaCount > LookBackBars)
                {
                    var degrees = GetDegrees(_belowMaCount - 1, deltaY * -1);

                    if (degrees > EntryAngle)
                    {
                        _plot.PlotColors[0] = Brushes.Red;
                        EntrySignalApi[0] = -1;
                    }
                    else if (degrees < EntryAngle)
                    {
                        _plot.PlotColors[0] = Brushes.DimGray;
                    }

                }
                else
                {
                    _plot.PlotColors[0] = Brushes.DimGray;
                }

            }

        }

        private double GetDeltaY(int bars)
        {
            return Input[0] - Input[bars];
        }

        private double GetDegrees(int bars, double changeInY)
        {
            var changeInX = bars;
            //var changeInY = _ma[0] - _ma[bars];
            var delta = changeInY / changeInX;
            var alphaRadians = Math.Atan(delta);
            var alphaDegrees = alphaRadians * 180 / Math.PI;
            //Print(Time[0] + " Time Start: "+ Time[bars] + " DX " + changeInX + " DY0 " + _ma[0] + " DY1 " + _ma[bars] + " delta " + delta + " alpha Degrees: " + alphaDegrees);
            return alphaDegrees;

            //Draw.ArrowDown(this, "Start", true, LookBackBars, High[0], Brushes.RoyalBlue);
        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public LineAngle LineAngle(int lookBackBars, int entryAngle, ScriptOptions options = null)
		{
			return LineAngle(Input, lookBackBars, entryAngle, options);
		}

		public LineAngle LineAngle(ISeries<double> input, int lookBackBars, int entryAngle, ScriptOptions options = null)
		{
			IEnumerable<LineAngle> cache = DataCalcContext.GetIndicatorCache<LineAngle>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.LookBackBars == lookBackBars && cacheIndicator.EntryAngle == entryAngle)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<LineAngle>(input, false, options);
			indicator.LookBackBars = lookBackBars;
			indicator.EntryAngle = entryAngle;
			indicator.ChangeState();
			return indicator;
		}
	}
}
namespace Twm.Custom.Strategies
{
	public partial class Strategy : Twm.Core.DataCalc.StrategyBase
	{
		public LineAngle LineAngle(int lookBackBars, int entryAngle, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.LineAngle(Input, lookBackBars, entryAngle, options);
		}

		public LineAngle LineAngle(ISeries<double> input, int lookBackBars, int entryAngle, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.LineAngle(input, lookBackBars, entryAngle, options);
		}
	}
}

#endregion
