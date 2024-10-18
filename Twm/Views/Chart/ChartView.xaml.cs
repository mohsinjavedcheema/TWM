using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Twm.Chart;
using Twm.Core.Helpers;
using Twm.ViewModels.Charts;
using Twm.ViewModels.Strategies;

namespace Twm.Views.Chart
{
    /// <summary>
    /// Логика взаимодействия для ChartControl.xaml
    /// </summary>
    public partial class ChartView : UserControl
    {

        private ChartViewModel _chartViewModel;

        public ChartView()
        {
            InitializeComponent();
            DataContextChanged += ChartView_DataContextChanged;
            Loaded += ChartView_Loaded;
        }

        private void ChartView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_chartViewModel.IsPerformanceChart)
            {
                _chartViewModel.Chart.ReCalc_VisibleCandlesRange();
                _chartViewModel.Chart.ReCalc_VisibleCandlesExtremums();
                _chartViewModel.Chart.ChartControl.Invalidate();
            }
        }

        private void ChartView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ChartViewModel oldChartViewModel)
            {
                if (oldChartViewModel.Chart != null)
                    oldChartViewModel.Chart.NotRender = true;
            }

            if (e.NewValue is ChartViewModel chartViewModel)
            {
                _chartViewModel = chartViewModel;
                _chartViewModel.Chart.NotRender = true;
            }
        }


        private readonly List<object> _dynamicallyMenuItems = new List<object>();

        private void ChartMenu_OnOpened(object sender, RoutedEventArgs e)
        {
            var menu = chartControl.ContextMenu;
            if (menu != null)
            {
                //Clear last dynamically added items
                foreach (var dynamicallyMenuItem in _dynamicallyMenuItems)
                {
                    menu.Items.Remove(dynamicallyMenuItem);
                }
                _dynamicallyMenuItems.Clear();

                if (_chartViewModel.IsChartTraderVisible && _chartViewModel.DataCalcContext.LastCandle != null)
                {

                  
                    //Add new items
                    var separator = new Separator();
                    _dynamicallyMenuItems.Add(separator);
                    chartControl.ContextMenu?.Items.Insert(3, separator);

                    

                   var position = Mouse.GetPosition(chartControl);
                    var price = _chartViewModel.Chart.GetPriceFromPoint(position, chartControl.MainPane);
                    if (price != null)
                    {
                        var val = price.Value.RoundToTickSize(_chartViewModel.Chart.TickSize);
                        var formatValue = val.ToString("0." + "".PadRight(_chartViewModel.Chart.TickSize.GetDecimalCount(), '0'));

                        if (val > _chartViewModel.DataCalcContext.LastCandle.C)
                        {
                            var menuItem = new MenuItem {Header = $"Buy Stop Market {formatValue}", CommandParameter = val };
                            var binding = new Binding
                            {
                                Source = _chartViewModel.ChartTrader, Path = new PropertyPath("BuyStopMarketCommand")
                            };
                            menuItem.SetBinding(MenuItem.CommandProperty, binding);
                            _dynamicallyMenuItems.Add(menuItem);
                            chartControl.ContextMenu?.Items.Insert(4, menuItem);

                            menuItem = new MenuItem {Header = $"Sell Limit {formatValue}", CommandParameter = val };
                            binding = new Binding
                            {
                                Source = _chartViewModel.ChartTrader,
                                Path = new PropertyPath("SellLimitCommand")
                            };
                            menuItem.SetBinding(MenuItem.CommandProperty, binding);
                            _dynamicallyMenuItems.Add(menuItem);
                            chartControl.ContextMenu?.Items.Insert(5, menuItem);

                            menuItem = new MenuItem { Header = $"Buy Stop Limit {formatValue}", CommandParameter = val };
                            binding = new Binding
                            {
                                Source = _chartViewModel.ChartTrader,
                                Path = new PropertyPath("BuyStopLimitCommand")
                            };
                            menuItem.SetBinding(MenuItem.CommandProperty, binding);
                            _dynamicallyMenuItems.Add(menuItem);
                            chartControl.ContextMenu?.Items.Insert(6, menuItem);

                        }
                        else
                        {

                            var menuItem = new MenuItem { Header = $"Sell Stop Market {formatValue}", CommandParameter = val };
                            var binding = new Binding
                            {
                                Source = _chartViewModel.ChartTrader,
                                Path = new PropertyPath("SellStopMarketCommand")
                            };
                            menuItem.SetBinding(MenuItem.CommandProperty, binding);
                            _dynamicallyMenuItems.Add(menuItem);
                            chartControl.ContextMenu?.Items.Insert(4, menuItem);

                            menuItem = new MenuItem { Header = $"Buy Limit {formatValue}", CommandParameter = val };
                            binding = new Binding
                            {
                                Source = _chartViewModel.ChartTrader,
                                Path = new PropertyPath("BuyLimitCommand")
                            };
                            menuItem.SetBinding(MenuItem.CommandProperty, binding);
                            _dynamicallyMenuItems.Add(menuItem);
                            chartControl.ContextMenu?.Items.Insert(5, menuItem);

                            menuItem = new MenuItem { Header = $"Sell Stop Limit {formatValue}", CommandParameter = val };
                            binding = new Binding
                            {
                                Source = _chartViewModel.ChartTrader,
                                Path = new PropertyPath("SellStopLimitCommand")
                            };
                            menuItem.SetBinding(MenuItem.CommandProperty, binding);
                            _dynamicallyMenuItems.Add(menuItem);
                            chartControl.ContextMenu?.Items.Insert(6, menuItem);

                        }
                        
                    }
                }
            }
        }
    }
}
