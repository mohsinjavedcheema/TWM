#region Using declarations

using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Optimization;
using Twm.Core.Enums;

#endregion

namespace Twm.Custom.OptimizationFitnesses
{
    public class MinDrawDownDays : OptimizationFitness
    {
        private const string OptimizationFitnessName = "Min Draw Down Days";

        public override void OnCalculatePerformanceValue(StrategyBase strategy)
        {
            var val = strategy.SystemPerformance.Summary.GetValue(AnalyticalFeature.MaxDrawDownDays) * -1;
            Value = val;

        }


    }
}