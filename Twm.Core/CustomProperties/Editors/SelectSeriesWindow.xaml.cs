using System.Windows;
using Twm.Core.CustomProperties.ViewModels;

namespace Twm.Core.CustomProperties.Editors
{
    /// <summary>
    /// Логика взаимодействия для SelectIndicatorWindow.xaml
    /// </summary>
    public partial class SelectSeriesWindow : Window
    {
        private readonly SelectSeriesViewModel _selectSeriesViewModel;

        public SelectSeriesWindow(SelectSeriesViewModel selectSeriesViewModel)
        {
            _selectSeriesViewModel = selectSeriesViewModel;
            DataContext = _selectSeriesViewModel;
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
