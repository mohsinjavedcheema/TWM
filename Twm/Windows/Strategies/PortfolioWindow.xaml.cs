using System.Windows;
using Twm.ViewModels.Strategies.Validator;

namespace Twm.Windows.Strategies
{
    /// <summary>
    /// Логика взаимодействия для PortfolioWindow.xaml
    /// </summary>
    public partial class PortfolioWindow : Window
    {
        public PortfolioWindow(ValidatorItemViewModel validatorItemViewModel)
        {
            InitializeComponent();
            DataContext = validatorItemViewModel;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
