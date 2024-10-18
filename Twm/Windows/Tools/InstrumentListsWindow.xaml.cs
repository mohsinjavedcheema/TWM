using System.Windows;
using Twm.ViewModels.Instruments;

namespace Twm.Windows.Tools
{
    /// <summary>
    /// Логика взаимодействия для InstrumentListsWindow.xaml
    /// </summary>
    public partial class InstrumentListsWindow : Window
    {
        private readonly InstrumentListsViewModel _instrumentListsViewModel;

        public InstrumentListsWindow(InstrumentListsViewModel instrumentListsViewModel)
        {
            _instrumentListsViewModel = instrumentListsViewModel;
            InitializeComponent();
            DataContext = _instrumentListsViewModel;
            Closed += InstrumentsWindow_Closed;
        }

        private void InstrumentsWindow_Closed(object sender, System.EventArgs e)
        {
            //
        }

        
        private void ButtonOKClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
