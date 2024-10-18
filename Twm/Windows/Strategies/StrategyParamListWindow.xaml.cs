using System.Windows;
using Twm.Core.Controllers;
using Twm.ViewModels.Strategies;
using Twm.ViewModels.Strategies.Optimizer;

namespace Twm.Windows.Strategies
{
    /// <summary>
    /// Логика взаимодействия для StrategyParamListWindow.xaml
    /// </summary>
    public partial class StrategyParamListWindow : Window
    {
        public StrategyParamListWindow(StrategyViewModel strategyViewModel)
        {

            DataContext = strategyViewModel;
            InitializeComponent();
        }
    }
}
