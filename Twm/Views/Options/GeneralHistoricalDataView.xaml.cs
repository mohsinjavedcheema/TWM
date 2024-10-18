using System.Windows;
using System.Windows.Controls;
using Twm.Core.Classes;
using Twm.Core.Controllers;


namespace Twm.Views.Options
{
    /// <summary>
    /// Interaction logic for GeneralHistoricalDataView.xaml
    /// </summary>
    public partial class GeneralHistoricalDataView : UserControl
    {

        private SystemOptions _systemOptions;

        public GeneralHistoricalDataView()
        {
            InitializeComponent();
            DataContextChanged += GeneralCacheView_DataContextChanged;
        }

        private void GeneralCacheView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is SystemOptions systemOptions)
            {
                _systemOptions = systemOptions;
            }
        }

        private void RemoveOnClick(object sender, RoutedEventArgs e)
        {
            if (treeView.SelectedItem is HistoricalData cacheData)
            {
                cacheData.RemoveDataCommand.Execute(_systemOptions);
            }
        }

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
           
        }

        private void AddOnClick(object sender, RoutedEventArgs e)
        {
           if (treeView.SelectedItem is HistoricalData cacheData)
           {
                cacheData.AddDataCommand.Execute(_systemOptions);
           }
        }
    }
}
