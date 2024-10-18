using System.Collections.Generic;
using Twm.Core.DataCalc.Optimization;
using Twm.Core.Enums;

namespace Twm.Core.Interfaces
{
    public interface IOptimizerStrategyPreset
    {
        string StrategyType { get; set; }

        string Guid { get; set; }

        string Symbol { get; set; }

        int DataSeriesValue { get; set; }

        DataSeriesType DataSeriesType { get; set; }

        string OptimizationFitnessType { get; set; }

        List<OptimizerParameter> Parameters { get; set; }

        Optimizer Optimizer { get; set; }

        string OptimizerType { get; set; }
    }
}