using System.Windows;
using Twm.ViewModels.Instruments;

namespace Twm.Windows.Tools
{
    /// <summary>
    /// Логика взаимодействия для InstrumentsWindow.xaml
    /// </summary>
    public partial class InstrumentsWindow : Window
    {
        private readonly InstrumentsViewModel _instrumentsViewModel;

        public InstrumentsWindow(InstrumentsViewModel instrumentsViewModel)
        {
            _instrumentsViewModel = instrumentsViewModel;
            InitializeComponent();
            DataContext = _instrumentsViewModel;
            Loaded += InstrumentsWindow_Loaded;
            Closed += InstrumentsWindow_Closed;
        }

        private void InstrumentsWindow_Closed(object sender, System.EventArgs e)
        {
            _instrumentsViewModel.CancelFetchDataCommand.Execute(null);
        }

        private void InstrumentsWindow_Loaded(object sender, RoutedEventArgs e)
        {
           // _instrumentsViewModel.FetchData();
        }

        private void ButtonOKClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
