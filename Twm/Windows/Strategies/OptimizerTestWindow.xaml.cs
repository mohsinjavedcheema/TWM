using System.Windows;
using Twm.ViewModels.Strategies.Optimizer;

namespace Twm.Windows.Strategies
{
    /// <summary>
    /// Логика взаимодействия для OptimizerTestWindow.xaml
    /// </summary>
    public partial class OptimizerTestWindow : Window
    {
        public OptimizerTestWindow(OptimizerTestViewModel optimizerTestViewModel)
        {
            InitializeComponent();
            DataContext = optimizerTestViewModel;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
