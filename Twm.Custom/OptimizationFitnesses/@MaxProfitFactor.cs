#region Using declarations

using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Optimization;
using Twm.Core.Enums;

#endregion

namespace Twm.Custom.OptimizationFitnesses
{
	public class MaxProfitFactor : OptimizationFitness
    {
        private const string OptimizationFitnessName = "Max Profit Factor";

        public override void OnCalculatePerformanceValue(StrategyBase strategy)
        {
            Value = strategy.SystemPerformance.Summary.GetValue(AnalyticalFeature.ProfitFactor);

        }

        
    }
}
