using System.Windows;
using System.Windows.Input;
using Twm.ViewModels.ScriptObjects;

namespace Twm.Windows.Strategies
{

    

    /// <summary>
    /// Логика взаимодействия для StrategySelectWindow.xaml
    /// </summary>
    public partial class StrategySelectWindow : Window
    {

        private SelectScriptObjectViewModel _selectScriptObjectViewModel;

        public StrategySelectWindow(SelectScriptObjectViewModel selectScriptObjectViewModel)
        {
            _selectScriptObjectViewModel = selectScriptObjectViewModel;
            InitializeComponent();
            DataContext = _selectScriptObjectViewModel;
        }

        private void AvailableObjectsView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectScriptObjectViewModel.SelectedObjectType = e.NewValue;
        }

        private void Bd_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                DialogResult = true;
            }
        }
    }
}
