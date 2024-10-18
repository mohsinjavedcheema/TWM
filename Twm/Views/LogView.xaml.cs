using System.Windows.Controls;
using Twm.Core.Controllers;

namespace Twm.Views
{
    /// <summary>
    /// Логика взаимодействия для LogView.xaml
    /// </summary>
    public partial class LogView : UserControl
    {
        public LogView()
        {
            DataContext = LogController.Instance;
            InitializeComponent();
        }
    }
}
