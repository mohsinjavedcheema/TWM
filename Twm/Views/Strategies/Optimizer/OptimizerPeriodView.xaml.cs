using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Twm.Views.Strategies.Optimizer
{
    /// <summary>
    /// Логика взаимодействия для OptimizerPeriodView.xaml
    /// </summary>
    public partial class OptimizerPeriodView : UserControl
    {

        private const int GridHeight = 320;

        public OptimizerPeriodView()
        {
            InitializeComponent();
            topHelperGrid.Height = Double.NaN;
            topButtonGrid.Height = GridHeight;
        }

        private void ExpanderTop_OnExpanded(object sender, RoutedEventArgs e)
        {

            if (Equals(e.Source, grid) || topSplitter == null)
                return;


            topHelperGrid.Height = Double.NaN;

            DoubleAnimation expanderTopAnimation = new
                DoubleAnimation
                {
                    From = 30,
                    To = GridHeight,
                    Duration = new Duration(
                        new TimeSpan(0, 0, 0, 0, GridHeight))
                };
            topButtonGrid.BeginAnimation(Expander.HeightProperty, expanderTopAnimation);
            topSplitter.IsEnabled = true;
        }

        private void ExpanderTop_OnCollapsed(object sender, RoutedEventArgs e)
        {
            if (Equals(e.Source, grid) || topSplitter == null)
                return;

            topHelperGrid.Height = Double.NaN;

            DoubleAnimation expanderTopAnimation = new
                DoubleAnimation
                {
                    From = topButtonGrid.ActualHeight,
                    To = 30,
                    Duration = new Duration(
                        new TimeSpan(0, 0, 0, 0, GridHeight))
                };
            topButtonGrid.BeginAnimation(Expander.HeightProperty, expanderTopAnimation);

            topHelperGrid.Height = 30;
            topSplitter.IsEnabled = false;
        }
    }
}
