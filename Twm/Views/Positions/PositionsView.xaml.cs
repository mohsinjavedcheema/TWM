using System.Windows.Controls;

namespace Twm.Views.Positions
{
    /// <summary>
    /// Логика взаимодействия для PositionsView.xaml
    /// </summary>
    public partial class PositionsView : UserControl
    {
        public PositionsView()
        {
            DataContext = App.Positions;
            InitializeComponent();
        }
    }
}
