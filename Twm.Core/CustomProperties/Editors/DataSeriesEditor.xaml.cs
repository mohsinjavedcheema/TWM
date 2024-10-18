using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Twm.Chart.Interfaces;
using Twm.Core.CustomProperties.ViewModels;
using Twm.Core.DataCalc;
using Twm.Core.ViewModels.DataSeries;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Twm.Core.CustomProperties.Editors
{
    /// <summary>
    /// Логика взаимодействия для DataSeriesEditor.xaml
    /// </summary>
    public partial class DataSeriesEditor : UserControl, ITypeEditor
    {
        public DataSeriesEditor()
        {
            InitializeComponent();
        }

       private DataCalcContext _dataCalcContext;

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(object), typeof(DataSeriesEditor), new PropertyMetadata(null));

        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }




        private StrategyBase _strategy;

       public FrameworkElement ResolveEditor(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem)
       {
           if (propertyItem.Instance is StrategyBase strategyBase)
           {
               _dataCalcContext = strategyBase.GetDataCalcContext();
               _strategy = strategyBase;
               textBox.Text = _dataCalcContext.Name;
           }

           return this;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {

            DataSeriesParamsViewModel dataSeriesParamsViewModel = new DataSeriesParamsViewModel();
            if (_strategy.DataSeriesSeriesParams != null)
            {
                dataSeriesParamsViewModel.Instrument = _strategy.DataSeriesSeriesParams.Instrument;
                
                dataSeriesParamsViewModel.SelectedConnection = dataSeriesParamsViewModel.Connections.FirstOrDefault(x=>x.Id == _strategy.DataSeriesSeriesParams.Instrument.ConnectionId);
                dataSeriesParamsViewModel.SelectedType = _strategy.DataSeriesSeriesParams.Instrument.Type;
                dataSeriesParamsViewModel.DataSeriesFormat = dataSeriesParamsViewModel.DataSeriesFormats.FirstOrDefault(x => x.Value == _strategy.DataSeriesSeriesParams.DataSeriesFormat.Value && x.Type == _strategy.DataSeriesSeriesParams.DataSeriesFormat.Type);
            };

            var dataSeriesParamWindow = new DataSeriesParamWindow(dataSeriesParamsViewModel);
            if (dataSeriesParamWindow.ShowDialog() == true)
            {
                _strategy.DataSeriesSeriesParams = dataSeriesParamsViewModel;
                textBox.Text = _dataCalcContext.Name;
            }
        }
    }
}
