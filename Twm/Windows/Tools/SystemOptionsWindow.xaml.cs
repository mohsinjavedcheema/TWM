using System.Windows;
using Twm.Core.Controllers;
using Twm.ViewModels.Options;

namespace Twm.Windows.Tools
{

    

    /// <summary>
    /// Логика взаимодействия для SystemOptionsWindow.xaml
    /// </summary>
    public partial class SystemOptionsWindow : Window
    {
        private readonly SystemOptionsViewModel _systemOptionsViewModel;

        public SystemOptionsWindow(SystemOptionsViewModel systemOptionsViewModel)
        {
            _systemOptionsViewModel = systemOptionsViewModel;
            DataContext = _systemOptionsViewModel;
            InitializeComponent();
            
        }

        private void TreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {

            if (e.NewValue is CategoryViewModel categoryViewModel)
            {
                _systemOptionsViewModel.SystemOptions = null;
                SystemOptions.Instance.SelectedCategory = categoryViewModel.Name;
                SystemOptions.Instance.SelectedGroup = "";
                
                _systemOptionsViewModel.SystemOptions = SystemOptions.Instance;
            }
            else if(e.NewValue is GroupViewModel groupViewModel)
            {
                _systemOptionsViewModel.SystemOptions = null;
                SystemOptions.Instance.SelectedCategory = "";
                SystemOptions.Instance.SelectedGroup = groupViewModel.Name;
                
                _systemOptionsViewModel.SystemOptions = SystemOptions.Instance;
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

        }
    }
}
