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
    public class ADXSuit : Indicator
    {
        private const string IndicatorName = "ADX Suit";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period", GroupName = "Parameters", Order = 10)]
        public int Period
        { get; set; }

        public enum MyView
        {
            TrueValues,
            Indexes
        }

        [TwmProperty]
        [Display(Name = "View", GroupName = "Parameters", Order = 11)]
        public ADXSuit.MyView SelectedView
        { get; set; }



        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> TrueRange { get; set; }

        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> PeriodTR { get; set; }


        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> DmPlus { get; set; }


        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> DmMinus { get; set; }


        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> PeriodDmPlus { get; set; }


        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> PeriodDmMinus { get; set; }


        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> DiPlus { get; set; }


        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> DiMinus { get; set; }


        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> DX { get; set; }


        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> ADX { get; set; }

        public PaneControl SecondPane;

        private Plot _trueRange;
        private Plot _periodTR;
        private Plot _dmPlus;
        private Plot _dmMinus;
        private Plot _periodDmPlus;
        private Plot _periodDmMinus;
        private Plot _diPlus;
        private Plot _diMinus;
        private Plot _dx;
        private Plot _adx;

        private List<double> _initTrs = new List<double>();
        private List<double> _dxs = new List<double>();
        private List<double> _initDmPlus = new List<double>();
        private List<double> _initDmMinus = new List<double>();


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
                ClearLists();
                AssignSeries();
                


                if (SelectedView == MyView.TrueValues)
                {
                    _trueRange.Color = Colors.Blue;
                    _dmPlus.Color = Colors.GreenYellow;
                    _dmMinus.Color = Colors.Crimson;
                    _periodDmPlus.Color = Colors.GreenYellow;
                    _periodDmMinus.Color = Colors.Crimson;

                    _diMinus.Color = Colors.Transparent;
                    _diPlus.Color = Colors.Transparent;
                    _adx.Color = Colors.Transparent;
                }

                if (SelectedView == MyView.Indexes)
                {
                    _trueRange.Color = Colors.Transparent;
                    _dmPlus.Color = Colors.Transparent;
                    _dmMinus.Color = Colors.Transparent;
                    _periodDmPlus.Color = Colors.Transparent;
                    _periodDmMinus.Color = Colors.Transparent;

                    _diMinus.Color = Colors.Crimson;
                    _diPlus.Color = Colors.GreenYellow;
                    _adx.Color = Colors.Blue;
                }
            }
        }

        private void ClearLists()
        {
            _initTrs.Clear();
            _dxs.Clear();
            _initDmPlus.Clear();
            _initDmMinus.Clear();
        }

        private void SetDefaultParameterValues()
        {
            Name = IndicatorName;
            Period = 14;
        }

        private void CreatePlots()
        {
            _trueRange = new Plot();
            _trueRange.Color = Colors.Blue;
            _trueRange.ChartType = PlotChartType.Bars;
            _trueRange.Name = "True Range";

            _periodTR = new Plot();
            _periodTR.Color = Colors.Transparent;
            _periodTR.Name = "N-True Range";

            _dmPlus = new Plot();
            _dmPlus.Name = "DM+";
            _dmPlus.ChartType = PlotChartType.Bars;
            _dmPlus.Color = Colors.GreenYellow;

            _dmMinus = new Plot();
            _dmMinus.Name = "DM-";
            _dmMinus.ChartType = PlotChartType.Bars;
            _dmMinus.Color = Colors.Crimson;

            _periodDmPlus = new Plot();
            _periodDmPlus.Name = "N-DM+";
            _periodDmPlus.Color = Colors.GreenYellow;
            _periodDmPlus.LineType = PlotLineType.Dashed;

            _periodDmMinus = new Plot();
            _periodDmMinus.Name = "N-DM-";
            _periodDmMinus.Color = Colors.Crimson;
            _periodDmMinus.LineType = PlotLineType.Dashed;

            _diPlus = new Plot(Colors.GreenYellow, "DI+",2, PlotLineType.Solid);

            _diMinus = new Plot(Colors.Crimson, "DI-",2, PlotLineType.Solid);

            _dx = new Plot(Colors.Transparent, "DX");
            _adx = new Plot(Colors.Blue, "ADX",3, PlotLineType.Solid);


            SecondPane = AddPane();
            AddPanePlot(SecondPane, _trueRange);
            AddPanePlot(SecondPane, _periodTR);
            AddPanePlot(SecondPane, _dmPlus);
            AddPanePlot(SecondPane, _dmMinus);
            AddPanePlot(SecondPane, _periodDmPlus);
            AddPanePlot(SecondPane, _periodDmMinus);
            AddPanePlot(SecondPane, _diPlus);
            AddPanePlot(SecondPane, _diMinus);
            AddPanePlot(SecondPane, _dx);
            AddPanePlot(SecondPane, _adx);
        }

        private void AssignSeries()
        {

            TrueRange = new Series<double>();
            _trueRange.DataSource = TrueRange;
            AddSeries(TrueRange);

            PeriodTR = new Series<double>();
            _periodTR.DataSource = PeriodTR;
            AddSeries(PeriodTR);

            DmPlus = new Series<double>();
            _dmPlus.DataSource = DmPlus;
            AddSeries(DmPlus);

            DmMinus = new Series<double>();
            _dmMinus.DataSource = DmMinus;
            AddSeries(DmMinus);

            PeriodDmPlus = new Series<double>();
            _periodDmPlus.DataSource = PeriodDmPlus;
            AddSeries(PeriodDmPlus);

            PeriodDmMinus = new Series<double>();
            _periodDmMinus.DataSource = PeriodDmMinus;
            AddSeries(PeriodDmMinus);

            DiPlus = new Series<double>();
            _diPlus.DataSource = DiPlus;
            AddSeries(DiPlus);

            DiMinus = new Series<double>();
            _diMinus.DataSource = DiMinus;
            AddSeries(DiMinus);

            DX = new Series<double>();
            _dx.DataSource = DX;
            AddSeries(DX);

            ADX = new Series<double>();
            _adx.DataSource = ADX;
            AddSeries(ADX);

        }


        public override void OnBarUpdate()
        {
            if (CurrentBar < 2)
                return;

            CalculateTrueRange();
            CalculateDM();
            CalculateDI();
            CalculateADX();
        }

        private void CalculateTrueRange()
        {
            var v1 = High[0] - Low[0];
            var v2 = Math.Abs(High[0] - Close[1]);
            var v3 = Math.Abs(Low[0] - Close[1]);

            var max = Math.Max(v1, v2);
            max = Math.Max(max, v3);

            TrueRange[0] = max;

            if (CurrentBar + 1 < Period)
            {
                _initTrs.Add(TrueRange[0]);
            }
            else if (CurrentBar + 1 == Period)
            {
                _initTrs.Add(TrueRange[0]);
                var sum = _initTrs.Sum();
                PeriodTR[0] = sum;
            }
            else
            {
                PeriodTR[0] = PeriodTR[1] - PeriodTR[1] / Period + TrueRange[0];
            }
        }

        private void CalculateDM()
        {
            if (High[0] > High[1] && Low[0] < Low[1])
            {
                DmPlus[0] = High[0] - High[1];
                DmMinus[0] = Low[1] - Low[0];

                if (DmPlus[0] > DmMinus[0])
                {
                    DmMinus[0] = 0;
                }
                else if (DmPlus[0] < DmMinus[0])
                {
                    DmPlus[0] = 0;
                }
            }
            else if (High[0] <= High[1] && Low[0] >= Low[1])
            {
                DmPlus[0] = 0;
                DmMinus[0] = 0;
            }
            else if (High[0] > High[1] && Low[0] >= Low[1])
            {
                DmPlus[0] = High[0] - High[1];
                DmMinus[0] = 0;
            }
            else if (Low[0] < Low[1] && High[0] <= High[1])
            {
                DmPlus[0] = 0;
                DmMinus[0] = Low[1] - Low[0];
            }

            if (CurrentBar + 1 < Period)
            {
                _initDmPlus.Add(DmPlus[0]);
                _initDmMinus.Add(DmMinus[0]);
            }
            else if (CurrentBar + 1 == Period)
            {
                _initDmPlus.Add(DmPlus[0]);
                _initDmMinus.Add(DmMinus[0]);

                PeriodDmPlus[0] = _initDmPlus.Sum();
                PeriodDmMinus[0] = _initDmMinus.Sum(); ;
            }
            else
            {
                PeriodDmPlus[0] = PeriodDmPlus[1] - PeriodDmPlus[1] / Period + DmPlus[0];
                PeriodDmMinus[0] = PeriodDmMinus[1] - PeriodDmMinus[1] / Period + DmMinus[0];
            }

            //Print("DMPLUS: " + DmPlus[0] + " DMMINUS: "+ DmMinus[0]);
        }

        private void CalculateDI()
        {
            if (CurrentBar + 1 < Period)
            {
                DiPlus[0] = 0;
                DiMinus[0] = 0;
            }
            else
            {
                DiPlus[0] = PeriodDmPlus[0] / PeriodTR[0] * 100;
                DiMinus[0] = PeriodDmMinus[0] / PeriodTR[0] * 100;
            }

        }

        private void CalculateADX()
        {
            var diff = Math.Abs(DiPlus[0] - DiMinus[0]);
            var sum = DiPlus[0] + DiMinus[0];

            if (sum == 0)
            {
                DX[0] = 0;
            }
            else
            {
                DX[0] = diff / sum * 100;
            }


            if (CurrentBar + 1 < Period)
            {
                _dxs.Add(DX[0]);
            }
            else if (CurrentBar + 1 == Period)
            {
                ADX[0] = _dxs.Average();
            }
            else
            {
                var val = (ADX[1] * (Period - 1) + DX[0]) / Period;
                ADX[0] = val;
            }

        }
    }











}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public ADXSuit ADXSuit(int period, ADXSuit.MyView selectedView, ScriptOptions options = null)
		{
			return ADXSuit(Input, period, selectedView, options);
		}

		public ADXSuit ADXSuit(ISeries<double> input, int period, ADXSuit.MyView selectedView, ScriptOptions options = null)
		{
			IEnumerable<ADXSuit> cache = DataCalcContext.GetIndicatorCache<ADXSuit>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period && cacheIndicator.SelectedView == selectedView)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<ADXSuit>(input, false, options);
			indicator.Period = period;
			indicator.SelectedView = selectedView;
			indicator.ChangeState();
			return indicator;
		}
	}
}
namespace Twm.Custom.Strategies
{
	public partial class Strategy : Twm.Core.DataCalc.StrategyBase
	{
		public ADXSuit ADXSuit(int period, ADXSuit.MyView selectedView, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.ADXSuit(Input, period, selectedView, options);
		}

		public ADXSuit ADXSuit(ISeries<double> input, int period, ADXSuit.MyView selectedView, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.ADXSuit(input, period, selectedView, options);
		}
	}
}

#endregion
