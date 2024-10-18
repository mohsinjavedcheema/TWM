#region Using declarations

using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Optimization;
using Twm.Core.Enums;

#endregion

namespace Twm.Custom.OptimizationFitnesses
{
    public class MaxSharp : OptimizationFitness
    {
        private const string OptimizationFitnessName = "Max Sharpe";

        public override void OnCalculatePerformanceValue(StrategyBase strategy)
        {
            Value = strategy.SystemPerformance.Summary.GetValue(AnalyticalFeature.Sharpe);
        }


    }
}