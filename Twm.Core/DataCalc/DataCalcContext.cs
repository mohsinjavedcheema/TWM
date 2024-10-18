using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Twm.Chart.Classes;
using Twm.Chart.Interfaces;
using Twm.Core.Classes;
using Twm.Core.DataCalc.DataSeries;
using Twm.Core.DataCalc.Performance;
using Twm.Core.DataProviders.Interfaces;
using Twm.Core.Enums;
using Twm.Core.Interfaces;
using Twm.Core.Managers;
using Twm.Model.Model;
using OperationCanceledException = System.OperationCanceledException;

namespace Twm.Core.DataCalc
{
    public class DataCalcContext : ICloneable
    {
        public IConnection Connection { get; set; }

        public Chart.Classes.Chart Chart { get; set; }

        private Dictionary<Type, IEnumerable<IndicatorBase>> _indicatorCache;

        public ObservableCollection<IndicatorBase> Indicators { get; set; }

        public ObservableCollection<StrategyBase> Strategies { get; set; }


        public DataCalcPeriodType DataCalcPeriodType { get; set; }

        private List<DataCalcObject> DataCalcObjects { get; set; }


        private readonly State[] _calculatingStates = {State.Historical, State.RealTime, State.Playback};


        private ObservableCollection<ICandle> _candles;

        public ObservableCollection<ICandle> Candles
        {
            get { return _candles; }
            set
            {
                if (_candles != value)
                {
                    _candles = value;
                    if (_candles != null)
                        BindingOperations.EnableCollectionSynchronization(Candles, _candleLockObject);
                }
            }
        }

        public ICandle LastCandle { get; set; }

        public int LastCandleIndex { get; set; }


        public Dictionary<string, IEnumerable<ICandle>> DataSeriesCash { get; set; }

        public List<ExtraDataSeries> ExtraDataSeries { get; set; }

        public Dictionary<string, List<ExtraDataSeries>> SymbolsExtraDataSeries { get; set; }

        public List<Instrument> Instruments { get; set; }


        public bool AllowLive { get; set; }

        public bool IsOptimization { get; set; }

        public bool IsValidation { get; set; }


        public CalcMode CalcMode { get; set; }


        public CancellationTokenSource CancellationTokenSourceCalc { get; set; }

        public bool IsCancelCalcRequest
        {
            get
            {
                if (CancellationTokenSourceCalc != null && CancellationTokenSourceCalc.IsCancellationRequested)
                {
                    return true;
                }

                return false;
            }
        }

        private State _state;

