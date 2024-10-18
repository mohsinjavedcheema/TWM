using System.Reflection;
using System;
using System.Windows.Controls;
using Twm.ViewModels.Charts;
using Xceed.Wpf.Toolkit.PropertyGrid;


namespace Twm.Views.Chart
{
    /// <summary>
    /// Логика взаимодействия для DrawingToolsView.xaml
    /// </summary>
    public partial class DrawingToolsView : UserControl
    {

        private ChartTraderViewModel _chartTraderViewModel;

        public DrawingToolsView()
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
