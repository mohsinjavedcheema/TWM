using System;
using System.Windows;
using Twm.ViewModels.Charts;

namespace Twm.Windows.Charts
{
    /// <summary>
    /// Interaction logic for DataBoxWindow.xaml
    /// </summary>
    public partial class DataBoxWindow : Window
    {
        public DataBoxWindow() => InitializeComponent();

        public DataBoxWindow(ChartViewModel chartViewModel) : this() =>
            DataContext = new DataBoxViewModel(chartViewModel);

        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            ((DataBoxViewModel)DataContext)?.Dispose();
            base.OnClosed(e);
            IsClosed = true;
        }
    }
}
