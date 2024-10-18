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
    public class VWAP : Indicator
    {
        private const string IndicatorName = "VWAP";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

        [TwmProperty]
        [Display(Name = "Period", GroupName = Parameters, Order = 2)]
        public int Period { get; set; }

        [TwmProperty]
        [Display(Name = "Session Start Hour", GroupName = Parameters, Order = 4)]
        public int SessionStartHour { get; set; }

        [TwmProperty]
        [Display(Name = "Session Start Min", GroupName = Parameters, Order = 5)]
        public int SessionStartMin { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> VWAPVal { get; set; }


        private Plot _plot;
        double _iCumVolume = 0;
        double _iCumTypicalVolume = 0;

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
                _iCumVolume = 0;
                _iCumTypicalVolume = 0;
            }
        }

        private void SetDefaultParameterValues()
        {
            Name = IndicatorName;
            Version = IndicatorVersion;
            Period = 14;
            SessionStartHour = 18;
            SessionStartMin = 0;
        }

        private void CreatePlots()
        {
            _plot = new Plot() { Thickness = 1, Color = Colors.Blue };
            _plot.ChartType = PlotChartType.Linear;
            _plot.PlotColors = new ColorMap(DataCalcContext);
            AddPanePlot(_plot);
        }

        private void AssignSeries()
        {
            VWAPVal = new Series<double>();
            AddSeries(VWAPVal);
            _plot.DataSource = VWAPVal;
        }

        public override void OnBarUpdate()
        {
            if (IsFirstBarOfSession())
            {
                
                _iCumVolume = Volume[0];
                _iCumTypicalVolume = Volume[0] * ((High[0] + Low[0] + Close[0]) / 3);
            }
            else
            {
                _iCumVolume = _iCumVolume + Volume[0];
                _iCumTypicalVolume = _iCumTypicalVolume + (Volume[0] * ((High[0] + Low[0] + Close[0]) / 3));
            }

            VWAPVal[0] = (_iCumTypicalVolume / _iCumVolume);
        }

        private bool IsFirstBarOfSession()
        {
            var timeNow = DateTime[0];
            var sessionStart = new DateTime(timeNow.Year, timeNow.Month, timeNow.Day, SessionStartHour, SessionStartMin,
                0);

            if (timeNow == sessionStart)
            {
                return true;
            }

            return false;
        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public VWAP VWAP(int period, int sessionStartHour, int sessionStartMin, ScriptOptions options = null)
		{
			return VWAP(Input, period, sessionStartHour, sessionStartMin, options);
		}

		public VWAP VWAP(ISeries<double> input, int period, int sessionStartHour, int sessionStartMin, ScriptOptions options = null)
		{
			IEnumerable<VWAP> cache = DataCalcContext.GetIndicatorCache<VWAP>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Period == period && cacheIndicator.SessionStartHour == sessionStartHour && cacheIndicator.SessionStartMin == sessionStartMin)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<VWAP>(input, false, options);
			indicator.Period = period;
			indicator.SessionStartHour = sessionStartHour;
			indicator.SessionStartMin = sessionStartMin;
			indicator.ChangeState();
			return indicator;
		}
	}
}
namespace Twm.Custom.Strategies
{
	public partial class Strategy : Twm.Core.DataCalc.StrategyBase
	{
		public VWAP VWAP(int period, int sessionStartHour, int sessionStartMin, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.VWAP(Input, period, sessionStartHour, sessionStartMin, options);
		}

		public VWAP VWAP(ISeries<double> input, int period, int sessionStartHour, int sessionStartMin, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.VWAP(input, period, sessionStartHour, sessionStartMin, options);
		}
	}
}

#endregion
