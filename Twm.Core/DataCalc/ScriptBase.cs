using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Twm.Chart.Classes;
using Twm.Chart.Controls;
using Twm.Chart.Interfaces;
using Twm.Core.Attributes;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.Core.CustomProperties;
using Twm.Core.CustomProperties.Editors;
using Twm.Core.DataCalc.DataSeries;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.Enums;
using Twm.Core.Interfaces;
using Twm.Core.Managers;
using Twm.Core.ViewModels;
using Twm.Model.Model;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using static System.Double;
using Application = System.Windows.Application;

namespace Twm.Core.DataCalc
{
    [CategoryOrder("Main", 1)]
    [CategoryOrder("Data series", 2)]
    [CategoryOrder("Visuals", 3)]
    [CategoryOrder("Parameters", 4)]
    public abstract class ScriptBase : DataCalcObject
    {
        protected Chart.Classes.Chart Chart;


        private Instrument _instrument;

        [Browsable(false)]
        public Instrument Instrument
        {
            get { return _instrument; }
            private set
            {
                if (value != _instrument)
                {
                    _instrument = value;
                    OnPropertyChanged();
                    _tickSize = GetTickSize();
                    OnPropertyChanged(nameof(TickSize));
                    OnPropertyChanged(nameof(Multiplier));
                    OnPropertyChanged(nameof(ConnectionName));
                }
            }
        }

        [Browsable(false)] public ViewModelBase ViewModel { get; set; }

        public List<ISeries<double>> Closes;
        public List<ISeries<double>> Opens;
        public List<ISeries<double>> Highs;
        public List<ISeries<double>> Lows;
        public List<ISeries<DateTime>> DateTimes;
        public List<ISeries<double>> Volumes;


        public ISeries<double> Close;

        public ISeries<double> Open;

        public ISeries<double> High;

        public ISeries<double> Low;

        public ISeries<DateTime> DateTime;

        public ISeries<double> Volume;

        public BarSeries CurrentBars;

        [Browsable(false)] public string LocalId { get; protected set; }

        [Browsable(false)] public Guid Guid { get; protected set; }

        [Browsable(false)]
        public ObservableCollection<ISeries<double>> Series
        {
            get
            {
                if (Chart == null)
                    return new ObservableCollection<ISeries<double>>();
                return Chart.Series;
            }
        }

        [Display(Name = "Name", GroupName = "Main", Order = 0)]
        [Category("Main")]
        public string Name { get; set; }


        [Display(Name = "Version", GroupName = "Main", Order = 1)]
        [Category("Main")]
        public string Version { get; protected set; }


        private bool _isAutoscale;
        [Display(Name = "IsAutoscale", GroupName = "Main", Order = 2)]
        [Category("Main")]
        [Browsable(false)]
        public bool IsAutoscale {
            get { return _isAutoscale; }
            set { 
                _isAutoscale = value;

                Plots.ForEach(x => x.IsAutoscale = value);
                if (Chart != null)
                    Chart.Plots.ToList().ForEach(x => x.IsAutoscale = value);
            }
        }

        /// <summary>
        /// Plot collection for UI settings
        /// </summary>
        [ExpandableObject]
        [Display(Name = "Plots", GroupName = "Visuals", Order = 2)]
        [Category("Visuals")]
        [VisibleAttribute(false, PropertyVisibility.Everywhere)]
        public ExpandableList<Plot> Plots { get; private set; }

        [Browsable(false)] public bool IsTemporary { get; set; }


        [Browsable(false)] public object Tag { get; set; }


        [Browsable(false)]
        public IEnumerable<string> TwmPropertyNames
        {
            get { return TwmPropertyValues.Keys.ToList(); }
        }

        [Browsable(false)]
        public int TwmPropertyCount
        {
            get { return TwmPropertyValues.Keys.Count; }
        }

        [Browsable(false)]
        public Dictionary<string, PropertyData> TwmPropertyTypes
        {
            get { return _tvmPropertyTypes; }
        }


        [Editor(typeof(DataSeriesEditor), typeof(DataSeriesEditor))]
        [Category("Data series")]
        [Display(Name = "Instrument", GroupName = "Data series", Order = 0)]
        [Browsable(false)]
        public DataSeriesParams DataSeriesSeriesParams
        {
            get { return DataCalcContext.GetParams(); }

            set
            {
                DataCalcContext.SetParams(new List<DataSeriesParams>() {value});
                Instrument = DataCalcContext.Instruments.FirstOrDefault();
                if (Instrument != null)
                    Connection = Session.Instance.GetConnection(Instrument.ConnectionId);
            }
        }


