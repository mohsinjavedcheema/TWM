using System.Windows.Controls;

namespace Twm.Core.DataProviders.Binance.UI
{
    /// <summary>
    /// Логика взаимодействия для BinanceInstrumentsView.xaml
    /// </summary>
    public partial class BinanceInstrumentsView : UserControl
    {
        private BinanceInstrumentManager _instrumentManager;

        public BinanceInstrumentsView()
        {
            InitializeComponent();
            DataContextChanged += BinanceInstrumentsView_DataContextChanged;
        }

        private void BinanceInstrumentsView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is BinanceInstrumentManager dxFeedInstrumentManager)
            {
                _instrumentManager = dxFeedInstrumentManager;
            }
        }
    }
}
