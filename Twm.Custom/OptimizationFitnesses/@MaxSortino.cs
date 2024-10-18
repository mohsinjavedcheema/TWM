#region Using declarations

using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Optimization;
using Twm.Core.Enums;

#endregion

namespace Twm.Custom.OptimizationFitnesses
{
    public class MaxSortino : OptimizationFitness
    {
        private const string OptimizationFitnessName = "Max Sortino";

        public override void OnCalculatePerformanceValue(StrategyBase strategy)
        {
            Value = strategy.SystemPerformance.Summary.GetValue(AnalyticalFeature.Sortino);

        }


    }
}