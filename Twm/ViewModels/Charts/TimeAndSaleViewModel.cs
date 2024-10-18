using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Twm.Chart;
using Twm.Chart.Annotations;
using Twm.Chart.Interfaces;

namespace Twm.ViewModels.Charts
{
    public  class TimeAndSaleViewModel : INotifyPropertyChanged, IDisposable
    {
        public ObservableCollection<ItemViewModel> Items { get; set; }

        public sealed class ItemViewModel
        {
            public ItemViewModel(
                DateTime dateTime, 
                double value,
                double volume 
                )
                : this(dateTime, value, volume,
                    Brushes.White, Brushes.Black)
            {
            }

            public ItemViewModel(
                DateTime dateTime,
                double value,
                double volume,
                Brush background)
                : this(dateTime, value, volume, background,
                    GetContrast(background))
            {
            }

            public ItemViewModel(
                DateTime dateTime,
                double value,
                double volume,
                Brush background,
                Brush foreground)
            {
                DateTime = dateTime;
                Value = value;
                Volume = volume;
                Background = background;
                Foreground = foreground;
            }


            public string ValueFormat { get; set; }
           
            

            public DateTime DateTime { get; }

            public double Value { get; }

            public double Volume { get; }

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

        public string Header { set; get; }

        public TimeAndSaleViewModel()
        {
            Items = new ObservableCollection<ItemViewModel>(); 
            Items.Add(new ItemViewModel(new DateTime(2022, 06,14, 12,23,12),123.34, 2 ));
            Items.Add(new ItemViewModel(new DateTime(2022, 06, 14, 12, 23, 13), 233.34, 4));
            Items.Add(new ItemViewModel(new DateTime(2022, 06, 14, 12, 23, 14), 133.3, 3));
        }

        public TimeAndSaleViewModel(
            ChartViewModel chartViewModel, double tickSize)
        {
            Items = new ObservableCollection<ItemViewModel>();            
            _chartViewModel = chartViewModel;
            _valueFormat = "{}{0:0." + "".PadRight(tickSize.GetDecimalCount(), '0') + "}";
        }

        private string _valueFormat;

        public void Dispose()
        {
            _chartViewModel.IsTimeAndSaleVisible = false;
        }

        public void AddItems(IEnumerable<ICandle> ticks)
        {
            foreach (var tick in ticks)
            {
                var item = new ItemViewModel(tick.t, tick.C, tick.V);
                item.ValueFormat = _valueFormat;
                Items.Insert(0, item);
            }
        }

        public void Clear()
        {
            Items.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(
            [CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        
    }
}
