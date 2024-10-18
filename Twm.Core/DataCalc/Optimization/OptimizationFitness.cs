using System;

namespace Twm.Core.DataCalc.Optimization
{
    public abstract class OptimizationFitness : ICloneable
    {
        public  string Name { get; set; }

        public double Value { get; set; }


        public abstract void OnCalculatePerformanceValue(StrategyBase strategy);


        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}