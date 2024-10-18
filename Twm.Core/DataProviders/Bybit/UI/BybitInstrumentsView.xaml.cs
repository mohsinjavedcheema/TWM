using System.Windows.Controls;

namespace Twm.Core.DataProviders.Bybit.UI
{
    /// <summary>
    /// Логика взаимодействия для BybitInstrumentsView.xaml
    /// </summary>
    public partial class BybitInstrumentsView : UserControl
    {
        private BybitInstrumentManager _instrumentManager;

        public BybitInstrumentsView()
        {
            InitializeComponent();
            DataContextChanged += BybitInstrumentsView_DataContextChanged;
        }

        private void BybitInstrumentsView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is BybitInstrumentManager dxFeedInstrumentManager)
            {
                _instrumentManager = dxFeedInstrumentManager;
            }
        }
    }
}
