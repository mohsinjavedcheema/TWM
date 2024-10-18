using System.Windows;
using System.Windows.Controls;
using Twm.Core.Controllers;

namespace Twm.Views.Options
{
    /// <summary>
    /// Логика взаимодействия для GeneralEmailView.xaml
    /// </summary>
    public partial class GeneralEmailView : UserControl
    {
        public GeneralEmailView()
        {
            InitializeComponent();
            DataContextChanged += GeneralEmailView_DataContextChanged;
        }

        private void GeneralEmailView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is SystemOptions systemOptions)
            {
                 pbPassword.Password = systemOptions.EmailPassword;
            }
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SystemOptions systemOptions)
            {
                systemOptions.EmailPassword = pbPassword.Password;
            }
        }
    }
}
