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
    public class Stochastics : Indicator
    {
        private const string IndicatorName = "Stochastics";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period K", GroupName = Parameters, Order = 2)]
        public int PeriodK { get; set; }

        [TwmProperty]
        [Display(Name = "Period D", GroupName = Parameters, Order = 3)]
        public int PeriodD { get; set; }


        [TwmProperty]
        [Display(Name = "Smooth", GroupName = Parameters, Order = 4)]
        public int Smooth { get; set; }

        [TwmProperty]
        [Display(Name = "Threshold", GroupName = Parameters, Order = 5)]
        public int Threshold { get; set; }


        [Browsable(false)]
        [XmlIgnore]
        public Series<double> K { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> D { get; set; }

        private Series<double> den;
        private Series<double> fastK;
        private Series<double> min;
        private Series<double> max;
        private Series<double> nom;
        private SMA smaFastK;
        private SMA smaK;
        private Plot _k;
        private Plot _d;

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

                K = new Series<double>();
                _k.DataSource = K;
                _k.PlotColors = new ColorMap(DataCalcContext);
                AddSeries(K);

                D = new Series<double>();
                _d.DataSource = D;
                _d.PlotColors = new ColorMap(DataCalcContext);
                AddSeries(D);

                den = new Series<double>();
                nom = new Series<double>();
                fastK = new Series<double>();
                AddSeries(den);
                AddSeries(nom);
                AddSeries(fastK);

                var options = new ScriptOptions();
                options.ShowPanes = false;
                options.ShowPlots = false;

                min = Min(Low, PeriodK, options).MinVal;
                max = Max(High, PeriodK, options).MaxVal;

                smaFastK = SMA(fastK, Smooth);
                smaK = SMA(K, PeriodD);

                Draw.LineHorizontal("UpBoundary", 100 - Threshold, Brushes.LightGray, 1, DashStyles.Dash, _secondPane);
                Draw.LineHorizontal("DownBoundary", Threshold, Brushes.LightGray, 1, DashStyles.Dash, _secondPane);
            }
        }

        private void SetDefaultParameterValues()
        {
            Name = IndicatorName;
            PeriodK = 14;
            PeriodD = 7;
            Smooth = 3;
            Threshold = 20;
        }

        private PaneControl _secondPane;

        private void CreatePlots()
        {
            _k = new Plot(Colors.Red, "K", 1, PlotLineType.Dashed, PlotChartType.Linear);
            _d = new Plot(Colors.Blue, "D", 1, PlotLineType.Solid, PlotChartType.Linear);

            _secondPane = AddPane();
            AddPanePlot(_secondPane, _k);
            AddPanePlot(_secondPane, _d);
        }


        public override void OnBarUpdate()
        {
            double min0 = min[0];
            nom[0] = Close[0] - min0;
            den[0] = max[0] - min0;

            if (den[0] == 0)
                fastK[0] = CurrentBar == 1 ? 50 : fastK[1];
            else
                fastK[0] = Math.Min(100, Math.Max(0, 100 * nom[0] / den[0]));

            // Slow %K == Fast %D
            K[0] = smaFastK[0];
            D[0] = smaK[0];
            //D[0] = smaK[0];
        }
    }











}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public Stochastics Stochastics(int periodK, int periodD, int smooth, int threshold, ScriptOptions options = null)
		{
			return Stochastics(Input, periodK, periodD, smooth, threshold, options);
		}

		public Stochastics Stochastics(ISeries<double> input, int periodK, int periodD, int smooth, int threshold, ScriptOptions options = null)
		{
			IEnumerable<Stochastics> cache = DataCalcContext.GetIndicatorCache<Stochastics>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.PeriodK == periodK && cacheIndicator.PeriodD == periodD && cacheIndicator.Smooth == smooth && cacheIndicator.Threshold == threshold)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<Stochastics>(input, false, options);
			indicator.PeriodK = periodK;
			indicator.PeriodD = periodD;
			indicator.Smooth = smooth;
			indicator.Threshold = threshold;
			indicator.ChangeState();
			return indicator;
		}
	}
}
namespace Twm.Custom.Strategies
{
	public partial class Strategy : Twm.Core.DataCalc.StrategyBase
	{
		public Stochastics Stochastics(int periodK, int periodD, int smooth, int threshold, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.Stochastics(Input, periodK, periodD, smooth, threshold, options);
		}

		public Stochastics Stochastics(ISeries<double> input, int periodK, int periodD, int smooth, int threshold, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.Stochastics(input, periodK, periodD, smooth, threshold, options);
		}
	}
}

#endregion
