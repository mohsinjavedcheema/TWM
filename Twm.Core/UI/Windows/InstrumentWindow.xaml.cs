using System.Windows;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Twm.Core.UI.Windows
{
    /// <summary>
    /// Логика взаимодействия для InstrumentWindow.xaml
    /// </summary>
    public partial class InstrumentWindow : Window
    {
        

        public InstrumentWindow(object instrument)
        {
            DataContext = instrument;
            InitializeComponent();
            
        }

       
        

        private void btnOKClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
