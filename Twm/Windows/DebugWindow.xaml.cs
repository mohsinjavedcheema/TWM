using System.Windows;
using Twm.Core.Controllers;

namespace Twm.Windows
{
    /// <summary>
    /// Логика взаимодействия для DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            DataContext = DebugController.Instance;
            InitializeComponent();
        }
    }
}
