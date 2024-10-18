using System.Windows;
using Twm.Core.ViewModels.DataSeries;
using Twm.ViewModels.Charts;

namespace Twm.Windows.Charts
{
    /// <summary>
    /// Логика взаимодействия для ChartParamsWindow.xaml
    /// </summary>
    public partial class ChartParamsWindow : Window
    {
        public ChartParamsWindow(ChartParamsViewModel chartParamsViewModel)
        {
            DataContext = chartParamsViewModel;
            InitializeComponent();
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
