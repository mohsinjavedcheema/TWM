using System.Windows.Controls;
using Twm.ViewModels.Charts;
using Twm.ViewModels.Strategies;

namespace Twm.Views.Chart
{
    /// <summary>
    /// Логика взаимодействия для ChartTraderControl.xaml
    /// </summary>
    public partial class ChartTraderView : UserControl
    {

        private ChartTraderViewModel _chartTraderViewModel;

        public ChartTraderView()
        {
            InitializeComponent();
            DataContextChanged += ChartView_DataContextChanged;
           
        }

       

        private void ChartView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
           
            if (e.NewValue is ChartTraderViewModel chartTraderViewModel)
            {
                _chartTraderViewModel = chartTraderViewModel;
              
            }
        }
    }
}