        private IConnection _connection;

        [Browsable(false)]
        public IConnection Connection
        {
            get { return _connection; }
            set
            {
                if (_connection != value)
                {
                    _connection = value;
                    OnPropertyChanged();
                    OnPropertyChanged("ConnectionName");
                }
            }
        }

        [Category("Data series")]
        [Display(Name = "Connection", GroupName = "Data series", Order = 1)]
        [Visible(true, PropertyVisibility.Optimizer)]
        public string ConnectionName
        {
            get
            {
                if (Connection != null)
                    return Connection.Name;

                return "";
            }
        }

        [Editor(typeof(SeriesEditor), typeof(SeriesEditor))]
        [Category("Data series")]
        [Display(Name = "Input series", GroupName = "Data series", Order = 2)]
        [Visible(false, PropertyVisibility.Optimizer, PropertyVisibility.Validator, PropertyVisibility.ShortValidator)]
        public ISeries<double> Input { get; set; }


        private double _tickSize;

        [Category("Data series")]
        [Display(Name = "TickSize", GroupName = "Data series", Order = 3)]
        [Visible(false, PropertyVisibility.ShortValidator)]
        public double TickSize
        {
            get
            {
                return _tickSize;
                /*if (Instrument != null)
                {
                    var inc = Instrument.PriceIncrements;

                    if (TryParse(inc, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                    {
                        return value;
                    }
                }

                return 0;*/
            }
        }

