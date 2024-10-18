using System.Windows;
using Twm.Core.Enums;
using Twm.Core.ViewModels.Presets;

namespace Twm.Core.UI.Windows.Presets
{
    /// <summary>
    /// Логика взаимодействия для PresetsWindow.xaml
    /// </summary>
    public partial class PresetsWindow : Window
    {
        private PresetsViewModel _presetsViewModel;

        public PresetsWindow(PresetsViewModel presetsViewModel)
        {
            _presetsViewModel = presetsViewModel;
            DataContext = presetsViewModel;
            InitializeComponent();
        }


        private void BtnLoadOnClick(object sender, RoutedEventArgs e)
        {
            if (_presetsViewModel.PresetFormMode == PresetFormMode.Load)
                DialogResult = true;
        }

        private void BtnSaveOnClick(object sender, RoutedEventArgs e)
        {
            if (_presetsViewModel.PresetFormMode == PresetFormMode.Save && _presetsViewModel.CheckName())
                DialogResult = true;
        }


        private void BtnExportOnClick(object sender, RoutedEventArgs e)
        {
            if (_presetsViewModel.PresetFormMode == PresetFormMode.Export)
                DialogResult = true;
        }
    }
}