        public State State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                }
            }
        }


        private IList<DataSeriesParams> DataParams { get; set; }

        public DataSeriesParams CurrentDataSeriesParams { get; set; }


        public int CurrentBar;


        private string _name;

        public string Name
        {
            get
            {
                var dataSeriesName = "";
                if (DataParams != null && DataParams.Any())
                {
                    dataSeriesName = DataParams.FirstOrDefault()?.DataSeriesName;
                }


                if (!string.IsNullOrEmpty(_name))
                {
                    return _name + " " + dataSeriesName;
                }

                return dataSeriesName;
            }

            set
            {
                if (_name != value)
                {
                    _name = value;
                }
            }
        }


        public string DataSeries
        {
            get
            {
                if (CurrentDataSeriesParams == null)
                {
                    return string.Empty;
                }


                return "(" + CurrentDataSeriesParams.DataSeriesValue + " " + CurrentDataSeriesParams.DataSeriesType +
                       ")";
            }
        }

        public int HistoricalCandlesCount { get; set; }
        public bool IsSingleChart { get; set; }

        private readonly object _candleLockObject = new object();

        private readonly object _tickLockObject = new object();


        public DataCalcContext()
        {
            CreateCollections();
            CalcMode = CalcMode.Async;
            AllowLive = false;
        
        }

        public DataCalcContext(IConnection connection) : this()
        {
            Connection = connection;
        }


        private void OnConnectionStatusChanged(object sender, ConnectionStatusChangeEventArgs e)
        {
            var indicators = DataCalcObjects.ToList();
            var strategies = Strategies.ToList();

            foreach (var indicator in indicators)
            {
                indicator.OnConnectionStatusChanged(e.OldStatus, e.NewStatus, e.ConnectionName);
            }

            foreach (var strategy in strategies)
            {
                strategy.OnConnectionStatusChanged(e.OldStatus, e.NewStatus, e.ConnectionName);
            }
        }

        public void CreateCollections()
        {
            Indicators = new ObservableCollection<IndicatorBase>();
            Strategies = new ObservableCollection<StrategyBase>();
            DataCalcObjects = new List<DataCalcObject>();
            Instruments = new List<Instrument>();
            ExtraDataSeries = new List<ExtraDataSeries>();
            SymbolsExtraDataSeries = new Dictionary<string, List<ExtraDataSeries>>();

            _indicatorCache = new Dictionary<Type, IEnumerable<IndicatorBase>>();
            _liveSemaphores = new Dictionary<string, SemaphoreSlim>();
        }


        public void SetParams(List<DataSeriesParams> dataParams)
        {
            DataParams = dataParams;
            CurrentDataSeriesParams = DataParams.FirstOrDefault();
            if (CurrentDataSeriesParams != null)
            {
                Instruments.Clear();
                Instruments.Add(CurrentDataSeriesParams.Instrument);
            }
        }


        public void SetParams(DataSeriesParams dataSeriesParams)
        {
            DataParams = new List<DataSeriesParams>() {dataSeriesParams};
            CurrentDataSeriesParams = dataSeriesParams;
            if (CurrentDataSeriesParams != null)
            {
                Instruments.Clear();
                Instruments.Add(CurrentDataSeriesParams.Instrument);
            }
        }

        public DataSeriesParams GetParams()
        {
            return CurrentDataSeriesParams;
        }


        public void CancelCalc()
        {
            CancellationTokenSourceCalc?.Cancel();
        }

        public async Task Execute()
        {
            ClearCalcObjects();

            if (CancellationTokenSourceCalc == null || CancellationTokenSourceCalc.IsCancellationRequested)
                CancellationTokenSourceCalc = new CancellationTokenSource();


            State = State.Undefined;
            NotifyStateChanged(State);

            if (!Indicators.Any())
                return;

            ProcessInput();

            State = State.Configured;
            NotifyStateChanged(State);

            State = State.Historical;
            NotifyStateChanged(State);

            try
            {
                var indicators = Indicators.ToArray();
                var candlesCount = Candles.Count + 1;
                var indicatorsCount = Indicators.Count;
                var isLive = false;
                if (AllowLive && Connection.IsConnected)
                {
                    lock (_tickLockObject)
                    {
                        _barClosedEvent = new ManualResetEventSlim(false);
                    }

                    CurrentBar = 1;
                    while (true)
                    {
                        if (CancellationTokenSourceCalc.IsCancellationRequested)
                            return;

                        for (int j = 0; j < indicatorsCount; j++)
                        {
                            //if (indicators[j].LastBarIndex != CurrentBar)
                            {
                                indicators[j].OnBarUpdate();
                                indicators[j].OnAfterBarUpdate();
                            }
                        }


                        if (CurrentBar < Candles.Count - 1)
                        {
                            //Historical data
                            CurrentBar++;
                        }
                        else
                        {
                            //Live
                            if (!isLive)
                            {
                                Chart.Dispatcher.InvokeAsync(() =>
                                    {
                                        var indicatorBase = indicators.FirstOrDefault(x => x.ViewModel != null);
                                        Chart.ReCalc_VisibleCandlesExtremums();
                                        Chart.ChartControl.Invalidate();
                                        if (indicatorBase != null)
                                            indicatorBase.ViewModel.IsBusy = false;
                                    }
                                );
                                isLive = true;
                            }

                            if (CurrentBar == Candles.Count || !Candles[CurrentBar].IsClosed)
                            {
                                _barClosedEvent?.Wait(CancellationTokenSourceCalc.Token);
                                lock (_tickLockObject)
                                {
                                    _barClosedEvent.Reset();
                                }
                            }

                            CurrentBar++;
                        }
                    }
                }
                else
                {
                    for (CurrentBar = 1; CurrentBar < candlesCount; CurrentBar++)
                    {
                        if (CancellationTokenSourceCalc.IsCancellationRequested)
                            return;

                        for (int j = 0; j < indicatorsCount; j++)
                        {
                            //if (indicators[j].LastBarIndex != CurrentBar)
                            {
                                indicators[j].OnBarUpdate();
                                indicators[j].OnAfterBarUpdate();
                            }
                        }
                    }

                    State = State.Finalized;
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                if (ex.InnerException is OperationCanceledException)
                    return;

                if (CancellationTokenSourceCalc.Token.IsCancellationRequested)
                    return;


                throw new Exception("Indicator calc exception", ex);
            }
            finally
            {
                if (CancellationTokenSourceCalc.IsCancellationRequested)
                {
                    State = State.Finalized;
                }
            }


            NotifyStateChanged(State);
        }


        private Barrier _iterationBarrier;

        private ManualResetEventSlim _barClosedEvent;

        public bool IsCalculatingState()
        {
            return _calculatingStates.Contains(State);
        }


        public async Task ExecuteStrategy(CancellationTokenSource cancellationTokenSource)
        {
            HistoricalCandlesCount = Candles.Count;

            ClearCalcObjects();

            if (cancellationTokenSource != null)
                CancellationTokenSourceCalc = cancellationTokenSource;
            else
            {
                if (CancellationTokenSourceCalc == null)
                    CreateToken();
                cancellationTokenSource = CancellationTokenSourceCalc;
            }

            cancellationTokenSource.Token.ThrowIfCancellationRequested();

            State = State.Undefined;
            NotifyStateChanged(State);

            ProcessInput();

            State = State.Configured;
            NotifyStateChanged(State);

            State = State.Historical;
            NotifyStateChanged(State);


            cancellationTokenSource.Token.ThrowIfCancellationRequested();

            try
            {
                var indicators = DataCalcObjects.ToList();
                indicators.ForEach(x => x.IsOptimization = IsOptimization);
                var strategyBase = Strategies.FirstOrDefault();
                if (strategyBase != null)
                {
                    strategyBase.IsOptimization = IsOptimization;
                    strategyBase.SystemPerformance = new SystemPerformance(new[] {strategyBase});                   
                }

                cancellationTokenSource.Token.ThrowIfCancellationRequested();
                var candlesCount = Candles.Count + 1;
                var indicatorsCount = indicators.Count;
                var isLive = false;
                if (AllowLive && Connection.IsConnected)
                {
                    lock (_tickLockObject)
                    {
                        _barClosedEvent = new ManualResetEventSlim(false);
                    }

                    CurrentBar = 1;
                    while (true)
                    {
                        if (CancellationTokenSourceCalc.IsCancellationRequested)
                            return;

                        for (int j = 0; j < indicatorsCount; j++)
                        {
                            if (indicators[j].LastBarIndex != CurrentBar)
                            {
                                indicators[j].OnBarUpdate();
                                indicators[j].OnAfterBarUpdate();
                            }
                        }

                        strategyBase.BeforeBarUpdate();
                        strategyBase.CalculateMaeMfe();
                        strategyBase.OnBarUpdate();
                        strategyBase.OnAfterBarUpdate();

                        if (CurrentBar < Candles.Count - 1)
                        {
                            //Historical data
                            CurrentBar++;
                        }
                        else
                        {
                            //Live
                            if (!isLive)
                            {
                                Chart.Dispatcher.InvokeAsync(() =>
                                    {
                                        foreach (var indicator in indicators)
                                        {
                                            if (indicator is IndicatorBase indicatorBase && indicatorBase.ViewModel != null)
                                            {
                                                Chart.ReCalc_VisibleCandlesExtremums();
                                                Chart.ChartControl.Invalidate();
                                                if (indicatorBase != null)
                                                    indicatorBase.ViewModel.IsBusy = false;
                                                break;
                                            }
                                            
                                        }
                                        if (strategyBase.ViewModel != null)
                                            strategyBase.ViewModel.IsBusy = false;


                                    }
                                );
                                isLive = true;
                            }

                            Chart.Dispatcher.InvokeAsync(() => Chart.ReCalc_VisibleCandlesExtremums());

                            if (CurrentBar == Candles.Count || !Candles[CurrentBar].IsClosed)
                            {
                                _barClosedEvent?.Wait(CancellationTokenSourceCalc.Token);
                                lock (_tickLockObject)
                                {
                                    _barClosedEvent.Reset();
                                }
                            }

                            CurrentBar++;
                        }
                    }
                }
                else
                {
                    for (CurrentBar = 1; CurrentBar < candlesCount; CurrentBar++)
                    {
                        if (CancellationTokenSourceCalc.IsCancellationRequested)
                            return;

                        for (int j = 0; j < indicatorsCount; j++)
                        {
                            if (indicators[j].LastBarIndex != CurrentBar)
                            {
                                indicators[j].OnBarUpdate();
                                indicators[j].OnAfterBarUpdate();
                            }
                        }

                        strategyBase.BeforeBarUpdate();
                        strategyBase.CalculateMaeMfe();
                        strategyBase.OnBarUpdate();
                        strategyBase.OnAfterBarUpdate();
                    }

                    State = State.Finalized;
                    NotifyStateChanged(State);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                if (ex.InnerException is OperationCanceledException)
                    return;

                if (CancellationTokenSourceCalc.Token.IsCancellationRequested)
                    return;


                throw new Exception("Strategy calc exception", ex);
            }
            finally
            {
                if (CancellationTokenSourceCalc.IsCancellationRequested)
                {
                    State = State.Finalized;
                    NotifyStateChanged(State);
                }
            }
        }

        public async Task OptimizeStrategy(StrategyBase strategy)
        {
            //await Task.Run( async () =>  await strategy.Optimizer.OnOptimize(), strategy.Optimizer.CancellationToken);

            await strategy.Optimizer.OnOptimize();
            await Task.WhenAll(strategy.Optimizer.OptimizationTasks.ToArray());
        }


        private void ProcessInput()
        {
            foreach (var indicator in Indicators)
            {
                var input = indicator.Input;
                while (input is IndicatorBase ib)
                {
                    ib.IsTemporary = false;
                    ib.SetDataCalcContext(this);
                    RegisterFirst(ib);
                    input = ib.Input;
                }
            }
        }


        private void ClearCalcObjects()
        {
            _indicatorCache.Clear();
            if (Indicators != null)
                foreach (var indicator in Indicators)
                {
                    indicator.Reset();
                }

            if (Strategies != null)
                foreach (var strategy in Strategies)
                {
                    strategy.Reset();
                }


            if (DataCalcObjects != null)
                foreach (var dataCalcObject in DataCalcObjects)
                {
                    if (dataCalcObject is ScriptBase script)
                    {
                        script.Clear();
                    }
                }

            DataCalcObjects.Clear();

            Chart?.Dispatcher.Invoke(() => Chart?.Refresh());

            ExtraDataSeries.Clear();
            SymbolsExtraDataSeries.Clear();

            _liveSemaphores.Clear();
        }


        public void ClearStrategyObjects(StrategyBase strategy)
        {
            _indicatorCache.Clear();

            strategy.Reset();

            foreach (var dataCalcObject in DataCalcObjects)
            {
                if (dataCalcObject is ScriptBase script)
                {
                    script.Clear();
                }
            }

            DataCalcObjects.Clear();

            Chart?.Dispatcher.Invoke(() => Chart?.Refresh());
        }


        public void NotifyStateChanged(State state)
        {
            if (state == State.Configured)
            {
                var objectCount = 0;

                foreach (var indicator in Indicators)
                {
                    indicator.ClearLocalVariables();
                    indicator.ChangeState();
                }

                foreach (var strategy in Strategies)
                {
                    if (strategy.Enabled)
                    {
                        strategy.ClearLocalVariables();
                        strategy.BeforeChangeState(state);
                        strategy.ChangeState();
                    }
                }

                while (objectCount != DataCalcObjects.Count)
                {
                    objectCount = DataCalcObjects.Count;
                    var objectsForChangeState = DataCalcObjects.ToList();
                    foreach (var dataCalcObject in objectsForChangeState)
                    {
                        dataCalcObject.ChangeState();
                    }
                }
            }
            else if (state == State.Finalized)
            {
                //Finalize only objects created on script execution
                foreach (var dataCalcObject in DataCalcObjects)
                {
                    dataCalcObject.ChangeState();
                }
            }
            else
            {
                foreach (var dataCalcObject in DataCalcObjects)
                {
                    if (dataCalcObject is IndicatorBase ib && ib.Input is IndicatorBase inputIb)
                    {
                        if (inputIb.GetDataCalcContext() == null)
                            inputIb.SetDataCalcContext(this);
                        inputIb.ChangeState();
                    }

                    dataCalcObject.ChangeState();
                }

                foreach (var indicator in Indicators)
                {
                    if (indicator.Input is IndicatorBase ib)
                    {
                        if (ib.GetDataCalcContext() == null)
                            ib.SetDataCalcContext(this);
                        ib.ChangeState();
                    }

                    indicator.ChangeState();
                }

                foreach (var strategy in Strategies)
                {
                    strategy.BeforeChangeState(state);
                    strategy.ChangeState();
                }
            }
        }

        public EventHandler<TickUpdateEventArgs> TickUpdate;


        public void NotifyTickUpdate(ICandle candle, ICandle tick)
        {

            CurrentTickNo++;

            Session.Instance.GetMockup(Connection)
                .OnTickUpdate(candle, tick, CurrentDataSeriesParams.Instrument.Symbol);

            foreach (var indicator in Indicators)
            {
                indicator.OnTickUpdate(candle, tick);
            }

            foreach (var strategy in Strategies)
            {
                if (strategy.Enabled)
                {
                    strategy.OnTickUpdate(candle, tick);
                    strategy.CalculateLivePositionStatistics(tick);
                }
            }
        }


        public void RegisterFirst(IndicatorBase indicatorBase)
        {
            DataCalcObjects.Insert(0, indicatorBase);
        }

        public void RegisterLast(IndicatorBase indicatorBase)
        {
            DataCalcObjects.Add(indicatorBase);
        }


        public IndicatorBase CreateIndicator(Type indicatorType, ISeries<double> input = null, bool isTemporary = false,
            ScriptOptions options = null)
        {
            dynamic indicator = Activator.CreateInstance(indicatorType);
            var indicatorBase = (IndicatorBase) indicator;
            indicatorBase.IsTemporary = isTemporary;
            indicatorBase.SetDataCalcContext(this);
            indicatorBase.SetInput(input);
            indicatorBase.IsOptimization = IsOptimization;
            if (!isTemporary)
            {
                RegisterLast(indicatorBase);
                AddToCache(indicatorType, indicatorBase);
            }

            if (options != null)
                indicatorBase.Options = options;
            indicatorBase.SetState(State.SetDefaults);
            indicatorBase.SetTwmProperties();
            return indicatorBase;
        }

        public T CreateIndicator<T>(ISeries<double> input = null, bool isTemporary = false,
            ScriptOptions options = null) where T : IndicatorBase
        {
            return (T) CreateIndicator(typeof(T), input, isTemporary, options);
        }


        public StrategyBase CreateStrategy(Type strategyType, ISeries<double> input = null, bool isTemporary = false)
        {
            dynamic strategy = Activator.CreateInstance(strategyType);
            var strategyBase = (StrategyBase) strategy;
            strategyBase.IsTemporary = isTemporary;
            strategyBase.SetDataCalcContext(this);
            strategyBase.SetInput(input);
            strategyBase.CreateCommission();
 
            strategyBase.IsOptimization = IsOptimization;
            if (!isTemporary)
            {
                // RegisterLast(indicatorBase);
                // AddToCache(indicatorType, indicatorBase);
            }

            strategyBase.SetState(State.SetDefaults);
            strategyBase.SetTwmProperties();
            return strategyBase;
        }

/*

        /// <summary>
        /// Set optimizer to strategy
        /// </summary>
        /// <param name="strategy"></param>
        public void SetOptimizer(StrategyBase strategy)
        {
            if (strategy.OptimizerType != null)
            {
                dynamic optimizer = Activator.CreateInstance(strategy.OptimizerType.ObjectType);
                strategy.Optimizer = optimizer;
                strategy.Optimizer.Init(strategy);
            }
        }*/


        public T CreateStrategy<T>(ISeries<double> input = null, bool isTemporary = false) where T : StrategyBase
        {
            return (T) CreateStrategy(typeof(T), input, isTemporary);
        }

        public ScriptBase CreateObject(Type objectType, ISeries<double> input = null, bool isTemporary = false)
        {
            if (objectType.IsSubclassOf(typeof(IndicatorBase)))
            {
                return CreateIndicator(objectType, input, isTemporary);
            }

            if (objectType.IsSubclassOf(typeof(StrategyBase)))
            {
                return CreateStrategy(objectType, input, isTemporary);
            }

            return null;
        }


        private void AddToCache(Type indicatorType, IndicatorBase indicator)
        {
            if (!_indicatorCache.TryGetValue(indicatorType, out var cache))
            {
                _indicatorCache.Add(indicatorType, new List<IndicatorBase>() {indicator});
            }
            else
            {
                _indicatorCache[indicatorType] = cache.Append(indicator);
            }
        }


        public IEnumerable<T> GetIndicatorCache<T>()
        {
            if (_indicatorCache.TryGetValue(typeof(T), out var cache))
            {
                return cache.OfType<T>();
            }

            return Enumerable.Empty<T>();
        }


        public void SynchronizeIndicators(List<ScriptBase> configuredObjects)
        {
            foreach (var scriptObject in configuredObjects)
            {
                //Add new indicators
                if (!Indicators.Contains(scriptObject))
                {
                    scriptObject.IsTemporary = false;

                    Indicators.Add(scriptObject as IndicatorBase);
                    scriptObject.SetChart(Chart);

                    if (Chart != null)
                    {
                        scriptObject.SynchronizeChart();
                    }
                }
            }

            var dataCalcContextIndicators = Indicators.ToArray();
            foreach (var indicator in dataCalcContextIndicators)
            {
                //Remove old indicators
                if (!configuredObjects.Contains(indicator))
                {
                    Indicators.Remove(indicator);
                    indicator.Clear();
                }
            }

            Chart?.Refresh();
        }


        public void SynchronizeStrategies(List<ScriptBase> configuredObjects)
        {
            foreach (var scriptObject in configuredObjects)
            {
                //Add new strategy
                if (!Strategies.Contains(scriptObject))
                {
                    scriptObject.IsTemporary = false;
                    var strategy = scriptObject as StrategyBase;

                    Strategies.Add(strategy);
                    Session.Instance.Strategies.Add(strategy);

                    scriptObject.SetChart(Chart);

                    if (Chart != null)
                    {
                        scriptObject.SynchronizeChart();
                    }
                }
            }

            var dataCalcContextStrategies = Strategies.ToArray();
            foreach (var strategy in dataCalcContextStrategies)
            {
                //Remove old strategy
                if (!configuredObjects.Contains(strategy))
                {
                    Strategies.Remove(strategy);
                    Session.Instance.Strategies.Remove(strategy);
                    strategy.Clear();
                }

                strategy.IsTemporary = false;
            }

            Chart?.Refresh();
        }

        public void CalculateObject(DataCalcObject dataCalcObject, int fromBar = 1)
        {
            var currentBar = CurrentBar;
            CurrentBar = fromBar;
            for (int i = fromBar; i < currentBar + 1; i++)
            {
                //if (dataCalcObject.LastBarIndex != CurrentBar)
                {
                    dataCalcObject.OnBarUpdate();
                    dataCalcObject.OnAfterBarUpdate();
                }

                CurrentBar++;
            }

            CurrentBar = currentBar;
        }

        public object Clone()
        {
            var obj = (DataCalcContext) MemberwiseClone();
            obj.CreateCollections();
            return obj;
        }

        public void CreateToken()
        {
            CancellationTokenSourceCalc = new CancellationTokenSource();
        }

        public IRequest GetLiveRequest(DataSeriesParams dataSeriesParams)
        {
            return Connection.Client.GetLiveDataRequest(dataSeriesParams);
        }


        public IRequest GetDepthRequest(DataSeriesParams dataSeriesParams)
        {
            return Connection.Client.GetDepthDataRequest(dataSeriesParams);
        }





        private SemaphoreSlim _liveSemaphore = new SemaphoreSlim(1, 1);


        private Queue<IEnumerable<ICandle>> _liveTicks = new Queue<IEnumerable<ICandle>>();


        private object liveLock = new object();

        public void AddLiveTicks(IEnumerable<ICandle> ticks)
        {
            lock (liveLock)
            {
                if (ticks != null)
                {
                    _liveTicks.Enqueue(ticks);
                    Monitor.Pulse(liveLock);
                }
            }
        }


        public long CurrentTickNo { get; set; }
        public async Task ProcessTicks(CancellationToken cancellationToken)
        {
            CurrentTickNo = 0;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                IEnumerable<ICandle> ticks;
                lock (liveLock)
                {
                    while (_liveTicks.Count == 0)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return;

                        Monitor.Wait(liveLock);
                    }

                    ticks = _liveTicks.Dequeue();
                }


                if (ticks == null)
                {
                    continue;
                }


                if (State == State.Historical && CurrentBar >= HistoricalCandlesCount - 1)
                {
                    State = Session.Instance.IsPlayback ? State.Playback : State.RealTime;
                    NotifyStateChanged(State);
                }

                var candle = Candles.LastOrDefault();

                if (candle == null)
                    return;

                var index = Candles.Count - 1;

                var ssTicks = new SemaphoreSlim(1, 1);

                foreach (var tick in ticks)
                {
                    await ssTicks.WaitAsync(cancellationToken);
                    //Tick hit to currentBar
                    if (candle.ct > tick.t && candle.t <= tick.t)
                    {
                        if (candle.IsClosed)
                        {
                            ssTicks.Release(1);
                            continue;
                        }

                        candle.C = tick.C;

                        if (tick.IsAggVolume)
                            candle.V = tick.V;
                        else
                            candle.V += tick.V;

                        if (tick.H > candle.H)
                            candle.H = tick.H;
                        if (tick.L < candle.L)
                            candle.L = tick.L;

                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            ChangeCandle(index, candle);
                            if (LastCandle == null)
                                LastCandle = candle;
                            NotifyTickUpdate(candle, tick);
                            ssTicks.Release(1);
                            //Debug.WriteLine("Change candle: " + candle + " by tick " + tick);
                        });
                    }
                    //Tick hit to new Bar
                    else if (candle.ct <= tick.t)
                    {
                        var startTime = candle.ct;
                        var endTime = HistoricalDataManager.CalcCloseTime(CurrentDataSeriesParams, startTime);
                        while (tick.t >= endTime)
                        {
                            startTime = endTime;
                            endTime = HistoricalDataManager.CalcCloseTime(CurrentDataSeriesParams, endTime);
                        }

                        var isClosed = false;
                        if (!candle.IsClosed)
                        {
                            candle.IsClosed = true;
                            isClosed = true;
                        }

                        candle = new Candle(startTime, tick.O, tick.H, tick.L, tick.C, tick.V, false)
                            {ct = endTime};

                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            InsertCandle(ref index, candle);
                            tick.IsFirstTickOfBar = true;
                            NotifyTickUpdate(candle, tick);
                            if (isClosed)
                                _barClosedEvent?.Set();
                            ssTicks.Release(1);
                            //  Debug.WriteLine("Insert new candle: " + candle);
                        });
                    }
                    else
                    {
                        ssTicks.Release(1);
                    }
                }

                await ssTicks.WaitAsync(cancellationToken);
            }
        }

        public async Task ProcessHistoricalTicks(IEnumerable<ICandle> ticks)
        {
            var tokenSource = new CancellationTokenSource();
            await _liveSemaphore.WaitAsync(tokenSource.Token);

            try
            {
                if (!ticks.Any())
                    return;

                List<ICandle> candlesToAdd = new List<ICandle>();

                var candle = Candles.LastOrDefault();

                if (candle == null)
                    return;

                //var index = Candles.Count - 1;

                var index = -1;

                var ssTicks = new SemaphoreSlim(1, 1);
                foreach (var tick in ticks)
                {
                    await ssTicks.WaitAsync(tokenSource.Token);
                    //Tick hit to currentBar
                    if (candle.ct > tick.t && candle.t <= tick.t)
                    {
                        if (candle.IsClosed)
                        {
                            ssTicks.Release(1);
                            continue;
                        }

                        candle.C = tick.C;
                        candle.V += tick.V;
                        if (tick.H > candle.H)
                            candle.H = tick.H;
                        if (tick.L < candle.L)
                            candle.L = tick.L;

                        candlesToAdd[index] = candle;

                        ssTicks.Release(1);
                    }
                    //Tick hit to new Bar
                    else if (candle.ct <= tick.t)
                    {
                        var startTime = candle.ct;
                        var endTime = HistoricalDataManager.CalcCloseTime(CurrentDataSeriesParams, startTime);
                        while (tick.t >= endTime)
                        {
                            startTime = endTime;
                            endTime = HistoricalDataManager.CalcCloseTime(CurrentDataSeriesParams, endTime);
                        }

                        if (!candle.IsClosed)
                        {
                            candle.IsClosed = true;
                        }

                        candle = new Candle(startTime, tick.O, tick.H, tick.L, tick.C, tick.V, false) {ct = endTime};

                        index++;
                        candlesToAdd.Insert(index, candle);
                        ssTicks.Release(1);
                    }
                    else
                    {
                        ssTicks.Release(1);
                    }
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var candleToAdd in candlesToAdd)
                    {
                        Candles.Add(candleToAdd);
                    }
                });

                await ssTicks.WaitAsync(tokenSource.Token);
            }
            finally
            {
                _liveSemaphore.Release(1);
            }
        }

        private Dictionary<string, SemaphoreSlim> _liveSemaphores;

        public async void ProcessExtraTicks(string symbol, IEnumerable<ICandle> ticks)
        {
            try
            {
                if (!_liveSemaphores.TryGetValue(symbol, out var semaphoreSlim))
                {
                    semaphoreSlim = new SemaphoreSlim(1, 1);
                }


                await semaphoreSlim.WaitAsync(CancellationTokenSourceCalc.Token);
                try
                {
                    if (SymbolsExtraDataSeries.TryGetValue(symbol, out var extraDataSeriesList))
                    {
                        var tickList = ticks.ToList();

                        foreach (var extraDataSeries in extraDataSeriesList)
                        {
                            var candle = extraDataSeries.Candles.LastOrDefault();

                            if (candle == null)
                                continue;

                            var index = extraDataSeries.Candles.Count - 1;

                            var ssTicks = new SemaphoreSlim(1, 1);
                            foreach (var tick in tickList)
                            {
                                await ssTicks.WaitAsync(CancellationTokenSourceCalc.Token);
                                //Tick hit to currentBar
                                if (candle.ct > tick.t && candle.t <= tick.t)
                                {
                                    if (candle.IsClosed)
                                    {
                                        ssTicks.Release(1);
                                        continue;
                                    }

                                    candle.C = tick.C;
                                    candle.V += tick.V;
                                    if (tick.H > candle.H)
                                        candle.H = tick.H;
                                    if (tick.L < candle.L)
                                        candle.L = tick.L;

                                    await Application.Current.Dispatcher.InvokeAsync(() =>
                                    {
                                        extraDataSeries.Candles[index] = candle;
                                        ssTicks.Release(1);
                                    });
                                }
                                //Tick hit to new Bar
                                else if (candle.ct <= tick.t)
                                {
                                    var startTime = candle.ct;
                                    var endTime = HistoricalDataManager.CalcCloseTime(CurrentDataSeriesParams, startTime);
                                    while (tick.t >= endTime)
                                    {
                                        startTime = endTime;
                                        endTime = HistoricalDataManager.CalcCloseTime(CurrentDataSeriesParams, endTime);
                                    }

                                    if (!candle.IsClosed)
                                    {
                                        candle.IsClosed = true;
                                    }

                                    candle = new Candle(startTime, tick.O, tick.H, tick.L, tick.C, tick.V, false)
                                    { ct = endTime };

                                    await Application.Current.Dispatcher.InvokeAsync(() =>
                                    {
                                        index++;
                                        extraDataSeries.Candles.Insert(index, candle);
                                        ssTicks.Release(1);
                                    });
                                }
                                else
                                {
                                    ssTicks.Release(1);
                                }
                            }

                            await ssTicks.WaitAsync(CancellationTokenSourceCalc.Token);
                        }
                    }
                }
                finally
                {
                    semaphoreSlim.Release(1);
                }
            }
            catch (OperationCanceledException oce) { }
            catch(Exception ex)
            {
                throw new Exception("Process extra tick exception");
            }
        }

        public ICandle GetCandle(DateTime candleTime, out int index, out bool isLast, out bool isPrev)
        {
            lock (_candleLockObject)
            {
                index = -1;
                isLast = false;
                isPrev = false;
                if (LastCandle != null && LastCandle.t == candleTime)
                {
                    index = LastCandleIndex;
                    isLast = true;
                    isPrev = false;
                    return LastCandle;
                }

                var candle = Candles.LastOrDefault(x => x.t == candleTime);

                if (candle != null)
                {
                    isPrev = false;
                    index = Candles.IndexOf(candle);
                    isLast = Candles.Count - 1 == index;
                    return candle;
                }

                candle = Candles.LastOrDefault(x => x.t < candleTime);

                if (candle != null)
                {
                    isPrev = true;
                    index = Candles.IndexOf(candle);
                    isLast = Candles.Count - 1 == index;
                }


                return candle;
            }
        }

        public void ChangeCandle(int candleIndex, ICandle candle)
        {
            Candles[candleIndex] = candle;
        }

        public void InsertCandle(ref int candleIndex, ICandle candle)
        {
            candleIndex++;
            Candles.Insert(candleIndex, candle);

            if (Candles.Count - 1 == candleIndex)
            {
                LastCandle = candle;
                LastCandleIndex = candleIndex;
            }
        }


        public void AddExtraDataSeries(string symbol, ExtraDataSeries dataSeries)
        {
            ExtraDataSeries.Add(dataSeries);
            if (SymbolsExtraDataSeries.TryGetValue(symbol, out var extraDataSeries))
            {
                extraDataSeries.Add(dataSeries);
            }
            else
            {
                SymbolsExtraDataSeries.Add(symbol, new List<ExtraDataSeries>() {dataSeries});
            }
        }

        public async void SetPlaybackTicks(IEnumerable<ICandle> ticks, bool isReload = true)
        {
            Chart.NotRender = true;
            var candlesForRemove = Candles.Where(x => x.t >= Session.Instance.Playback.PeriodStart).ToList();
            foreach (var candle in candlesForRemove)
            {
                Candles.Remove(candle);
            }

            await ProcessHistoricalTicks(ticks);

            Chart.NotRender = false;
            if (isReload)
                Chart.Reload();
        }

        public void SubscribePlayback()
        {
            Session.Instance.Playback.OnReset += Playback_OnReset;
        }

        public void UnSubscribePlayback()
        {
            Session.Instance.Playback.OnReset -= Playback_OnReset;
        }

        public void SubscribeConnectionStatusChanged()
        {
            Session.Instance.OnConnectionStatusChanged += OnConnectionStatusChanged;
        }

        public void UnsubscribeConnectionStatusChanged()
        {
            Session.Instance.OnConnectionStatusChanged -= OnConnectionStatusChanged;
        }

        

        private void Playback_OnReset(object sender, DataProviders.Playback.PlaybackEventArgs e)
        {
            foreach (var strategy in Strategies)
            {
                strategy.Enabled = false;
            }

            SetPlaybackTicks(e.Ticks);
        }

        public void Destroy()
        {
            ClearCalcObjects();
            Indicators?.Clear();
            Indicators = null;
            Strategies?.Clear();
            Strategies = null;
            Candles?.Clear();
            Candles = null;
            Chart = null;
        }
    }
}