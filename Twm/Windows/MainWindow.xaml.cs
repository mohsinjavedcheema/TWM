using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Microsoft.EntityFrameworkCore.Internal;

namespace Twm.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       

        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.Navigation;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            Closed += MainWindow_Closed;
            EnsureStandardPopupAlignment();
           
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            var activeStrategies = Session.Instance.Strategies.Where(x => x.Enabled);

            if (activeStrategies.Any())
            {
                var result = MessageBox.Show("One or more strategies are active. Are you sure you want to close the application?", "Confirmation", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No || result == MessageBoxResult.Cancel)
                    e.Cancel = true;
            }
        }

        private void EnsureStandardPopupAlignment()
        {
            var menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
            if (SystemParameters.MenuDropAlignment && menuDropAlignmentField != null)
            {
                menuDropAlignmentField.SetValue(null, false);
            }
        }

        private void MainWindow_Closed(object sender, System.EventArgs e)
        {
           

            foreach (var connection in Session.Instance.Connections)
            {
                connection.Value.Disconnect();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            App.CreateConnectionsMenu(this);
            Session.Instance.Dispatcher = Dispatcher;
            Session.Instance.UiContext = SynchronizationContext.Current;
            
            DebugController.Instance.Init();

        }
    }
}
