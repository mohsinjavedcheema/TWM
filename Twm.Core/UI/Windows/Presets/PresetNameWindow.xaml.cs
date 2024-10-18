using System.Windows;
using Twm.Core.ViewModels;

namespace Twm.Core.UI.Windows.Presets
{
    /// <summary>
    /// Логика взаимодействия для PresetNameWindow.xaml
    /// </summary>
    public partial class PresetNameWindow : Window
    {
        public PresetNameWindow(ViewModelBase viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
