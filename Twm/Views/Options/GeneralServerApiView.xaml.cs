using System.Windows;
using System.Windows.Controls;
using Twm.Core.Controllers;

namespace Twm.Views.Options
{
    /// <summary>
    /// Логика взаимодействия для GeneralServerApiView.xaml
    /// </summary>
    public partial class GeneralServerApiView : UserControl
    {
        public GeneralServerApiView()
        {
            InitializeComponent();
            DataContextChanged += GeneralServerApiView_DataContextChanged;
        }

        private void GeneralServerApiView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is SystemOptions systemOptions)
            {
                 pbPassword.Password = systemOptions.ApiPassword;
            }
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SystemOptions systemOptions)
            {
                systemOptions.ApiPassword = pbPassword.Password;
            }
        }
    }
}
