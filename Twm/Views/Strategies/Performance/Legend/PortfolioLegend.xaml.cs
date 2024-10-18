using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;

namespace AlgoDesk.Views.Strategies.Performance.Legend
{
    public partial class PortfolioLegend : UserControl, IChartLegend
    {
        private List<SeriesViewModel> _series;

        public PortfolioLegend()
        {
            InitializeComponent();

            DataContext = this;
        }

        public List<SeriesViewModel> Series
        {
            get { return _series; }
            set
            {
                _series = value;
                OnPropertyChanged("Series");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
