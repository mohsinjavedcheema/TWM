using System.Windows.Controls;

namespace Twm.Views.Assets
{
    /// <summary>
    /// Логика взаимодействия для AssetsView.xaml
    /// </summary>
    public partial class AssetsView : UserControl
    {
        public AssetsView()
        {
            DataContext = App.Assets;
            InitializeComponent();
        }
    }
}
