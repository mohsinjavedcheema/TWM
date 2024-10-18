using System.Windows;
using Twm.Core.ViewModels;
using Twm.Core.ViewModels.Instruments;
using Twm.ViewModels.Instruments;

namespace Twm.Windows.Tools
{
    /// <summary>
    /// Логика взаимодействия для InstrumentListWindow.xaml
    /// </summary>
    public partial class InstrumentListWindow : Window
    {
        public InstrumentListWindow(InstrumentListViewModel instrumentListViewModel)
        {
            DataContext = instrumentListViewModel;
            InitializeComponent();
        }

        private void ButtonOKClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