        private double GetTickSize()
        {
            if (Instrument != null)
            {
                var inc = Instrument.PriceIncrements;
                inc = inc.Replace(',', '.');

                if (TryParse(inc, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                {
                    return value;
                }
            }

            return 0;
        }


        [Category("Data series")]
        [Display(Name = "Multiplier", GroupName = "Data series", Order = 3)]
        [Visible(false, PropertyVisibility.Everywhere)]
        public double Multiplier
        {
            get
            {
                if (Instrument != null)
                {
                    var inc = Instrument.Multiplier.ToString();

                    if (TryParse(inc, out var value))
                    {
                        return value;
                    }
                }

                return 0;
            }
        }


        [Browsable(false)]
        public string DataSeries
        {
            get { return DataCalcContext.DataSeries; }
        }


        /// <summary>
        /// Temporary PaneControl collection for UI settings
        /// </summary>
        protected Dictionary<FrameworkElement, List<Plot>> PanePlots;


        protected Dictionary<string, object> TwmPropertyValues;
        protected Dictionary<string, PropertyData> _tvmPropertyTypes;

        protected Dictionary<string, PropertyInfo> TwmPropertyInfos;
        private Brush _barBrush;
        private Brush _backBrushAll;


        [Browsable(false)] protected List<ISeries<double>> _values;
        private Dictionary<ISeries<double>, State> SeriesStates { get; set; }


        [Browsable(false)]
        public Brush BarBrush
        {
            get => _barBrush;
            protected set
            {
                _barBrush = value;
                if (Chart != null)
                    Chart.BarColors[DataCalcContext.CurrentBar - 1] = value;
            }
        }

        [Browsable(false)]
        public Brush BackBrushAll
        {
            get => _backBrushAll;
            protected set
            {
                _backBrushAll = value;
                if (Chart != null)
                    Chart.BarBackgroundsColors[DataCalcContext.CurrentBar - 1] = value;
            }
        }

        [Browsable(false)] public ScriptOptions Options { get; set; }


        [Browsable(false)] public Draw Draw { get; private set; }


        public sealed override void SetDataCalcContext(DataCalcContext dataCalcContext)
        {
            DataCalcContext = dataCalcContext;
            CurrentBars = new BarSeries(DataCalcContext);
            Open = new DoubleSeries(DataCalcContext, CandleProperty.Open);
            High = new DoubleSeries(DataCalcContext, CandleProperty.High);
            Low = new DoubleSeries(DataCalcContext, CandleProperty.Low);
            Close = new DoubleSeries(DataCalcContext, CandleProperty.Close);
            DateTime = new DateTimeSeries(DataCalcContext, CandleProperty.DateTime);
            Volume = new DoubleSeries(DataCalcContext, CandleProperty.Volume);
            Input = new DoubleSeries(DataCalcContext, CandleProperty.Close);

            Opens = new List<ISeries<double>>() {Open};
            Highs = new List<ISeries<double>>() {High};
            Lows = new List<ISeries<double>>() {Low};
            Closes = new List<ISeries<double>>() {Close};
            DateTimes = new List<ISeries<DateTime>>() {DateTime};
            Volumes = new List<ISeries<double>>() {Volume};

            Instrument = DataCalcContext.Instruments.FirstOrDefault();
            if (Instrument != null)
                Connection = Session.Instance.GetConnection(Instrument.ConnectionId);
            CreateCollections();

            if (!IsTemporary)
                SetChart(dataCalcContext?.Chart);
        }


        private void CreateCollections()
        {
            Plots = new ExpandableList<Plot>();
            Draw = new Draw();
            PanePlots = new Dictionary<FrameworkElement, List<Plot>>();
            SeriesStates = new Dictionary<ISeries<double>, State>();
        }


        public void AddDataSeries(DataSeriesType dataSeriesType, int dataSeriesValue, string symbol = "", string type = "", string connectionCode = "")
        {
            Instrument instrument = DataCalcContext.CurrentDataSeriesParams.Instrument;
            if (!string.IsNullOrEmpty(symbol))
            {
                if (string.IsNullOrEmpty(type))
                    type = instrument.Type;

                if (string.IsNullOrEmpty(connectionCode))
                    connectionCode = Session.Instance.GetConnection(instrument.ConnectionId).Code;

                instrument = Session.Instance.GetInstrument(symbol, type, connectionCode).Result;
                if (instrument == null)
                    throw new Exception($"Symbol {symbol} not found in database");
            }            

            var dataSeries = new ExtraDataSeries
            {
                Instrument = instrument,
                PeriodStart = DataCalcContext.CurrentDataSeriesParams.PeriodStart,
                PeriodEnd = DataCalcContext.CurrentDataSeriesParams.PeriodEnd,
                DataSeriesType = dataSeriesType,
                DataSeriesValue = dataSeriesValue,
                Candles = new List<ICandle>()
            };

            if (DataCalcContext.DataSeriesCash != null &&
                DataCalcContext.DataSeriesCash.TryGetValue(dataSeries.DataSeriesCode, out var candles))
            {
                if (DataCalcContext.CancellationTokenSourceCalc.Token.IsCancellationRequested)
                    return;

                dataSeries.Candles.AddRange(candles);
            }
            else
            {
                IEnumerable<IHistoricalCandle> historicalCandles;
                var historicalDataManager = new HistoricalDataManager();
                try
                {
                    historicalDataManager.RaiseMessageEvent += HistoricalDataManager_RaiseMessageEvent;
                    historicalCandles = historicalDataManager
                        .GetData(dataSeries, DataCalcContext.CancellationTokenSourceCalc.Token).GetAwaiter()
                        .GetResult();
                }
                finally
                {
                    historicalDataManager.RaiseMessageEvent -= HistoricalDataManager_RaiseMessageEvent;
                    Chart?.Dispatcher.InvokeAsync(() =>
                        {
                            if (ViewModel != null)
                            {
                                ViewModel.Message = "Calculating...";
                                ViewModel.SubMessage = "";
                            }
                        }
                    );
                }

                foreach (var historicalCandle in historicalCandles)
                {
                    if (DataCalcContext.CancellationTokenSourceCalc.Token.IsCancellationRequested)
                        return;

                    dataSeries.Candles.Add(Session.Instance.Mapper.Map<IHistoricalCandle, ICandle>(historicalCandle));
                }
            }

            DataCalcContext.AddExtraDataSeries(dataSeries.Instrument.Symbol, dataSeries);


            var open = new DoubleSeries(DataCalcContext, CandleProperty.Open,
                DataCalcContext.ExtraDataSeries.Count - 1);
            Opens.Add(open);
            var high = new DoubleSeries(DataCalcContext, CandleProperty.High,
                DataCalcContext.ExtraDataSeries.Count - 1);
            Highs.Add(high);
            var low = new DoubleSeries(DataCalcContext, CandleProperty.Low, DataCalcContext.ExtraDataSeries.Count - 1);
            Lows.Add(low);
            var close = new DoubleSeries(DataCalcContext, CandleProperty.Close,
                DataCalcContext.ExtraDataSeries.Count - 1);
            Closes.Add(close);
            var dateTime = new DateTimeSeries(DataCalcContext, CandleProperty.DateTime,
                DataCalcContext.ExtraDataSeries.Count - 1);
            DateTimes.Add(dateTime);
            var volume = new DoubleSeries(DataCalcContext, CandleProperty.Volume,
                DataCalcContext.ExtraDataSeries.Count - 1);
            Volumes.Add(volume);


            if (Session.Instance.IsPlayback)
            {
                if (!Session.Instance.Playback.Ticks.ContainsKey(dataSeries.Instrument.Symbol))
                {
                    var playbackParams = Session.Instance.Playback.GetParams(dataSeries);
                    var historicalDataManager = new HistoricalDataManager();
                    var ticks = historicalDataManager
                        .GetData(playbackParams, DataCalcContext.CancellationTokenSourceCalc.Token).Result;

                    if (DataCalcContext.CancellationTokenSourceCalc.Token.IsCancellationRequested)
                        return;

                    Application.Current.Dispatcher.Invoke(() =>
                        {
                            Session.Instance.Playback.Ticks.Add(playbackParams.Instrument.Symbol,
                                historicalDataManager.GetTicksByHistoricalCandles(ticks,
                                    DataCalcContext.CancellationTokenSourceCalc.Token));
                        }
                    );
                }
            }

            if (Connection.IsConnected)
            {
                //var dataSeriesParams = Session.Instance.Playback.GetParams(dataSeries);

                dataSeries.DataSeriesType = DataSeriesType.Tick;
                dataSeries.DataSeriesValue = 1;

                Connection.Client.SubscribeToLive(
                    DataCalcContext.GetLiveRequest(dataSeries),
                    UpdateSeriesByTicks);
            }
        }

        private void UpdateSeriesByTicks(string symbol, IEnumerable<ICandle> ticks)
        {
            Task.Run(() => DataCalcContext.ProcessExtraTicks(symbol, ticks));
        }

        private void HistoricalDataManager_RaiseMessageEvent(object sender, Messages.MessageEventArgs e)
        {
            Chart?.Dispatcher.InvokeAsync(() =>
                {
                    if (ViewModel != null)
                    {
                        ViewModel.Message = e.Message;
                        ViewModel.SubMessage = e.SubMessage;
                    }
                }
            );
        }


        public async void SendMail(string toAddress, string subject, string body)
        {
            await EmailService.Instance.SendEmail(toAddress, subject, body);
        }

        /// <summary>
        /// Add data series
        /// </summary>
        /// <param name="series">Data series</param>
        public void AddSeries(ISeries<double> series)
        {
            if (series is DataCalcObject dco)
            {
                dco.SetDataCalcContext(DataCalcContext);
                dco.SetParent(this);
            }


            if (!_values.Contains(series))
            {
                Chart?.Dispatcher.Invoke(() => Chart?.AddSeries(series));
                _values.Add(series);
                SeriesStates.Add(series, State);
            }
            else
                throw new Exception("Series already contains!");
        }

        public void SetChart(Chart.Classes.Chart chart)
        {
            Chart = chart;
            Draw.SetChart(chart);
        }


        /// <summary>
        /// Redraws the chart associated with this object 
        /// </summary>
        public void InvalidateChart()
        {
            Chart?.Dispatcher.Invoke(() =>
                {
                    Chart?.ReCalc_VisibleCandlesExtremums();
                    Chart?.ChartControl.Invalidate();
                }
            );
        }


        public void AddPanePlot(PaneControl paneControl, Plot plot)
        {
            plot.SetName(Name);
            plot.IsAutoscale = IsAutoscale;

            if (Chart == null)
            {
                if (!IsOptimization)
                {
                    //Create temporary plot 
                    Plots.Add(plot);
                    var uc = paneControl ?? PanePlots.FirstOrDefault(x => x.Key.Tag != null && (bool) x.Key.Tag).Key ??
                             Application.Current.Dispatcher.Invoke(() => new FrameworkElement {Tag = true});


                    if (PanePlots.TryGetValue(uc, out var list))
                    {
                        list.Add(plot);
                    }
                    else
                    {
                        PanePlots.Add(uc, new List<Plot>() {plot});
                    }
                }
            }
            else
            {
                plot.SetOwner(this);
                Chart?.Dispatcher.Invoke(() => Chart.AddPanePlot(paneControl, plot));
            }
        }


        public void AddPanePlot(Plot plot)
        {
            AddPanePlot(null, plot);
        }


        /// <summary>
        /// Remove all plots created by the object
        /// </summary>
        public void RemovePlots()
        {
            Plots.Clear();
            Chart?.Dispatcher.Invoke(() => Chart?.RemovePlots(this));
        }

        /// <summary>
        /// Remove all panes created by the object
        /// </summary>
        public void RemovePanes()
        {
            Chart?.Dispatcher.Invoke(() => Chart?.RemovePanes(this));
        }

        /// <summary>
        /// Create new pane on the chart
        /// </summary>
        /// <param name="paneName">pane name </param>
        /// <returns>pane view</returns>
        public PaneControl AddPane(string paneName)
        {
            PaneControl pane = null;


            if (Chart == null)
            {
                if (!IsOptimization)
                {
                    Application.Current.Dispatcher.Invoke(() => { pane = new PaneControl {Name = paneName}; }
                    );
                    //Create temporary PaneControl

                    if (!PanePlots.ContainsKey(pane))
                    {
                        PanePlots.Add(pane, new List<Plot>());

                        //pane.Id = (PanePlots.Keys.Count + 1).ToString();
                        pane.Id = Guid.NewGuid().ToString();
                    }
                }
            }
            else
            {
                Chart.Dispatcher.Invoke(() =>
                {
                    pane = Chart?.AddPane(paneName);
                    pane?.SetOwner(this);
                });
            }

            return pane;
        }

        /// <summary>
        /// Create new pane on the chart
        /// </summary>
        /// <returns>pane view</returns>
        public PaneControl AddPane()
        {
            return AddPane(string.Empty);
        }


        /// <summary>
        /// Clear all collections where object owner and clear reference to the Chart and DataCalcContext
        /// </summary>
        public virtual void Clear()
        {
            DataCalcContext?.CancelCalc();
            RemovePlots();
            RemovePanes();
            PanePlots.Clear();
            Chart = null;
            DataCalcContext = null;
        }


        public virtual void Reset()
        {
            var seriesStates = SeriesStates.ToList();
            foreach (var seriesState in seriesStates)
            {
                var series = seriesState.Key;
                if (seriesState.Value == State.SetDefaults)
                {
                    series.Reset();
                }
                else if (seriesState.Value == State.Configured)
                {
                    series.Clear();
                    _values.Remove(series);
                    Chart?.Dispatcher.Invoke(() => Chart?.RemoveSeries(series));
                    SeriesStates.Remove(series);
                }
            }

            if (Opens.Count > 1)
                Opens.RemoveRange(1, Opens.Count - 1);

            if (Highs.Count > 1)
                Highs.RemoveRange(1, Highs.Count - 1);

            if (Lows.Count > 1)
                Lows.RemoveRange(1, Lows.Count - 1);

            if (Closes.Count > 1)
                Closes.RemoveRange(1, Closes.Count - 1);

            if (DateTimes.Count > 1)
                DateTimes.RemoveRange(1, DateTimes.Count - 1);

            if (Volumes.Count > 1)
                Volumes.RemoveRange(1, Volumes.Count - 1);


            Input.Reset();

            if (Input is IndicatorBase ib && ib.DataCalcContext == null)
            {
                ib.DataCalcContext = DataCalcContext;
            }

            Draw.Reset();
        }


        /// <summary>
        /// Synchronize temporary PaneControl collection with chart
        /// </summary>
        public void SynchronizeChart(bool remove = true)
        {
            if (Chart != null)
            {
                foreach (var panePlot in PanePlots)
                {
                    if (panePlot.Key.Tag == null || !(bool) panePlot.Key.Tag)
                    {
                        var pane = Chart.AddPane(panePlot.Key.Name);
                        pane.Id = ((PaneControl) panePlot.Key).Id;
                        pane.SetOwner(this);
                        foreach (var plot in panePlot.Value)
                        {
                            plot.SetOwner(this);
                            Chart.Dispatcher.Invoke(() => Chart.AddPanePlot(pane, plot));
                        }
                    }
                    else
                    {
                        foreach (var plot in panePlot.Value)
                        {
                            plot.SetOwner(this);
                            Chart.Dispatcher.Invoke(() => Chart.AddPanePlot(null, plot));
                        }
                    }
                }

                if (remove)
                    PanePlots.Clear();
            }
        }


        public void SetInput(ISeries<double> input)
        {
            if (input != null)
            {
                Input = input;
                OnPropertyChanged("DisplayName");
            }
        }


        public void SetTwmProperties()
        {
            TwmPropertyValues = new Dictionary<string, object>();
            _tvmPropertyTypes = new Dictionary<string, PropertyData>();
            TwmPropertyInfos = new Dictionary<string, PropertyInfo>();
            var properties = GetType().GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(TwmPropertyAttribute)));

            foreach (var property in properties)
            {
                TwmPropertyValues.Add(property.Name, property.GetValue(this));


                var displayName = property.Name;
                var attrs = property.GetCustomAttributes(false);
                var attr = attrs.FirstOrDefault(x => x is DisplayAttribute);
                if (attr != null && attr is DisplayAttribute displayAttribute)
                {
                    displayName = displayAttribute.Name;
                }


                _tvmPropertyTypes.Add(property.Name,
                    new PropertyData() {DisplayName = displayName, Type = property.PropertyType});


                TwmPropertyInfos.Add(property.Name, property);
            }

            OnPropertyChanged("DisplayName");
        }


