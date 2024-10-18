using System.Windows;
using Twm.Core.Controllers;
using Twm.ViewModels.Strategies.Optimizer;

namespace Twm.Windows.Strategies
{
    /// <summary>
    /// Логика взаимодействия для OptimizerErrorWindow.xaml
    /// </summary>
    public partial class OptimizerErrorWindow : Window
    {
        public OptimizerErrorWindow(TaskViewModel taskViewModel)
        {
            DataContext = taskViewModel;
            InitializeComponent();
        }
    }
}
