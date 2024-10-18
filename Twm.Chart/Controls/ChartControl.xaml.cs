using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Twm.Chart.Controls
{
    /// <summary>
    /// Логика взаимодействия для ChartControl.xaml
    /// </summary>
    public partial class ChartControl : UserControl
    {
        private Classes.Chart _chart;

        public static readonly DependencyProperty PaneCountProperty
            = DependencyProperty.Register("PaneCount", typeof(int), typeof(ChartControl), new UIPropertyMetadata(1));

        public int PaneCount
        {
            get { return (int) GetValue(PaneCountProperty); }
            set { SetValue(PaneCountProperty, value); }
        }

        public bool NotRender { get; set; }


       

        public ChartControl()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
            Unloaded += ChartControl_Unloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            OnSizeChanged(sender, null);
        }

        private void ChartControl_Unloaded(object sender, RoutedEventArgs e)
        {
            DataContextChanged -= OnDataContextChanged;
            Loaded -= OnLoaded;
            Unloaded -= ChartControl_Unloaded;

        }


        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is Classes.Chart chart)
            {
                _chart = chart;
                if (_chart.ChartControl == null)
                {
                    _chart.ChartControl = this;
                    _chart.NotRender = false;
                }
                else
                {
                    _chart.NotRender = true;


                    if (_chart.ChartControl != this)
                    {
                        var oldChartControl = _chart.ChartControl;
                        _chart.ChartControl = this;
                       


                        //Replace Main pane
                        if (_chart.PanePlots.TryGetValue(oldChartControl.MainPane, out var plots))
                        {
                            _chart.PanePlots.Remove(oldChartControl.MainPane);
                            _chart.PanePlots.Add(_chart.ChartControl.MainPane, plots);
                        }
                    }

                    _chart.ChartControl.RemoveAllPane();
                    var otherVirtualPanes = _chart.PanePlots.Keys.Where(x => !x.IsMainPane).ToList();
                    foreach (var virtualPane in otherVirtualPanes)
                    {
                        var pane = AddPane(virtualPane.Name);
                        pane.SetOwner(virtualPane.Owner);
                        if (_chart.PanePlots.TryGetValue(virtualPane, out var plots))
                        {
                            _chart.PanePlots.Remove(virtualPane);
                            _chart.PanePlots.Add(pane, plots);
                        }

                        pane.Id = virtualPane.Id;
                    }

                    _chart.NotRender = false;

                    if (_chart.ChartControl.IsLoaded)
                    {
                        _chart.ReCalc_VisibleCandlesRange();
                        _chart.ReCalc_VisibleCandlesExtremums();
                        _chart.ChartControl.Invalidate();
                        _chart.Refresh();
                    }
                }


                OnSizeChanged(sender, null);
            }
        }




        public IEnumerable<PaneControl> GetAllPanes()
        {
            return chartView.Children.OfType<PaneControl>();
        }


        public PaneControl AddPane(string paneName)
        {
            chartView.RowDefinitions.Add(
                new RowDefinition {Height = new GridLength(1, GridUnitType.Auto)});

            var gridSplitter = new GridSplitter
            {
                ShowsPreview = false, Height = 3,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
            };
            //
            gridSplitter.SetBinding(GridSplitter.HeightProperty, new Binding("IndicatorSeparatorWidth"));
            gridSplitter.SetBinding(GridSplitter.BackgroundProperty, new Binding("IndicatorSeparatorColor"));


            Grid.SetColumn(gridSplitter, 0);
            Grid.SetRow(gridSplitter, chartView.RowDefinitions.Count - 1);

             chartView.Children.Add(gridSplitter);
            //chartView.Children.Insert(0, gridSplitter);
            var currentHeight = 0.0;
            foreach (var rowDefinition in chartView.RowDefinitions)
            {
                if (rowDefinition.Height.IsStar)
                {
                    currentHeight += rowDefinition.Height.Value;
                }
            }

            chartView.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(currentHeight / 3, GridUnitType.Star)
            });

            var paneView = new PaneControl
            {
                Name = paneName,

            };

            Grid.SetColumn(paneView, 0);
            Grid.SetRow(paneView, chartView.RowDefinitions.Count - 1);

             chartView.Children.Add(paneView);
            //chartView.Children.Insert(0, paneView);
            paneView.paneChart.PaneControl = paneView;
            paneView.valueTicksElement.PaneControl = paneView;
            paneView.SetBinding(PaneControl.ChartBackgroundProperty, new Binding("ChartBackground"));
            Panel.SetZIndex(paneView, 0);


            PaneCount++;
            paneView.Id = PaneCount.ToString();
            ReloadPaneElements();

            return paneView;
        }

        public void RemovePane()
        {
            if (PaneCount > 1)
            {
                //Remove pane
                chartView.RowDefinitions.RemoveAt(chartView.RowDefinitions.Count - 1);
                chartView.Children.RemoveAt(chartView.Children.Count - 1);

                //Remove Splitter
                chartView.RowDefinitions.RemoveAt(chartView.RowDefinitions.Count - 1);
                chartView.Children.RemoveAt(chartView.Children.Count - 1);

                PaneCount--;
            }

            ReloadPaneElements();
        }

        public void RemovePane(PaneControl paneControl)
        {
            if (PaneCount > 1)
            {
                var children = chartView.Children.OfType<PaneControl>().ToList();
                if (children.Any(x => x == paneControl))
                {
                    var index = children.IndexOf(paneControl);

                    //Remove pane
                    chartView.RowDefinitions.RemoveAt(index * 2);
                    chartView.Children.Remove(paneControl);

                    //Remove Splitter
                    chartView.RowDefinitions.RemoveAt(index * 2 - 1);
                    chartView.Children.RemoveAt(index * 2);

                    for (index *= 2; index < chartView.Children.Count; ++index)
                    {
                        var child = chartView.Children[index];
                        Grid.SetRow(child, Grid.GetRow(child) - 2);
                    }

                    PaneCount--;
                }
            }

            ReloadPaneElements();
        }

        public void RemoveAllPane()
        {
            while (PaneCount > 1)
            {
                RemovePane();
            }
        }


        public void Invalidate()
        {
            foreach (var control in chartView.Children)
            {
                if (control is PaneControl paneView)
                {
                    paneView.paneChart.Render();
                    paneView.valueTicksElement.Render();
                }
            }
        }

        private void ReloadPaneElements()
        {
            var panes = FindVisualChildren<PaneControl>(this).OrderByDescending(Grid.GetRow).ToList();
            if (!panes.Any()) return;

            for (int i = 0; i < panes.Count; i++)
            {
                var pane = panes[i];

                pane.TimeVisible = i == 0 ? Visibility.Visible : Visibility.Collapsed;
                pane.ScrollBarVisibility = i == 0 ? Visibility.Visible : Visibility.Collapsed;
                pane.ScrollBarHeight = i == 0 ? GridLength.Auto : new GridLength(0);
                //Panel.SetZIndex(pane, i  ==  (panes.Count - 1) ? 1 :2);
            }
        }

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T dependencyObject)
                    {
                        yield return dependencyObject;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void OnMouseMoveInsidePriceChartContainer(object sender, MouseEventArgs e)
        {
            _chart.CurrentMousePosition = Mouse.GetPosition(this);


            if (_chart.IsMouseInChartArea)
            {
                var dateTime = _chart.GetTimeFromPoint(_chart.CurrentMousePosition);
                _chart.CrossLinesTime = _chart.FormatTime(dateTime);
                _chart.CrossLinesOnlyTime = _chart.FormatTimeWithMilli(dateTime);
            }
            else
            {

                _chart.CrossLinesTime = null;
                _chart.CrossLinesOnlyTime = null;

            }
            _chart.ChartControl.MainPane.paneChart.Render();


        }


        private void OnSizeChanged(Object sender, SizeChangedEventArgs e)
        {
            OnSizeChanged();
        }

        public void OnSizeChanged()
        {
            if (_chart == null || MainPane == null)
            {
                return;
            }

            var lastPane = FindVisualChildren<PaneControl>(this)
                .OrderByDescending(Grid.GetRow)
                .FirstOrDefault();
            if (lastPane == null)
            {
                return;
            }


            var height = ActualHeight - lastPane.scroller.ActualHeight  - _chart.TimeAxisHeight;
            var width = ActualWidth - _chart.PriceAxisWidth;
            _chart.AxisPosition = new Point(width, height);
            _chart.CrossHairPriceTextBoxHeight = crossHairPriceTextBox.Height;
            _chart.CrossHairTimeTextBoxWidth = crossHairTimeTextBox.Width;
            _chart.UpdateProperty("AxisPosition");
        }

        private void CrossHairTimeTextBox_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            OnSizeChanged();
        }

    }
}