        public void CheckTwmProperty(string propertyName, object value)
        {
            if (TwmPropertyValues.Keys.Contains(propertyName))
            {
                TwmPropertyValues[propertyName] = value;
                OnPropertyChanged("DisplayName");
            }
        }


        public bool IsTwmProperty(string propertyName)
        {
            if (TwmPropertyValues.Keys.Contains(propertyName))
                return true;
            return false;
        }


        public object GetTwmPropertyValue(string propertyName)
        {
            if (TwmPropertyInfos.TryGetValue(propertyName, out var info))
            {
                return info.GetValue(this);
            }

            return null;
        }


        public void SetTwmPropertyValue(string propertyName, object value)
        {
            if (TwmPropertyInfos.TryGetValue(propertyName, out var info))
            {
                if (info.PropertyType == typeof(int))
                    info.SetValue(this, Convert.ChangeType(value, typeof(int), CultureInfo.InvariantCulture));
                else if (info.PropertyType == typeof(double))
                    info.SetValue(this, Convert.ChangeType(value, typeof(double), CultureInfo.InvariantCulture));
                else if (info.PropertyType.IsEnum)
                {
                    if (value is string stringValue)
                    {
                        
                        
                        var intValue = (int)Enum.Parse(info.PropertyType, stringValue);
                        info.SetValue(this, intValue);
                    }
                    else
                        info.SetValue(this, Convert.ChangeType(value, Enum.GetUnderlyingType(info.PropertyType)));
                }
                else
                    info.SetValue(this, value);

                TwmPropertyValues[propertyName] = value;
            }
        }

