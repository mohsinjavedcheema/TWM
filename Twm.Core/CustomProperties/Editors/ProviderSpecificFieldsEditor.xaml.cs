using System.Windows;
using System.Windows.Controls;
using Twm.Chart.Interfaces;
using Twm.Core.CustomProperties.ViewModels;
using Twm.Core.DataCalc;
using Twm.Model.Model;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Twm.Core.CustomProperties.Editors
{
    /// <summary>
    /// Логика взаимодействия для ProviderSpecificFieldsEditor.xaml
    /// </summary>
    public partial class ProviderSpecificFieldsEditor : UserControl, ITypeEditor
    {
        public ProviderSpecificFieldsEditor()
        {
            InitializeComponent();
        }

       

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(object), typeof(ProviderSpecificFieldsEditor), new PropertyMetadata(null));

        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        private Instrument _instrument;

        private SelectSeriesViewModel _selectSeriesViewModel;

       public FrameworkElement ResolveEditor(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem)
       {
           if (propertyItem.Instance is Instrument instrument)
           {
               _instrument = instrument;
               
              // _selectSeriesViewModel = new SelectSeriesViewModel(_dataCalcContext);



               textBox.Text = "(fields)";
           }
          

           return this;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            /*_selectSeriesViewModel.Init(indicator.Input);
            _selectSeriesViewModel.Select(indicator.Input);
            var selectSeriesWindow = new SelectSeriesWindow(_selectSeriesViewModel);
            if (selectSeriesWindow.ShowDialog() == true)
            {
                indicator.SetInput(_selectSeriesViewModel.SelectedSeries as ISeries<double>);
                textBox.Text = indicator.Input.ToString();
            }*/
        }
    }
}
