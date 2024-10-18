using System.Windows;
using Twm.ViewModels.DataBase;

namespace Twm.Windows.Database
{
    /// <summary>
    /// Логика взаимодействия для DatabaseWindow.xaml
    /// </summary>
    public partial class DatabaseWindow : Window
    {
        public DatabaseWindow()
        {
            DataContext = new DatabaseViewModel();
            InitializeComponent();
        }
    }
}
