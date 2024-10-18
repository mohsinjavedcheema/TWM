using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Twm.Chart.Interfaces;
using Twm.Core.CustomProperties.ViewModels;
using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Optimization;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Twm.Core.CustomProperties.Editors
{
    /// <summary>
    /// Логика взаимодействия для SeriesEditor.xaml
    /// </summary>
    public partial class OptimizerParametersEditor : UserControl, ITypeEditor
    {
        public OptimizerParametersEditor()
        {
            InitializeComponent();
        }

        private DataCalcContext _dataCalcContext;

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(object), typeof(OptimizerParametersEditor), new PropertyMetadata(null));

        public object Value
        {
            get { return (object) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        


        private StrategyBase _strategyBase;
       // private Optimizer _optimizer;

        public FrameworkElement ResolveEditor(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem)
        {
            propertyItem.IsReadOnly = false;


            if (propertyItem.Instance is StrategyBase strategyBase)
            {

                //TypeDescriptor.AddAttributes(strategyBase.Optimizer, new Attribute[] { new ReadOnlyAttribute(false) });

                _strategyBase = strategyBase;
                _dataCalcContext = strategyBase.GetDataCalcContext();
                textBox.Text = $"{strategyBase.TwmPropertyCount} parameters";
          //    _optimizer = strategyBase.Optimizer;
            }

            

            return this;
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            
            var optimizerParametersWindow = new OptimizerParametersWindow(_strategyBase.Optimizer);
            if (optimizerParametersWindow.ShowDialog() == true)
            {
                _strategyBase.NotifyOptimizationParamsChanged();
            }
        }
    }
}