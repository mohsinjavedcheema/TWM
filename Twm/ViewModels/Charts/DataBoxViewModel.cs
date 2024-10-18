using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Twm.Chart;
using Twm.Chart.Annotations;
using Twm.Core.Market;
using Twm.ViewModels.Positions;
using ChartClass=Twm.Chart.Classes.Chart;

namespace Twm.ViewModels.Charts
{
    internal sealed class DataBoxViewModel : INotifyPropertyChanged, IDisposable
    {
        public sealed class PanelViewModel
        {
            public PanelViewModel(
                int index) =>
                Name = $"Panel {index}";

            public string Name { get; }

            public ObservableCollection<PlotViewModel> Plots { get; } =
                new ObservableCollection<PlotViewModel>();
        }

        public sealed class PlotViewModel
        {
            public PlotViewModel(
                string name) =>
                Name = name;

            public string Name { get; }

            public ObservableCollection<ItemViewModel> Items { get; } =
                new ObservableCollection<ItemViewModel>();
        }

        public sealed class ItemViewModel
        {
            public ItemViewModel(
                string name, 
                string value)
                : this(name, value, 
                    Brushes.White, Brushes.Black)
            {
            }

            public ItemViewModel(
                string name, 
                string value,
                Brush background)
                : this(name, value, background,
                    GetContrast(background))
            {
            }

            public ItemViewModel(
                string name, 
                string value,
                Brush background,
                Brush foreground)
            {
                Name = name;
                Value = value;
                Background = background;
                Foreground = foreground;
            }

            public string Name { get; }

            public string Value { get; }

            public Brush Background { get; }

            public Brush Foreground { get; }

            private static Brush GetContrast(Brush background)
            {
                if (background is SolidColorBrush solidColorBrush)
                {
                    var color = solidColorBrush.Color;
                    var Y = 0.2126 * color.ScR + 0.7152 * color.ScG + 0.0722 * color.ScB;
                    return Y > 0.4 ? Brushes.Black : Brushes.White;
                }

                return background;
            }
        }

        private readonly ChartViewModel _chartViewModel;

        private readonly ChartClass _chart;

        private string _date;

        private string _floatFormat;

        public DataBoxViewModel()
        {
            Date = "22.12.2020";

            var mainPanel = new PanelViewModel(1);
            var candlePlot = new PlotViewModel("M2K 06-12");

            candlePlot.Items.Add(new ItemViewModel("Time", "05:00:00"));
            candlePlot.Items.Add(new ItemViewModel("Open", "1967.0"));
            candlePlot.Items.Add(new ItemViewModel("High", "1969.5"));
            candlePlot.Items.Add(new ItemViewModel("Low", "1960.8"));
            candlePlot.Items.Add(new ItemViewModel("Close", "1961.2"));
            candlePlot.Items.Add(new ItemViewModel("Volume", "1,222"));

            mainPanel.Plots.Add(candlePlot);
            Panels.Add(mainPanel);

            var smaPlot = new PlotViewModel("SMA");
            smaPlot.Items.Add(new ItemViewModel("SMA", "1953.9", Brushes.DarkGoldenrod));
            mainPanel.Plots.Add(smaPlot);

            var adxPlot = new PlotViewModel("ADX");
            adxPlot.Items.Add(new ItemViewModel("ADX", "18.45", Brushes.SeaGreen));
            var adxPanel = new PanelViewModel(2);
            adxPanel.Plots.Add(adxPlot);
            Panels.Add(adxPanel);
        }

        public DataBoxViewModel(
            ChartViewModel chartViewModel)
        {
            _chartViewModel = chartViewModel;
            _chart = chartViewModel.Chart;
            _chart.PropertyChanged += HandlePropertyChanged;
            _floatFormat = "F2";
            if (double.TryParse(_chartViewModel.DataCalcContext.CurrentDataSeriesParams.Instrument.PriceIncrements.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture,
                             out var tickSize))
            {
                _floatFormat = "F" + tickSize.GetDecimalCount();
            }

        }

        public string Date
        {
            get => _date;
            set
            {
                if (string.Equals(_date, value, StringComparison.Ordinal))
                {
                    return;
                }

                _date = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<PanelViewModel> Panels { get; } =
            new ObservableCollection<PanelViewModel>();

        public void Dispose()
        {
            _chart.PropertyChanged -= HandlePropertyChanged;
            _chartViewModel.IsDataBoxVisible = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void HandlePropertyChanged(
            object sender, PropertyChangedEventArgs e)
        {
            var index = (int) Math.Floor(
                _chart.CurrentMousePosition.X / (_chart.CandleWidth + _chart.CandleGap));
            if (index >= 0 && index < _chart.VisibleCandlesRange.Count)
            {
                var panelIndex = 1;
                index += _chart.VisibleCandlesRange.Start_i;

               

                var candle = _chart.CandlesSource[index];
                Date = candle.t.ToString("d", CultureInfo.CurrentCulture);

                var mainPanel = new PanelViewModel(panelIndex++);
                var candlePlot = new PlotViewModel(_chart.Symbol); // TODO: olegra - bar size

                candlePlot.Items.Add(new ItemViewModel(
                    "Time", candle.t.ToString("T", CultureInfo.InvariantCulture)));
                candlePlot.Items.Add(new ItemViewModel(
                    "CloseTime", candle.ct.ToString("T", CultureInfo.InvariantCulture)));
                candlePlot.Items.Add(new ItemViewModel(
                    "Open", candle.O.ToString(_floatFormat, CultureInfo.InvariantCulture)));
                candlePlot.Items.Add(new ItemViewModel(
                    "High", candle.H.ToString(_floatFormat, CultureInfo.InvariantCulture)));
                candlePlot.Items.Add(new ItemViewModel("Low", 
                    candle.L.ToString(_floatFormat, CultureInfo.InvariantCulture)));
                candlePlot.Items.Add(new ItemViewModel("Close", 
                    candle.C.ToString(_floatFormat, CultureInfo.InvariantCulture)));
                candlePlot.Items.Add(new ItemViewModel("Volume", 
                    candle.V.ToString(_floatFormat, CultureInfo.InvariantCulture)));
                candlePlot.Items.Add(new ItemViewModel("Current Bar",
                    index.ToString()));

                mainPanel.Plots.Add(candlePlot);

                Panels.Clear();
                Panels.Add(mainPanel);

                foreach (var kvp in _chart.PanePlots)
                {
                    var panel = mainPanel;
                    if (!kvp.Key.IsMainPane)
                    {
                        panel = new PanelViewModel(panelIndex++);
                        Panels.Add(panel);
                    }

                    foreach (var plots in kvp.Value
                        .GroupBy(_ => _.ParentName, StringComparer.Ordinal))
                    {
                        var plot = new PlotViewModel(plots.Key);
                        foreach (var item in plots)
                        {
                            var brush = item.PlotColors != null &&
                                        item.PlotColors.TryGetValue(index, out var color)
                                ? color : new SolidColorBrush(item.Color);
                            var value = item.DataSource?.GetValueAt(index) ?? 0D;
                            plot.Items.Add(new ItemViewModel(item.Name, 
                                value.ToString(_floatFormat, CultureInfo.InvariantCulture), brush));
                        }
                        panel.Plots.Add(plot);
                    }
                }
            }
            else
            {
                Date = null;
                Panels.Clear();
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(
            [CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
