using System.Windows.Controls;
using Twm.ViewModels.Instruments;

namespace Twm.Views.Instruments
{
    /// <summary>
    /// Логика взаимодействия для InstrumentsView.xaml
    /// </summary>
    public partial class InstrumentsView : UserControl
    {
        private readonly InstrumentsViewModel _instrumentsViewModel;

        public InstrumentsView()
        {
            InitializeComponent();
        }
    }
}
