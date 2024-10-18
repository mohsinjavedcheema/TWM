using System.ComponentModel;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;

namespace AlgoDesk.Views.Strategies.Performance.ToolTips
{
    /// <summary>
    /// Логика взаимодействия для TradeTooltip.xaml
    /// </summary>
    public partial class PortfolioTradeTooltip : UserControl, IChartTooltip
    {
        private TooltipData _data;
        public PortfolioTradeTooltip()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TooltipData Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged("Data");
            }
        }

        public TooltipSelectionMode? SelectionMode { get; set; }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
