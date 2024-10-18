using System.Windows.Controls;

namespace Twm.Views.Orders
{
    /// <summary>
    /// Логика взаимодействия для OrdersView.xaml
    /// </summary>
    public partial class OrdersView : UserControl
    {
        public OrdersView()
        {
            DataContext = App.Orders;
            InitializeComponent();
        }
    }
}
