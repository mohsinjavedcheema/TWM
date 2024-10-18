using System.Windows;
using Twm.Core.DataCalc.Optimization;

namespace Twm.Core.CustomProperties.Editors
{
    /// <summary>
    /// Логика взаимодействия для OptimizerParametersWindow.xaml
    /// </summary>
    public partial class OptimizerParametersWindow : Window
    {
        public OptimizerParametersWindow(Optimizer optimizer)
        {
            DataContext = optimizer;
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

       
    }
}
