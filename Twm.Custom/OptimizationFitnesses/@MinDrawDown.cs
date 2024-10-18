#region Using declarations

using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Optimization;
using Twm.Core.Enums;

#endregion

namespace Twm.Custom.OptimizationFitnesses
{
    public class MinDrawDown : OptimizationFitness
    {
        private const string OptimizationFitnessName = "Min Draw Down";

        public override void OnCalculatePerformanceValue(StrategyBase strategy)
        {
            Value = strategy.SystemPerformance.Summary.GetValue(AnalyticalFeature.MaxDrawDown);

        }


    }
}