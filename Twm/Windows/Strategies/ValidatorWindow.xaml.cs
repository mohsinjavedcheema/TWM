using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Twm.Core.Enums;
using Twm.Extensions;
using Twm.ViewModels.Strategies.Validator;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Twm.Windows.Strategies
{
    /// <summary>
    /// Логика взаимодействия для ValidatorWindow.xaml
    /// </summary>
    public partial class ValidatorWindow : Window
    {
        private readonly ValidatorViewModel _validatorViewModel;

        public ValidatorWindow(ValidatorViewModel validatorViewModel)
        {
            _validatorViewModel = validatorViewModel;
            InitializeComponent();
            DataContext = validatorViewModel;
            Closed += ValidatorWindow_Closed;
            leftHelperGrid.Width = Double.NaN;
            leftButtonGrid.Width = 300;
            rightHelperGrid.Width = Double.NaN;
            rightButtonGrid.Width = 300;
           }

   

        private void ValidatorWindow_Closed(object sender, EventArgs e)
        {
            _validatorViewModel.Clear();
        }

        private void AvailableObjectsView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is ValidatorItemViewModel validatorItemViewModel)
            {
                if (validatorItemViewModel.IsPortfolio)
                {
                    PropertyGridExt.SetPropsVisibility(propertyGrid, PropertyVisibility.ShortValidator);
                    var st = validatorItemViewModel.Strategy;
                    validatorItemViewModel.ReloadStrategy(st);
                    var be = propertyGrid.GetBindingExpression(PropertyGrid.SelectedObjectProperty);
                    if (be != null)
                    {
                        be.UpdateSource();
                        be.UpdateTarget();
                    }
                }
                else
                {
                    PropertyGridExt.SetPropsVisibility(propertyGrid, PropertyVisibility.Validator);

                    if (validatorItemViewModel.InstrumentList.ApplyToAllInstruments)
                    {
                        var st = validatorItemViewModel.InstrumentList.Strategy;
                        if (st != null)
                        {
                            validatorItemViewModel.InstrumentList.ReloadStrategy(st);
                            st.DataSeriesSeriesParams = validatorItemViewModel.InstrumentSeriesParams;
                        }
                    }

                    validatorItemViewModel.UpdateProperty("Strategy");

                }

                _validatorViewModel.SelectedValidatorItem = validatorItemViewModel;

               
            }
        }


        private void ExpanderRight_OnExpanded(object sender, RoutedEventArgs e)
        {
            if (Equals(e.Source, propertyGrid) || propertyGrid == null)
                return;


            rightHelperGrid.Width = Double.NaN;
            DoubleAnimation expanderRightAnimation = new
                DoubleAnimation
                {
                    From = 30,
                    To = 300,
                    Duration = new Duration(
                        new TimeSpan(0, 0, 0, 0, 300))
                };
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


        private void PropertyGrid_OnPropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
           
           // _validatorViewModel.SelectedValidatorItem?.Invalid();
        }

        private void LoadOnClick(object sender, RoutedEventArgs e)
        {
            _validatorViewModel.ValidatorStrategyPresetsLoadCommand.Execute(null);
        }

        private void SaveOnClick(object sender, RoutedEventArgs e)
        {
            _validatorViewModel.ValidatorStrategyPresetsSaveCommand.Execute(null);
        }

        private void ExportOnClick(object sender, RoutedEventArgs e)
        {
            _validatorViewModel.ValidatorStrategyPresetsLoadCommand.Execute(null);
        }
    }
}