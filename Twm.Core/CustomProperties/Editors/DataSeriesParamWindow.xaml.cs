using System.Windows;
using Twm.Core.Classes;
using Twm.Core.ViewModels.DataSeries;

namespace Twm.Core.CustomProperties.Editors
{
    /// <summary>
    /// Логика взаимодействия для InstrumentParamWindow.xaml
    /// </summary>
    public partial class DataSeriesParamWindow : Window
    {
        private DataSeriesParamsViewModel _dataSeriesParamsViewModel;

        public DataSeriesParamWindow(DataSeriesParamsViewModel dataSeriesParamsViewModel)
        {
            _dataSeriesParamsViewModel = dataSeriesParamsViewModel;
            InitializeComponent();
            DataContext = _dataSeriesParamsViewModel;            
        }



        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
