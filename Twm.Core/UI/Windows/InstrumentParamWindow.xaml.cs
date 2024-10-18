using System.Windows;
using Twm.Core.Classes;
using Twm.Core.ViewModels;

namespace Twm.Core.UI.Windows
{
    /// <summary>
    /// Логика взаимодействия для InstrumentParamWindow.xaml
    /// </summary>
    public partial class InstrumentParamWindow : Window
    {
        

        public InstrumentParamWindow(ViewModelBase viewModel)
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
