using System.Windows;
using Twm.ViewModels.Instruments;

namespace Twm.Windows.Tools
{
    /// <summary>
    /// Логика взаимодействия для InstrumentsMapWindow.xaml
    /// </summary>
    public partial class InstrumentsMapWindow : Window
    {
        private readonly InstrumentsMapViewModel _instrumentsMapViewModel;

        public InstrumentsMapWindow(InstrumentsMapViewModel instrumentsMapViewModel)
        {
            _instrumentsMapViewModel = instrumentsMapViewModel;
            InitializeComponent();
            DataContext = instrumentsMapViewModel;
        
            Closed += InstrumentsWindow_Closed;
        }

        private void InstrumentsWindow_Closed(object sender, System.EventArgs e)
        {
            _instrumentsMapViewModel.CancelFetchDataCommand.Execute(null);
        }

        

        private void ButtonOKClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
