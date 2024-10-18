using System.Windows.Controls;

namespace Twm.Views.Accounts
{
    /// <summary>
    /// Логика взаимодействия для AccountsView.xaml
    /// </summary>
    public partial class AccountsView : UserControl
    {
        public AccountsView()
        {
            DataContext = App.Accounts;
            InitializeComponent();
        }
    }
}
