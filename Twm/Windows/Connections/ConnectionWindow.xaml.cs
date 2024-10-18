using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using Twm.ViewModels.Connections;

namespace Twm.Windows.Connections
{
    /// <summary>
    /// Логика взаимодействия для ConnectionWindow.xaml
    /// </summary>
    public partial class ConnectionWindow : Window
    {
        private readonly ConfigureConnectionViewModel _configureConnectionViewModel;

        public ConnectionWindow(ConfigureConnectionViewModel configureConnectionViewModel)
        {
            _configureConnectionViewModel = configureConnectionViewModel;
            DataContext = _configureConnectionViewModel;
            InitializeComponent();
        }

        private void BtnOK_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
