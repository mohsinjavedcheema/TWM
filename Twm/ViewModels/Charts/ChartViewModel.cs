using Twm.Chart.Classes;
using Twm.Chart.Controls;
using Twm.Chart.Interfaces;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.Core.DataCalc;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.Enums;
using Twm.Core.Interfaces;
using Twm.Core.ViewModels;
using Twm.Core.ViewModels.DataSeries;
using Twm.ViewModels.ScriptObjects;
using Twm.ViewModels.Strategies;
using Twm.Windows;
using Twm.Windows.Charts;
using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Twm.Core.Managers;
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;
using MessageBox = System.Windows.Forms.MessageBox;
using OrderChangeEventArgs = Twm.Chart.Classes.OrderChangeEventArgs;

namespace Twm.ViewModels.Charts
{
    public class ChartViewModel : ViewModelBase
    {
        private DataBoxWindow _dataBoxWindow;
        private TimeAndSaleWindow _timeAndSaleWindow;

        private TimeAndSaleViewModel _timeAndSaleViewModel;
        public ChartWindow ChartWindow { get; set; }

        private IConnection _connection;
        public IConnection Connection
        {
            get { return _connection; }
            set
            {
                if (_connection != value)
                {
                    _connection = value;
                    if (ChartTrader != null)
                        ChartTrader.Connection = _connection;
                    OnPropertyChanged();
                }
            }
        }


