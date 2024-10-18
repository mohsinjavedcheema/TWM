using System.Windows;
using Twm.ViewModels.Strategies.Optimizer;
using Twm.ViewModels.Strategies.Validator;

namespace Twm.Windows.Strategies
{
    /// <summary>
    /// Логика взаимодействия для OptimizerPeriodWindow.xaml
    /// </summary>
    public partial class OptimizerPeriodWindow : Window
    {
        private OptimizerPeriodViewModel _optimizerPeriodViewModel;

        public OptimizerPeriodWindow(OptimizerPeriodViewModel optimizerPeriodViewModel)
        {
            _optimizerPeriodViewModel = optimizerPeriodViewModel;
            InitializeComponent();
            DataContext = optimizerPeriodViewModel;
        }



        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
