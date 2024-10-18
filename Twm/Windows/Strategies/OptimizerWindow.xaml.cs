using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Twm.Core.ViewModels;
using Twm.ViewModels.Strategies.Optimizer;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Twm.Windows.Strategies
{
    /// <summary>
    /// Логика взаимодействия для OptimizerWindow.xaml
    /// </summary>
    public partial class OptimizerWindow : Window
    {
        private readonly OptimizerViewModel _optimizerViewModel;

        public OptimizerWindow(OptimizerViewModel optimizerViewModel)
        {
            _optimizerViewModel = optimizerViewModel;
            InitializeComponent();
            DataContext = optimizerViewModel;
            propertyGrid.IsPropertyBrowsable += PropertyGrid_IsPropertyBrowsable;
            leftHelperGrid.Width = Double.NaN;
            leftButtonGrid.Width = 300;
            rightHelperGrid.Width = Double.NaN;
            rightButtonGrid.Width = 300;
            Closed += OptimizerWindow_Closed;
        }

        private void OptimizerWindow_Closed(object sender, EventArgs e)
        {
            propertyGrid.IsPropertyBrowsable -= PropertyGrid_IsPropertyBrowsable;
            _optimizerViewModel.Destroy();
        }

        private void PropertyGrid_IsPropertyBrowsable(object sender, IsPropertyBrowsableArgs e)
        {
            if (_optimizerViewModel.SelectedOptimizerItem is IOptimizerItem optimizerItem)
            {
                var result = optimizerItem.CheckStrategyBrowsableProperty(e.PropertyDescriptor.Name);

                if (result != null)
                {
                    e.IsBrowsable = result;
                }
            }
        }

        private void trOptimizerTests_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is ViewModelBase viewModelBase)
            {
                _optimizerViewModel.SelectedOptimizerItem = viewModelBase;
            }
        }


        private void ExpanderRight_OnExpanded(object sender, RoutedEventArgs e)
        {
            if (Equals(e.Source, propertyGrid) || propertyGrid == null)
                return;

      
            rightHelperGrid.Width = Double.NaN;

            DoubleAnimation expanderRightAnimation = new
                DoubleAnimation();
            expanderRightAnimation.From = 30;
            expanderRightAnimation.To = 300;
            expanderRightAnimation.Duration =
                new Duration(
                    new TimeSpan(0, 0, 0, 0, 300));
            rightButtonGrid.BeginAnimation(Expander.WidthProperty, expanderRightAnimation);
            rightSplitter.IsEnabled = true;
        }

        private void ExpanderRight_OnCollapsed(object sender, RoutedEventArgs e)
        {
            if (Equals(e.Source, propertyGrid) || propertyGrid == null)
                return;

            rightHelperGrid.Width = Double.NaN;

            DoubleAnimation expanderRightAnimation = new
                DoubleAnimation
                {
                    From = rightButtonGrid.ActualWidth,
                    To = 30,
                    Duration = new Duration(
                        new TimeSpan(0, 0, 0, 0, 300))
                };
            rightButtonGrid.BeginAnimation(Expander.WidthProperty, expanderRightAnimation);

            rightHelperGrid.Width = 30;
            rightSplitter.IsEnabled = false;
        }

   

        private void ExpanderLeft_OnExpanded(object sender, RoutedEventArgs e)
        {
            if (Equals(e.Source, propertyGrid) || propertyGrid == null)
                return;

            leftHelperGrid.Width = Double.NaN;

            DoubleAnimation expanderRightAnimation = new
                DoubleAnimation();
            expanderRightAnimation.From = 30;
            expanderRightAnimation.To = 300;
            expanderRightAnimation.Duration =
                new Duration(
                    new TimeSpan(0, 0, 0, 0, 300));
            leftButtonGrid.BeginAnimation(Expander.WidthProperty, expanderRightAnimation);
            leftSplitter.IsEnabled = true;
        }

        private void ExpanderLeft_OnCollapsed(object sender, RoutedEventArgs e)
        {
            if (Equals(e.Source, propertyGrid) || propertyGrid == null)
                return;

            leftHelperGrid.Width = Double.NaN;

            DoubleAnimation expanderRightAnimation = new
                DoubleAnimation
                {
                    From = leftButtonGrid.ActualWidth,
                    To = 30,
                    Duration = new Duration(
                        new TimeSpan(0, 0, 0, 0, 300))
                };
            leftButtonGrid.BeginAnimation(Expander.WidthProperty, expanderRightAnimation);

            leftHelperGrid.Width = 30;
            leftSplitter.IsEnabled = false;
        }

        private void LoadOnClick(object sender, RoutedEventArgs e)
        {
            _optimizerViewModel.OptimizerStrategyPresetLoadCommand.Execute(null);
        }

        private void SaveOnClick(object sender, RoutedEventArgs e)
        {
            _optimizerViewModel.OptimizerStrategyPresetSaveCommand.Execute(null);
        }

        private void ExportOnClick(object sender, RoutedEventArgs e)
        {
            _optimizerViewModel.OptimizerStrategyPresetExportCommand.Execute(null);
        }
    }
}