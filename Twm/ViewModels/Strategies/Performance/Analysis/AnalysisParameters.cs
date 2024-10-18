using System.Collections.Generic;
using Twm.Core.DataCalc;

namespace Twm.ViewModels.Strategies.Performance.Analysis
{
    public class AnalysisParameters
    {
        public List<StrategyBase> Strategies { get; set; }

        public bool IsPortfolio { get; set; }

        public double StartingCapital { get; set; }
    }
}