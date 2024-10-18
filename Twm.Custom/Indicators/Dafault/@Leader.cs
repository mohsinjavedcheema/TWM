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
    public class Leader : Indicator
    {
        private const string IndicatorName = "Leader";
        private const string IndicatorVersion = " 1.0";

        private const string Parameters = "Parameters";
        private const string Visuals = "Visuals";



        [TwmProperty]
        [Display(Name = "Fast", GroupName = Parameters, Order = 2)]
        public int Fast { get; set; }

        [TwmProperty]
        [Display(Name = "Slow", GroupName = Parameters, Order = 3)]
        public int Slow { get; set; }


        [Browsable(false)]
        [XmlIgnore]
        public Series<double> LeaderValues { get; set; }



        private Plot _leader;
        private Series<double> mySeries1;
        private Series<double> mySeries2;

        private EMA _emaFast;
        private EMA _emaSlow;
        private EMA _emaSeriesFast;
        private EMA _emaSeriesSlow;

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
                AssignSeries();

                var options = new ScriptOptions();
                options.ShowPanes = false;
                options.ShowPlots = false;

                mySeries1 = new Series<double>();
                AddSeries(mySeries1);
                mySeries2 = new Series<double>();
                AddSeries(mySeries2);

                _emaFast = EMA(Fast, options);
                _emaSlow = EMA(Slow, options);

                _emaSeriesFast = EMA(mySeries1, Fast, options);
                _emaSeriesSlow = EMA(mySeries2, Slow, options);
            }
        }

        private void SetDefaultParameterValues()
        {
            Name = IndicatorName;
            Fast = 12;
            Slow = 26;
        }

        private void CreatePlots()
        {
            _leader = new Plot(Colors.Red, "Leader", 1);
            _leader.Name = "Leader";

            var secondPane = AddPane();
            AddPanePlot(secondPane, _leader);
            
        }

        private void AssignSeries()
        {
            LeaderValues = new Series<double>();
            _leader.DataSource = LeaderValues;
            AddSeries(LeaderValues);

          
        }

        
        public override void OnBarUpdate()
        {
            mySeries1[0] = (Close[0] - _emaFast[0]);
            mySeries2[0] = (Close[0] - _emaSlow[0]);

            LeaderValues[0] = (_emaFast[0] + _emaSeriesFast[0]) - (_emaSlow[0] + _emaSeriesSlow[0]);
        }
    }











}

#region Twm generated code. Neither change nor remove.

namespace Twm.Custom.Indicators
{
	public partial class Indicator : Twm.Core.DataCalc.IndicatorBase
	{
		public Leader Leader(int fast, int slow, ScriptOptions options = null)
		{
			return Leader(Input, fast, slow, options);
		}

		public Leader Leader(ISeries<double> input, int fast, int slow, ScriptOptions options = null)
		{
			IEnumerable<Leader> cache = DataCalcContext.GetIndicatorCache<Leader>();
			foreach (var cacheIndicator in cache)
			{
				if (cacheIndicator.EqualsInput(input) && cacheIndicator.Fast == fast && cacheIndicator.Slow == slow)
					return cacheIndicator;
			}
			var indicator = DataCalcContext.CreateIndicator<Leader>(input, false, options);
			indicator.Fast = fast;
			indicator.Slow = slow;
			indicator.ChangeState();
			return indicator;
		}
	}
}
namespace Twm.Custom.Strategies
{
	public partial class Strategy : Twm.Core.DataCalc.StrategyBase
	{
		public Leader Leader(int fast, int slow, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.Leader(Input, fast, slow, options);
		}

		public Leader Leader(ISeries<double> input, int fast, int slow, ScriptOptions options = null)
		{
			indicator.SetDataCalcContext(GetDataCalcContext());
			return indicator.Leader(input, fast, slow, options);
		}
	}
}

#endregion
