#region Using declarations

using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Optimization;
using Twm.Core.Enums;

#endregion

namespace Twm.Custom.OptimizationFitnesses
{
    public class PTWinRate : OptimizationFitness
    {
        private const string OptimizationFitnessName = "Winrate";

        public override void OnCalculatePerformanceValue(StrategyBase strategy)
        {
            var pt = strategy.SystemPerformance.Summary.GetValue(AnalyticalFeature.ProfitFactor);
            var wr = strategy.SystemPerformance.Summary.GetValue(AnalyticalFeature.TradesInProfit);
            var np = strategy.SystemPerformance.Summary.GetValue(AnalyticalFeature.NetProfitSum);

            var coef = pt * wr;

            if (np <= 0)
                coef = 0;

            Value = coef;

        }

        
    }
}