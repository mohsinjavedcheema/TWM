using System.Collections.Generic;
using Twm.Core.ViewModels;

namespace Twm.Core.DataCalc.Performance
{
    public class PerformanceOptions
    {
        
        public bool IsPortfolio { get; set; }

        public bool IsOptimization { get; set; }

        public List<object> ExcludeSections { get; set; }

        public ViewModelBase ParentViewModel { get; set; }

        public bool CreateAnalysis { get; set; }

        public PerformanceOptions()
        {
            IsPortfolio = false;
            IsOptimization = false;
            ExcludeSections = new List<object>();
            CreateAnalysis = true;
        }

    }
}