        public Dictionary<string, object> GetTwmPropertyValues()
        {
            var dict = new Dictionary<string, object>();
            foreach (var info in TwmPropertyInfos)
            {
                dict.Add(info.Key, info.Value.GetValue(this));
            }

            return dict;
        }


        public bool? CheckBrowsableProperty(string propertyName)
        {
            if (propertyName == "Optimizer")
            {
                return false;
            }

            if (propertyName == "DataSeriesSeriesParams")
            {
                return false;
            }

            if (propertyName == "OptimizerTypes")
            {
                return false;
            }

            return null;
        }


        public void CopyPanePlots(ScriptBase script)
        {
            PanePlots = new Dictionary<FrameworkElement, List<Plot>>();

            if (script.PanePlots != null)
                foreach (var panePlot in script.PanePlots)
                {
                    var pane = Application.Current.Dispatcher.Invoke(
                        () => new FrameworkElement {Tag = panePlot.Key.Tag});

                    List<Plot> plots = new List<Plot>();
                    foreach (var plot in panePlot.Value)
                    {
                        var clonePlot = (Plot) plot.Clone();

                        if (plot.Owner == script)
                        {
                            clonePlot.SetOwner(this);
                        }

                        plots.Add(clonePlot);
                    }

                    PanePlots.Add(pane, plots);
                }
        }

        public void ClearLocalVariables()
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                     | BindingFlags.Static;

            var fields = GetType().GetFields(bindFlags)
                .Where(x => x.Name.StartsWith("_") && x.DeclaringType != typeof(ScriptBase));

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(double))
                {
                    field.SetValue(this, 0.0);
                }

                if (field.FieldType == typeof(int))
                {
                    field.SetValue(this, 0);
                }

                if (field.FieldType == typeof(bool))
                {
                    field.SetValue(this, false);
                }
            }
        }

        public int ToTime(DateTime time)
        {
            return time.Hour * 100 + time.Minute;
        }
    }
}