using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Twm.Core.DataCalc;
using Twm.ViewModels.ScriptObjects;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Twm.Windows
{
    /// <summary>
    /// Логика взаимодействия для SelectScriptObjectWindow.xaml
    /// </summary>
    public partial class SelectScriptObjectWindow : Window
    {
        private readonly SelectScriptObjectViewModel _selectScriptObjectViewModel;

        public SelectScriptObjectWindow(SelectScriptObjectViewModel selectScriptObjectViewModel)
        {
            _selectScriptObjectViewModel = selectScriptObjectViewModel;
            DataContext = _selectScriptObjectViewModel;
            _selectScriptObjectViewModel.PropertyChanged += _selectIndicatorViewModel_PropertyChanged;
            InitializeComponent();
            propertyGrid.SelectedObjectChanged += PropertyGrid_SelectedObjectChanged;
            propertyGrid.IsPropertyBrowsable += PropertyGrid_IsPropertyBrowsable;
        }

        private void PropertyGrid_IsPropertyBrowsable(object sender, IsPropertyBrowsableArgs e)
        {

            if (_selectScriptObjectViewModel.SelectedObject is ScriptBase script)
            {
                var result = script.CheckBrowsableProperty(e.PropertyDescriptor.Name);

                if (result != null)
                {
                    e.IsBrowsable = result;
                }
            }
        }

        private void PropertyGrid_SelectedObjectChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_selectScriptObjectViewModel.SelectedObject is StrategyBase strategy)
            {
                CheckStrategyEnable(strategy.Enabled);
            }
        }

        private void _selectIndicatorViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedObject" && _selectScriptObjectViewModel.SelectedObject != null)
            {
                if (availableObjectsView.ItemContainerGenerator.ContainerFromItem(availableObjectsView.SelectedItem) is
                    TreeViewItem tvi)
                {
                    tvi.IsSelected = false;
                }
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }


        private void PropertyGrid_OnPropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            if (e.OriginalSource is PropertyItem pi)
            {
                _selectScriptObjectViewModel.SelectedObject.CheckTwmProperty(pi.PropertyName, e.NewValue);

                if (_selectScriptObjectViewModel.SelectedObject is StrategyBase)
                {
                    if (pi.PropertyName == "Enabled")
                    {
                        CheckStrategyEnable((bool) e.NewValue);

                    }
                }

            }
        }

        private void CheckStrategyEnable(bool value)
        {
            foreach (var property in propertyGrid.Properties.OfType<PropertyItem>())
            {
                if (property.PropertyName == "Enabled")
                    continue;

                property.IsEnabled = !value;
            }
        }

        private void AvailableObjectsView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectScriptObjectViewModel.SelectedObjectType = e.NewValue;
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            var list = sender as ListView;
            list.SelectedIndex = 0;
        }

        private void LoadOnClick(object sender, RoutedEventArgs e)
        {
            _selectScriptObjectViewModel.StrategyPresetsLoadCommand.Execute(null);
        }

        private void SaveOnClick(object sender, RoutedEventArgs e)
        {
            _selectScriptObjectViewModel.StrategyPresetsSaveCommand.Execute(null);
        }

        private void ExportOnClick(object sender, RoutedEventArgs e)
        {
            _selectScriptObjectViewModel.StrategyPresetsExportCommand.Execute(null);
        }
    }
}