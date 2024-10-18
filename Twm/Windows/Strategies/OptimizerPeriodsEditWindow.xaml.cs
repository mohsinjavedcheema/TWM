using System.Windows;
using Twm.ViewModels.Strategies.Optimizer;

namespace Twm.Windows.Strategies
{
    /// <summary>
    /// Логика взаимодействия для OptimizerPeriodsEditWindow.xaml
    /// </summary>
    public partial class OptimizerPeriodsEditWindow : Window
    {
        private OptimizerPeriodsEditViewModel _optimizerPeriodsEditViewModel;





        public OptimizerPeriodsEditWindow(OptimizerPeriodsEditViewModel optimizerPeriodsEditViewModel)
        {
            _optimizerPeriodsEditViewModel = optimizerPeriodsEditViewModel;
            InitializeComponent();
            DataContext = _optimizerPeriodsEditViewModel;

            optimizerPeriodsEditViewModel.UpdateWindowHeight();
            
        }



        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
