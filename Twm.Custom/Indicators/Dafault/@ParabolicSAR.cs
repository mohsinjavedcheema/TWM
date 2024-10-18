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
    public class ParabolicSAR : Indicator
    {
        private const string IndicatorName = "Parabolic SAR";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Acceleration", GroupName = Parameters, Order = 2)]
        public double Acceleration { get; set; }

        [TwmProperty]
        [Display(Name = "Acceleration Max", GroupName = Parameters, Order = 3)]
        public double AccelerationMax { get; set; }

        [TwmProperty]
        [Display(Name = "Acceleration Step", GroupName = Parameters, Order = 4)]
        public double AccelerationStep { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ParabolicValues { get; set; }


        private double af;              // Acceleration factor
        private bool afIncreased;
        private bool longPosition;
        private double prevBar;
        private double prevSAR;
        private double reverseBar;
        private double reverseValue;
        private double todaySAR;        // SAR value
        private double xp;				// Extreme Price
        private Plot _plot;
        private Series<double> afSeries;
        private List<bool> afIncreasedSeries;
        private List<bool> longPositionSeries;
        private Series<double> prevBarSeries;
        private Series<double> prevSARSeries;
        private Series<double> reverseBarSeries;
        private Series<double> reverseValueSeries;
        private Series<double> todaySARSeries;
        private Series<double> xpSeries;

        private ScriptOptions _options;

        public override void OnStateChanged()
        {
            if (State == State.SetDefaults)
            {
                SetDefaultParameterValues();
                CreatePlots();
            }
            else if (State == State.Configured)
            {

                AssignSeries();

                xp = 0.0;
                af = 0;
                todaySAR = 0;
                prevSAR = 0;
                reverseBar = 0;
                reverseValue = 0;
                prevBar = 0;
                afIncreased = false;

                afSeries = new Series<double>();
                AddSeries(afSeries);
                prevBarSeries = new Series<double>();
                AddSeries(prevBarSeries);
                prevSARSeries = new Series<double>();
                AddSeries(prevSARSeries);
                reverseBarSeries = new Series<double>();
                AddSeries(reverseBarSeries);
                reverseValueSeries = new Series<double>();
                AddSeries(reverseValueSeries);
                todaySARSeries = new Series<double>();
                AddSeries(todaySARSeries);
                xpSeries = new Series<double>();
                AddSeries(xpSeries);

                afIncreasedSeries = new List<bool>();
                //AddSeries(afIncreasedSeries);
                longPositionSeries = new List<bool>();
                //AddSeries(longPositionSeries);

                _options = new ScriptOptions();
                _options.ShowPanes = false;
                _options.ShowPlots = false;
            }
        }

        private void SetDefaultParameterValues()
        {
            Name = IndicatorName;
            Version = IndicatorVersion;
            Acceleration = 0.02;
            AccelerationStep = 0.02;
            AccelerationMax = 0.2;
        }

        private void CreatePlots()
        {
            _plot = new Plot() { Thickness = 1, Color = Colors.Blue, Name = "Parabolic SAR"};
            _plot.ChartType = PlotChartType.Linear;
            _plot.LineType = PlotLineType.Dashed;
            _plot.PlotColors = new ColorMap(DataCalcContext);
            AddPanePlot(_plot);
        }

        private void AssignSeries()
        {
            ParabolicValues = new Series<double>();
            AddSeries(ParabolicValues);
            _plot.DataSource = ParabolicValues;
        }

        public override void OnBarUpdate()
        {
            if (CurrentBar < 4)
                return;


            if (CurrentBar == 4)
            {
                var max = Max(High, CurrentBar, _options)[0];
                var min = Min(Low, CurrentBar, _options)[0];

                // Determine initial position
                longPosition = High[0] > High[1];
                xp = longPosition ? max : min;
                af = Acceleration;
                ParabolicValues[0] = xp + (longPosition ? -1 : 1) * ((max - min) * af);

                return;
            }
            
            if (CurrentBar < prevBar)
            {
                af = afSeries[0];
                afIncreased = afIncreasedSeries[afIncreasedSeries.Count-1];
                longPosition = longPositionSeries[longPositionSeries.Count-1];
                prevBar = prevBarSeries[0];
                prevSAR = prevSARSeries[0];
                reverseBar = reverseBarSeries[0];
                reverseValue = reverseValueSeries[0];
                todaySAR = todaySARSeries[0];
                xp = xpSeries[0];
            }

            // Reset accelerator increase limiter on new bars
            if (afIncreased && prevBar != CurrentBar)
                afIncreased = false;

            // Current event is on a bar not marked as a reversal bar yet
            if (reverseBar != CurrentBar)
            {
                // SAR = SAR[1] + af * (xp - SAR[1])
                todaySAR = TodaySAR(ParabolicValues[1] + af * (xp - ParabolicValues[1]));
                for (int x = 1; x <= 2; x++)
                {
                    if (longPosition)
                    {
                        if (todaySAR > Low[x])
                            todaySAR = Low[x];
                    }
                    else
                    {
                        if (todaySAR < High[x])
                            todaySAR = High[x];
                    }
                }

                // Holding long position
                if (longPosition)
                {
                    // Process a new SAR value only on a new bar or if SAR value was penetrated.
                    if (prevBar != CurrentBar || Low[0] < prevSAR)
                    {
                        ParabolicValues[0] = todaySAR;
                        prevSAR = todaySAR;
                    }
                    else
                        ParabolicValues[0] = prevSAR;

                    if (High[0] > xp)
                    {
                        xp = High[0];
                        AfIncrease();
                    }
                }

                // Holding short position
                else if (!longPosition)
                {
                    // Process a new SAR value only on a new bar or if SAR value was penetrated.
                    if (prevBar != CurrentBar || High[0] > prevSAR)
                    {
                        ParabolicValues[0] = todaySAR;
                        prevSAR = todaySAR;
                    }
                    else
                        ParabolicValues[0] = prevSAR;

                    if (Low[0] < xp)
                    {
                        xp = Low[0];
                        AfIncrease();
                    }
                }
            }

            // Current event is on the same bar as the reversal bar
            else
            {
                // Only set new xp values. No increasing af since this is the first bar.
                if (longPosition && High[0] > xp)
                    xp = High[0];
                else if (!longPosition && Low[0] < xp)
                    xp = Low[0];

                ParabolicValues[0] = prevSAR;

                // SAR = SAR[1] + af * (xp - SAR[1])
                todaySAR = TodaySAR(longPosition ? Math.Min(reverseValue, Low[0]) : Math.Max(reverseValue, High[0]));
            }

            prevBar = CurrentBar;

            // Reverse position
            if ((longPosition && (Low[0] < todaySAR || Low[1] < todaySAR))
                || (!longPosition && (High[0] > todaySAR || High[1] > todaySAR)))
                ParabolicValues[0] = Reverse();

            afSeries[0] = af;
            afIncreasedSeries.Add(afIncreased);
            longPositionSeries.Add(longPosition);
            prevBarSeries[0] = prevBar;
            prevSARSeries[0] = prevSAR;
            reverseBarSeries[0] = reverseBar;
            reverseValueSeries[0] = reverseValue;
            todaySARSeries[0] = todaySAR;
            xpSeries[0] = xp;
        }

        private void AfIncrease()
        {
            if (!afIncreased)
            {
                af = Math.Min(AccelerationMax, af + AccelerationStep);
                afIncreased = true;
            }
        }

        // Additional rule. SAR for today can't be placed inside the bar of day - 1 or day - 2.
        private double TodaySAR(double todaySAR)
        {
            if (longPosition)
            {
                double lowestSAR = Math.Min(Math.Min(todaySAR, Low[0]), Low[1]);
                if (Low[0] > lowestSAR)
                    todaySAR = lowestSAR;
            }
            else
            {
                double highestSAR = Math.Max(Math.Max(todaySAR, High[0]), High[1]);
                if (High[0] < highestSAR)
                    todaySAR = highestSAR;
            }
            return todaySAR;
        }

        private double Reverse()
        {
            double todaySAR = xp;

            if ((longPosition && prevSAR > Low[0]) || (!longPosition && prevSAR < High[0]) || prevBar != CurrentBar)
            {
                longPosition = !longPosition;
                reverseBar = CurrentBar;
                reverseValue = xp;
                af = Acceleration;
                xp = longPosition ? High[0] : Low[0];
                prevSAR = todaySAR;
            }
            else
                todaySAR = prevSAR;
            return todaySAR;
        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public ParabolicSAR ParabolicSAR(double acceleration, double accelerationMax, double accelerationStep, ScriptOptions options = null)
		{
			return ParabolicSAR(Input, acceleration, accelerationMax, accelerationStep, options);
		}

		public ParabolicSAR ParabolicSAR(ISeries<double> input, double acceleration, double accelerationMax, double accelerationStep, ScriptOptions options = null)
		{
			IEnumerable<ParabolicSAR> cache = DataCalcContext.GetIndicatorCache<ParabolicSAR>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Acceleration == acceleration && cacheIndicator.AccelerationMax == accelerationMax && cacheIndicator.AccelerationStep == accelerationStep)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<ParabolicSAR>(input, false, options);
			indicator.Acceleration = acceleration;
			indicator.AccelerationMax = accelerationMax;
			indicator.AccelerationStep = accelerationStep;
			indicator.ChangeState();
			return indicator;
		}
	}
}
namespace Twm.Custom.Strategies
{
	public partial class Strategy : Twm.Core.DataCalc.StrategyBase
	{
		public ParabolicSAR ParabolicSAR(double acceleration, double accelerationMax, double accelerationStep, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.ParabolicSAR(Input, acceleration, accelerationMax, accelerationStep, options);
		}

		public ParabolicSAR ParabolicSAR(ISeries<double> input, double acceleration, double accelerationMax, double accelerationStep, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.ParabolicSAR(input, acceleration, accelerationMax, accelerationStep, options);
		}
	}
}

#endregion
