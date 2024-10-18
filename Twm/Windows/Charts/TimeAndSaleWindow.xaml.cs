using System;
using System.Windows;
using Twm.ViewModels.Charts;

namespace Twm.Windows.Charts
{
    /// <summary>
    /// Interaction logic for DataBoxWindow.xaml
    /// </summary>
    public partial class TimeAndSaleWindow : Window
    {
        public TimeAndSaleWindow() => InitializeComponent();

        public TimeAndSaleWindow(TimeAndSaleViewModel timeAndSaleViewModel) : this()
        {
            DataContext = timeAndSaleViewModel;
        }
            

        public bool IsClosed { get; private set; }

        protected override void OnClosed(EventArgs e)
        {
            ((TimeAndSaleViewModel)DataContext)?.Dispose();
            base.OnClosed(e);
            IsClosed = true;
        }
    }
}
