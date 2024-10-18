using System.Collections.ObjectModel;
using Twm.Core.DataCalc;
using Twm.Core.ViewModels.DataSeries;
using Twm.ViewModels.Charts;

namespace Twm.ViewModels.Strategies.Optimizer
{
    public interface IOptimizerItem
    {
        ObservableCollection<StrategyBase> Strategies { get; set; }

        StrategyViewModel SelectedStrategy { get; set; }

        StrategyBase Strategy { get; set; }

        /*StrategyPerformanceViewModel Performance { get; set; }*//**/
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }

        bool? CheckStrategyBrowsableProperty(string propertyName);

    }
}