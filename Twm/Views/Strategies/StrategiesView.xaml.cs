using System.Windows.Controls;

namespace Twm.Views.Strategies
{
    /// <summary>
    /// Логика взаимодействия для StrategiesView.xaml
    /// </summary>
    public partial class StrategiesView : UserControl
    {
        public StrategiesView()
        {
            DataContext = App.Strategies;
            InitializeComponent();
        }
    }
}