        private Chart.Classes.Chart _chart;
        public Chart.Classes.Chart Chart
        {
            get { return _chart; }
            set
            {
                if (_chart != value)
                {
                    _chart = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Header
        {
            get
            {
                if (DataCalcContext?.CurrentDataSeriesParams == null)
                    return "";

                var seriesName = DataCalcContext.CurrentDataSeriesParams.DataSeriesName;

                var connectionName =
                    Session.Instance.GetConnection(DataCalcContext.CurrentDataSeriesParams.Instrument.ConnectionId).Name;

                if (!DataCalcContext.Strategies.Any())
                    return $"{seriesName} ({connectionName})";

                var strategyName = DataCalcContext.Strategies.FirstOrDefault().Name;

                return $"{seriesName} - {strategyName} ({connectionName})";
            }
        }

        private bool _isDataBoxVisible;
        public bool IsDataBoxVisible
        {
            get => _isDataBoxVisible;
            set
            {
                if (_isDataBoxVisible == value)
                {
                    return;
                }

                _isDataBoxVisible = value;
                OnPropertyChanged();
            }
        }


        private bool _timeAndSaleVisible;
        public bool IsTimeAndSaleVisible
        {
            get => _timeAndSaleVisible;
            set
            {
                if (_timeAndSaleVisible == value)
                {
                    return;
                }

                _timeAndSaleVisible = value;
                OnPropertyChanged();
            }
        }

        private StrategyViewModel _strategy;
        public StrategyViewModel Strategy
        {
            get { return _strategy; }
            set
            {
                if (_strategy != value)
                {
                    if (_strategy != null)
                    {
                        _strategy.Enabled = false;
                    }

                    _strategy = value;

                    OnPropertyChanged();
                }
            }
        }


        private DrawingToolsViewModel _drawingTools;
        public DrawingToolsViewModel DrawingTools
        {
            get { return _drawingTools; }
            set
            {
                if (_drawingTools != value)
                {
                    _drawingTools = value;
                    OnPropertyChanged();
                }
            }
        }


        private ChartTraderViewModel _chartTrader;
        public ChartTraderViewModel ChartTrader
        {
            get { return _chartTrader; }
            set
            {
                if (_chartTrader != value)
                {
                    _chartTrader = value;
                    OnPropertyChanged();
                }
            }
        }


        public bool IsVisible { get; set; }
        public bool IsPerformanceChart { get; set; }

        private ChartParamsViewModel ChartParams { get; set; }

        public DataCalcContext DataCalcContext { get; set; }


        public OperationCommand ChangeParamsCommand { get; set; }
        public OperationCommand SelectIndicatorCommand { get; set; }
        public OperationCommand SelectStrategyCommand { get; set; }
        public OperationCommand DataBoxCommand { get; }

        public OperationCommand TimeAndSaleCommand { get; }
        public OperationCommand CompileProjectCommand { get; private set; }
        public OperationCommand CrossLineCommand { get; }

        public OperationCommand ReloadScriptCommand { get; set; }

        public OperationCommand PerformanceCommand { get; }

        public OperationCommand PerformanceLiveOrdersCommand { get; }

        public OperationCommand PerformanceLiveTradesCommand { get; }

        public OperationCommand ChartTraderCommand { get; set; }

        public OperationCommand DrawingToolsCommand { get; set; }

        private readonly object _busyLock = new object();

        private CancellationTokenSource _ctsFetchData;

        private CancellationTokenSource _ctsLiveData;

        private Task _liveTask;

        private readonly object _liveTaskLock = new object();



        public ChartViewModel(DataSeriesParamsViewModel chartSeriesParamsViewModel,
            DataCalcContext dataCalcContext = null) : this(dataCalcContext, true)
        {


            ChartParams = new ChartParamsViewModel();
            ChartParams.ChartParams.Add(chartSeriesParamsViewModel);

            IsDrawingToolsVisible = false;
            DrawingToolsVisibility = Visibility.Collapsed;


            ChartTraderButtonVisibility = Visibility.Collapsed;
            IsChartTraderVisible = false;
            ChartTraderVisibility = Visibility.Collapsed;

            Connection = Session.GetConnection(chartSeriesParamsViewModel.Instrument.ConnectionId);

            DataCalcContext.Connection = Connection;

        }

        public ChartViewModel(ChartParamsViewModel chartParamsViewModel) : this()
        {
            ChartTraderButtonVisibility = Visibility.Visible;
            SetChartTrader();
            SetDrawingTools();
            ChartParams = chartParamsViewModel;
            var chartParams = ChartParams.ChartParams.FirstOrDefault();
            if (chartParams != null)
            {
                ChartTrader.Instrument = chartParams.Instrument;
                Connection = Session.GetConnection(chartParams.Instrument.ConnectionId);
                //ChartTrader.Init();
                DataCalcContext.Connection = Connection;
            }
            DataCalcContext.IsSingleChart = true;

            Session.Instance.OnConnectionStatusChanged += Instance_OnConnectionStatusChanged;
            DataCalcContext.SubscribeConnectionStatusChanged();
        }




        public Visibility PerformanceButtonVisibility
        {
            get
            {
                if (IsPerformanceChart)
                {
                    return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }
        }


        private Visibility _chartTraderButtonVisibility;
        public Visibility ChartTraderButtonVisibility
        {
            get { return _chartTraderButtonVisibility; }
            set
            {
                if (_chartTraderButtonVisibility != value)
                {
                    _chartTraderButtonVisibility = value;
                    OnPropertyChanged();
                }
            }
        }


        private Visibility _chartTraderVisibility;
        public Visibility ChartTraderVisibility
        {
            get { return _chartTraderVisibility; }
            set
            {
                if (_chartTraderVisibility != value)
                {
                    _chartTraderVisibility = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isChartTraderVisible;
        public bool IsChartTraderVisible
        {
            get => _isChartTraderVisible;
            set
            {
                if (_isChartTraderVisible == value)
                {
                    return;
                }

                _isChartTraderVisible = value;
                OnPropertyChanged();
            }
        }



        private Visibility _drawingToolsVisibility;
        public Visibility DrawingToolsVisibility
        {
            get { return _drawingToolsVisibility; }
            set
            {
                if (_drawingToolsVisibility != value)
                {
                    _drawingToolsVisibility = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isDrawingToolsVisible;
        public bool IsDrawingToolsVisible
        {
            get => _isDrawingToolsVisible;
            set
            {
                if (_isDrawingToolsVisible == value)
                {
                    return;
                }

                _isDrawingToolsVisible = value;
                OnPropertyChanged();
            }
        }


        #region Constructor Init

        private ChartViewModel(DataCalcContext dataCalcContext = null, bool isPerformanceChart = false)
        {
            IsPerformanceChart = isPerformanceChart;
            IsVisible = true;
            CreateChart();
            CheckForIcon();
            SetDataCalcContext(dataCalcContext);


            SelectIndicatorCommand = new OperationCommand(SelectIndicator);
            ReloadScriptCommand = new OperationCommand(ReloadScript);
            DataBoxCommand = new OperationCommand(DataBox);
            CrossLineCommand = new OperationCommand(CrossLine);
            ChangeParamsCommand = new OperationCommand(ChangeParam);
            SelectStrategyCommand = new OperationCommand(SelectStrategy);
            PerformanceCommand = new OperationCommand(Performance);
            CompileProjectCommand = new OperationCommand(CompileProject);
            PerformanceLiveOrdersCommand = new OperationCommand(PerformanceLiveOrders);
            PerformanceLiveTradesCommand = new OperationCommand(PerformanceLiveTrades);
            TimeAndSaleCommand = new OperationCommand(TimeAndSale);
            ChartTraderCommand = new OperationCommand(ShowChartTrader);
            DrawingToolsCommand = new OperationCommand(ShowDrawingTools);



        }

        private void CreateChart()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Chart = new Chart.Classes.Chart()
                {
                    MarkerTextBrush = new SolidColorBrush(SystemOptions.Instance.MarkerTextColor),
                    TradeBuyArrowFill = (Brush)(new SolidColorBrush(SystemOptions.Instance.TradeBuyColor))
                        .GetCurrentValueAsFrozen(),
                    TradeSellArrowFill = (Brush)(new SolidColorBrush(SystemOptions.Instance.TradeSellColor))
                        .GetCurrentValueAsFrozen(),
                    BearishCandleFill = (Brush)(new SolidColorBrush(SystemOptions.Instance.DownBarColor))
                        .GetCurrentValueAsFrozen(),
                    BullishCandleFill =
                        (Brush)(new SolidColorBrush(SystemOptions.Instance.UpBarColor)).GetCurrentValueAsFrozen(),
                    BearishCandleStroke = (Brush)(new SolidColorBrush(SystemOptions.Instance.CandleOutlineColor))
                        .GetCurrentValueAsFrozen(),
                    BullishCandleStroke = (Brush)(new SolidColorBrush(SystemOptions.Instance.CandleOutlineColor))
                        .GetCurrentValueAsFrozen(),
                    WickCandleStroke = (Brush)(new SolidColorBrush(SystemOptions.Instance.CandleWickColor))
                        .GetCurrentValueAsFrozen(),
                    ChartBackground = (Brush)(new SolidColorBrush(SystemOptions.Instance.ChartBackgroundColor))
                        .GetCurrentValueAsFrozen(),
                    TextColor =
                        (Brush)(new SolidColorBrush(SystemOptions.Instance.TextColor)).GetCurrentValueAsFrozen(),
                    IndicatorSeparatorColor =
                        (Brush)(new SolidColorBrush(SystemOptions.Instance.IndicatorSeparatorColor))
                        .GetCurrentValueAsFrozen(),
                    IndicatorSeparatorWidth = (int)SystemOptions.Instance.IndicatorSeparatorWidth,


                    VerticalGridlinesPen = (Pen)(new Pen(
                        (Brush)new SolidColorBrush(SystemOptions.Instance.ChartVGridColor).GetCurrentValueAsFrozen(),
                        Settings.DefaultVerticalGridlinesThickness)).GetCurrentValueAsFrozen(),
                    HorizontalGridlinesPen = (Pen)(new Pen(
                        (Brush)new SolidColorBrush(SystemOptions.Instance.ChartHGridColor).GetCurrentValueAsFrozen(),
                        Settings.DefaultHorizontalGridlinesThickness)).GetCurrentValueAsFrozen(),
                    PlotExecutions = (int)SystemOptions.Instance.PlotExecutions
                };

                if (IsPerformanceChart)
                {
                    Chart.ChartControl = new ChartControl() { DataContext = Chart };
                }
            });
        }

        /*private void SetDxFeedIconLogic()
        {
            Session.PropertyChanged += CheckConnection;
            CheckForIcon();
        }

        private void CheckConnection(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName.Equals("CurrentConnection"))
            {
                CheckForIcon();
            }
        }*/

        private void SetDataCalcContext(DataCalcContext dataCalcContext)
        {
            if (dataCalcContext == null)
                DataCalcContext = new DataCalcContext() { Chart = Chart, AllowLive = true };
            else
            {
                DataCalcContext = dataCalcContext;
                dataCalcContext.Chart = Chart;
            }

            DataCalcContext.Strategies.CollectionChanged += Strategies_CollectionChanged;

            if (!Session.Instance.DataCalcContexts.Contains(DataCalcContext))
                Session.Instance.DataCalcContexts.Add(DataCalcContext);
        }

        private void SetChartTrader()
        {

            

            ChartTrader = new ChartTraderViewModel();

            ChartTraderVisibility = IsChartTraderVisible ? Visibility.Visible : Visibility.Collapsed;
            ChartTrader.Chart = Chart;
            Chart.OrderCanceled += OrderCanceled;
            Chart.OrderChanged += OrderChanged;

        }
       

        private void SetDrawingTools()
        {

            DrawingTools = new DrawingToolsViewModel();

            DrawingToolsVisibility = IsDrawingToolsVisible ? Visibility.Visible : Visibility.Collapsed;
            DrawingTools.Chart = Chart;
            DrawingTools.Color = Colors.Blue;
            DrawingTools.Width = 2;
            DrawingTools.Size = 12;
        }

        private void CheckForIcon()
        {            
            Chart.IsDarkTheme = SystemOptions.Instance.SelectedTheme.IsDark;
        }


        #endregion

        private void Instance_OnConnectionStatusChanged(object sender, ConnectionStatusChangeEventArgs e)
        {
            if (Connection != null && Connection.Id == e.ConnectionId && e.NewStatus == ConnectionStatus.Connected)
            {
                if (ChartTrader != null && ChartTrader.Account == null)
                    ChartTrader.Account = Session.Instance.GetAccount(e.ConnectionId);
                FetchData();
                CheckForIcon();
            }
        }

        private void OrderCanceled(object sender, string orderGuid)
        {
            ChartTrader.Account.CancelOrder(orderGuid);
        }

        private void OrderChanged(object sender, OrderChangeEventArgs eventArgs)
        {
            if (eventArgs.Quantity != null)
            {
                ChartTrader.Account.ChangeOrderQuantity(eventArgs.Guid, eventArgs.Quantity.Value);
            }

            if (eventArgs.Price != null)
            {
                ChartTrader.Account.ChangeOrderPrice(eventArgs.Guid, eventArgs.Price.Value);
            }
        }

        private void ShowChartTrader(object obj)
        {
            ChartTraderVisibility = IsChartTraderVisible ? Visibility.Visible : Visibility.Collapsed;
            Chart.IsChartTraderVisible = IsChartTraderVisible;
        }

        private void ShowDrawingTools(object obj)
        {
            DrawingToolsVisibility = IsDrawingToolsVisible ? Visibility.Visible : Visibility.Collapsed;
            Chart.IsDrawingToolsVisible = IsDrawingToolsVisible;
        }

        private void Strategies_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Header");
        }

        private void CompileProject(object obj)
        {
            BuildController.Instance.CompileProject();
        }


        public override void FetchData()
        {
            var isStrategyEnable = false;
            if (Strategy != null && Strategy.Enabled)
            {
                Strategy.Enabled = false;
                isStrategyEnable = true;
                if (Strategy.Strategy.Account == null)
                {
                    Strategy.Strategy.Account = Session.Instance.GetAccount(Connection.Id);
                    Strategy.UpdateProperty("AccountName");
                }
            }


            _ctsFetchData = new CancellationTokenSource();
            var oldRange = Chart.VisibleCandlesRange;
            Chart.VisibleCandlesRange = IntRange.Undefined;
            Task.Run(() => LoadData(_ctsFetchData.Token), _ctsFetchData.Token)
                .ContinueWith(x => GetLiveData(), TaskContinuationOptions.OnlyOnRanToCompletion)
                .ContinueWith(x =>
                {
                    //var count = DataCalcContext.Candles.Count - 1;
                    //Chart. VisibleCandlesRange = new IntRange(count - oldRange.Count, oldRange.Count);
                    if (DataCalcContext.State != State.Undefined)
                    {
                        Chart.Dispatcher.InvokeAsync(() => { ReloadScript(isStrategyEnable); }
                        );
                    }
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private async Task LoadData(CancellationToken token)
        {
            lock (_busyLock)
            {
                IsBusy = true;
            }
            var historicalDataManager = new HistoricalDataManager();
            try
            {
                DataCalcContext.SetParams(ChartParams.ChartParams.ToList<DataSeriesParams>());
                OnPropertyChanged("Header");
                foreach (var chartParam in ChartParams.ChartParams)
                {
                    historicalDataManager.RaiseMessageEvent += HistoricalDataManagerRaiseMessageEvent;

                    var historicalCandles = await historicalDataManager.GetData(chartParam, token);

                    if (token.IsCancellationRequested)
                        return;

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            var candleSource = new ObservableCollection<ICandle>();

                            foreach (var historicalCandle in historicalCandles)
                            {
                                if (token.IsCancellationRequested)
                                    return;

                                candleSource.Add(Session.Mapper.Map<IHistoricalCandle, ICandle>(historicalCandle));
                            }

                            DataCalcContext.Candles = candleSource;

                            Chart.Symbol = chartParam.Instrument.Symbol;
                            double.TryParse(chartParam.Instrument.PriceIncrements.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture,
                               out var tickSize);
                            Chart.TickSize = tickSize;
                            BindingOperations.SetBinding(Chart, Twm.Chart.Classes.Chart.CandlesSourceProperty,
                                new Binding() { Source = DataCalcContext, Path = new PropertyPath("Candles") });
                        }
                    );


                    if (Session.IsPlayback)
                    {
                        var playbackParams = Session.Playback.GetParams((DataSeriesParams)chartParam.Clone());
                        var ticks = await historicalDataManager.GetData(playbackParams, token);

                        if (token.IsCancellationRequested)
                            return;

                        DataCalcContext.SubscribePlayback();
                        DataCalcContext.CreateToken();

                        await Application.Current.Dispatcher.InvokeAsync(() =>
                            {
                                Session.Playback.Symbol = playbackParams.Instrument.Symbol;

                                if (!Session.Playback.Ticks.ContainsKey(playbackParams.Instrument.Symbol))
                                {
                                    Session.Playback.Ticks.Add(playbackParams.Instrument.Symbol,
                                        historicalDataManager.GetTicksByHistoricalCandles(ticks, token));
                                }

                                DataCalcContext.SetPlaybackTicks(Session.Playback.GetCurrentTicks(Session.Playback.Symbol), false);
                            }
                        );




                    }
                }




            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                historicalDataManager.RaiseMessageEvent -= HistoricalDataManagerRaiseMessageEvent;
                lock (_busyLock)
                {
                    IsBusy = false;
                }
            }
        }

        private async Task GetLiveData()
        {
            try
            {
                if (Connection.IsConnected)
                {
                    if (!Session.IsPlayback)
                    {
                        lock (_busyLock)
                        {
                            IsBusy = true;
                        }

                        Message = "Go to live...";
                        SubMessage = "";
                    }

                    _liveTask = null;
                    foreach (var chartParam in ChartParams.ChartParams)
                    {
                        _ctsLiveData = new CancellationTokenSource();
                        Connection.Client.SubscribeToLive(DataCalcContext.GetLiveRequest(chartParam),
                            UpdateLiveByTicks);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        private async Task StopLiveData()
        {
            try
            {
                if (Connection.IsConnected)
                {
                    foreach (var chartParam in ChartParams.ChartParams)
                    {
                        Connection.Client.UnSubscribeFromLive(
                            DataCalcContext.GetLiveRequest(chartParam), UpdateLiveByTicks);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        private void UpdateLiveByTicks(string symbol, IEnumerable<ICandle> ticks)
        {
            var chartParam = ChartParams.ChartParams.FirstOrDefault();
            if (chartParam != null && chartParam.Symbol == symbol)
            {
                var arrTicks = ticks.ToArray();
                DataCalcContext.AddLiveTicks(arrTicks);

                if (ticks.Any())
                {
                    Chart.LastPrice = arrTicks.LastOrDefault().C;
                }

                if (_timeAndSaleWindow != null && _timeAndSaleWindow.IsVisible)
                {
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        _timeAndSaleViewModel.AddItems(arrTicks);

                    });

                }

                if (_liveTask == null)
                {
                    lock (_liveTaskLock)
                    {
                        if (_liveTask == null)
                        {
                            _liveTask = Task.Run(() =>


                                DataCalcContext.ProcessTicks(_ctsLiveData.Token)

                                , _ctsLiveData.Token


                            );
                        }
                    }
                }

                if (IsBusy)
                {
                    lock (_busyLock)
                    {
                        IsBusy = false;
                    }
                }
            }
        }


        private void HistoricalDataManagerRaiseMessageEvent(object sender, Core.Messages.MessageEventArgs e)
        {
            Message = e.Message;
            SubMessage = e.SubMessage;
        }

        private void SelectIndicator(object obj)
        {
            var selectIndicatorVm = new SelectScriptObjectViewModel(typeof(IndicatorBase), DataCalcContext)
            {
                AvailableObjectsHeader = "Available Indicators",
                ConfiguredObjectsHeader = "Configured Indicators",
            };
            selectIndicatorVm.ApplyCommand = new OperationCommand(o => { ApplyIndicators(selectIndicatorVm); });
            selectIndicatorVm.Init();
            var selectIndicatorWindow = new SelectScriptObjectWindow(selectIndicatorVm) { Title = "Indicators" };
            if (selectIndicatorWindow.ShowDialog() == true)
            {
                ApplyIndicators(selectIndicatorVm);
            }
            else
            {
                //TODO Cancel logic
            }
        }

        private void ApplyIndicators(SelectScriptObjectViewModel selectIndicatorVm)
        {
            DataCalcContext.CancelCalc();


            if (DataCalcContext.IsCalculatingState())
            {
                while (DataCalcContext.State != State.Finalized)
                {
                    Task.Delay(100);
                }
            }


            var indicators = selectIndicatorVm.ConfiguredObjects.ToList();
            DataCalcContext.SynchronizeIndicators(indicators);
            foreach (var indicator in indicators)
            {
                indicator.ViewModel = this;
            }

            DataCalcContext.CreateToken();
            Task.Run(() => ExecuteScript(), DataCalcContext.CancellationTokenSourceCalc.Token);
        }


        private void SelectStrategy(object obj)
        {
            var selectStrategyVm = new SelectScriptObjectViewModel(typeof(StrategyBase), DataCalcContext)
            {
                AvailableObjectsHeader = "Available Strategies",
                ConfiguredObjectsHeader = "Configured Strategies"
            };
            selectStrategyVm.ApplyCommand = new OperationCommand(o => { ApplyStrategies(selectStrategyVm); });
            selectStrategyVm.Init();
            var selectStrategyWindow = new SelectScriptObjectWindow(selectStrategyVm) { Title = "Strategies" };
            if (selectStrategyWindow.ShowDialog() == true)
            {
                ApplyStrategies(selectStrategyVm);
            }
            else
            {
                DataCalcContext.Strategies.ForAll(x => { x.IsTemporary = false; });

                //TODO Cancel logic
            }
        }

        private void ApplyStrategies(SelectScriptObjectViewModel selectStrategyVm)
        {
            DataCalcContext.CancelCalc();

            if (DataCalcContext.IsCalculatingState())
            {
                while (DataCalcContext.State != State.Finalized)
                {
                    Task.Delay(100);
                }
            }

            var strategies = selectStrategyVm.ConfiguredObjects.ToList();
            DataCalcContext.SynchronizeStrategies(strategies);
            Strategy = App.Strategies.GetStrategy(DataCalcContext);
            if (Strategy != null)
            {
                Strategy.Strategy.ViewModel = this;

                if (Strategy.Enabled)
                {
                    if (Strategy.State != State.SetDefaults)
                        Strategy.Stop();
                    Strategy.Start();
                }
                else
                {
                    Strategy.Stop();
                }
            }
        }

        private void DataBox(object obj)
        {
            if (_dataBoxWindow == null ||
                _dataBoxWindow.IsClosed)
            {
                _dataBoxWindow = new DataBoxWindow(this);
            }

            if (_dataBoxWindow.IsVisible)
            {
                _dataBoxWindow.Hide();
                IsDataBoxVisible = false;
            }
            else
            {
                _dataBoxWindow.Show();
                IsDataBoxVisible = true;
            }
        }

        private void TimeAndSale(object obj)
        {
            if (_timeAndSaleWindow == null ||
                _timeAndSaleWindow.IsClosed)
            {
                _timeAndSaleViewModel = new TimeAndSaleViewModel(this, Chart.TickSize);
                _timeAndSaleViewModel.Header = Header;                

                _timeAndSaleWindow = new TimeAndSaleWindow(_timeAndSaleViewModel);
            
            }

            if (_timeAndSaleWindow.IsVisible)
            {

                _timeAndSaleWindow.Hide();
                IsTimeAndSaleVisible = false;
                _timeAndSaleViewModel.Clear();
            }
            else
            {
                _timeAndSaleViewModel.Clear();
                _timeAndSaleWindow.Show();
                IsTimeAndSaleVisible = true;
            }
        }

        private void CrossLine(object obj) => _chart.IsCrossLinesVisible = !_chart.IsCrossLinesVisible;

        private void ChangeParam(object obj)
        {
            List<IRequest> oldChartParamRequests = new List<IRequest>();
            foreach (var chartParam in ChartParams.ChartParams)
            {
                oldChartParamRequests.Add(DataCalcContext.GetLiveRequest(chartParam));
            }

            var chartParamsWindow = new ChartParamsWindow(ChartParams);

            if (chartParamsWindow.ShowDialog() == true)
            {
                var chartParams = ChartParams.ChartParams.FirstOrDefault();
                if (chartParams != null)
                {
                    ChartTrader.Instrument = chartParams.Instrument;
                    Connection = Session.GetConnection(chartParams.Instrument.ConnectionId);
                }

                if (Connection.IsConnected)
                {
                    foreach (var request in oldChartParamRequests)
                    {
                        _ctsLiveData.Cancel();
                        Connection.Client.UnSubscribeFromLive(request, UpdateLiveByTicks);
                    }
                }

                FetchData();
            }

            OnPropertyChanged("Header");
        }

        private async Task ExecuteScript()
        {
            IsBusy = true;
            Message = "Calculating...";
            try
            {
                await DataCalcContext.Execute();
                await Chart.Dispatcher.InvokeAsync(() =>
                    {
                        Chart.ReCalc_VisibleCandlesExtremums();
                        Chart.ChartControl?.Invalidate();
                    }
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Execute script error");
                LogController.Print(ex.Message);
                LogController.Print(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    LogController.Print(ex.InnerException.StackTrace);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }


        private async void ReloadScript(object obj)
        {
            if (Strategy != null)
            {
                if (obj is bool isStrategyEnable)
                {
                    Strategy.Enabled = isStrategyEnable;
                }
                else
                {
                    BuildController.Instance.RecreateStrategies(DataCalcContext);
                }
            }
            else
            {
                await BuildController.Instance.RecreateIndicators(DataCalcContext);
            }
        }


        private void Performance(object obj)
        {
            Strategy?.TogglePerformance();
        }

        public void PerformanceLiveOrders(object obj)
        {
            Strategy?.TogglePerformance(StrategyPerformanceSection.Orders, TradeSourceType.RealTime);
        }

        public void PerformanceLiveTrades(object obj)
        {
            Strategy?.TogglePerformance(StrategyPerformanceSection.Trades, TradeSourceType.RealTime);
        }

        public void Destroy()
        {
            _dataBoxWindow?.Close();
            _timeAndSaleWindow?.Close();
            _ctsFetchData?.Cancel();
            _ctsLiveData?.Cancel();
            DataCalcContext.CancelCalc();
            if (Connection.IsConnected && ChartParams != null)
            {
                //So far, we only support the first chart
                var chartParam = ChartParams.ChartParams.FirstOrDefault();
                Connection.Client.UnSubscribeFromLive(
                    DataCalcContext.GetLiveRequest(chartParam), UpdateLiveByTicks);

                _liveTask = null;


            }

            Session.Instance.DataCalcContexts.Remove(DataCalcContext);

            if (Session.IsPlayback)
            {
                Session.Playback.Ticks.Clear();
                DataCalcContext.UnSubscribePlayback();
            }
            Session.Instance.OnConnectionStatusChanged -= Instance_OnConnectionStatusChanged;
            DataCalcContext.UnsubscribeConnectionStatusChanged();
            ChartTrader.Close();
            DrawingTools.Close();
            Chart?.Destroy();
        }



        public void ClearTrades(DateTime date)
        {
            if (Chart != null)
            {
                //Chart.BarTrades
                var tradeDrawIds = Chart.TradeDraws.Where(x => x.Value.EntryTime < date).Select(y => y.Key).ToList();

                foreach (var barTrade in Chart.BarTrades)
                {
                    barTrade.Value.RemoveAll(x => tradeDrawIds.Contains(x));
                }

                foreach (var tradeDrawId in tradeDrawIds)
                {
                    Chart.TradeDraws.Remove(tradeDrawId);
                }
            }
        }
    }
}