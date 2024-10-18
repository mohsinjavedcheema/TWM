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
    public class Typical : Indicator
    {
        private const string IndicatorName = "Typical";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";

       
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> TypicalValues { get; set; }


        private double constant1;
        private double constant2;
        private Plot _plot;

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
            }
        }

        private void SetDefaultParameterValues()
        {
            Name = IndicatorName;
            Version = IndicatorVersion;
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
            TypicalValues = new Series<double>();
            AddSeries(TypicalValues);
            _plot.DataSource = TypicalValues;
        }

        public override void OnBarUpdate()
        {
            TypicalValues[0] = (High[0] + Low[0] + Close[0]) / 3;
        }
    }
}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public Typical Typical(ScriptOptions options = null)
		{
			return Typical(Input, options);
		}

		public Typical Typical(ISeries<double> input, ScriptOptions options = null)
		{
			IEnumerable<Typical> cache = DataCalcContext.GetIndicatorCache<Typical>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input))
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<Typical>(input, false, options);
			indicator.ChangeState();
			return indicator;
		}
	}
}
namespace Twm.Custom.Strategies
{
	public partial class Strategy : Twm.Core.DataCalc.StrategyBase
	{
		public Typical Typical(ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.Typical(Input, options);
		}

		public Typical Typical(ISeries<double> input, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.Typical(input, options);
		}
	}
}

#endregion
