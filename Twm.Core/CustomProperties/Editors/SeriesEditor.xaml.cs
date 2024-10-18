using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Twm.Chart.Interfaces;
using Twm.Core.CustomProperties.ViewModels;
using Twm.Core.DataCalc;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Twm.Core.CustomProperties.Editors
{
    /// <summary>
    /// Логика взаимодействия для SeriesEditor.xaml
    /// </summary>
    public partial class SeriesEditor : UserControl, ITypeEditor
    {
        public SeriesEditor()
        {
            InitializeComponent();
        }

       private DataCalcContext _dataCalcContext;

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(object), typeof(SeriesEditor), new PropertyMetadata(null));

        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        private IndicatorBase indicator;

        private SelectSeriesViewModel _selectSeriesViewModel;

       public FrameworkElement ResolveEditor(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem)
       {
           if (propertyItem.Instance is IndicatorBase indicatorBase)
           {
               indicator = indicatorBase;
               _dataCalcContext = indicator.GetDataCalcContext();
               _selectSeriesViewModel = new SelectSeriesViewModel(_dataCalcContext);
               textBox.Text = indicator.Input.ToString();
           }
           else if (propertyItem.Instance is StrategyBase strategyBase)
           {
               _dataCalcContext = strategyBase.GetDataCalcContext();
               textBox.Text = _dataCalcContext.Name;
               button.IsEnabled = false;
           }

           return this;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            _selectSeriesViewModel.Init(indicator.Input);
            _selectSeriesViewModel.Select(indicator.Input);
            var selectSeriesWindow = new SelectSeriesWindow(_selectSeriesViewModel);
            if (selectSeriesWindow.ShowDialog() == true)
            {
                indicator.SetInput(_selectSeriesViewModel.SelectedSeries as ISeries<double>);
                textBox.Text = indicator.Input.ToString();
            }
        }
    }
}
