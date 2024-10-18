using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Twm.Core.Attributes;
using Twm.Core.Converters;
using Twm.Core.DataCalc.Performance;
using Twm.Core.Enums;
using Newtonsoft.Json;

namespace Twm.Core.DataCalc.Optimization
{
    [DataContract]
    [JsonConverter(typeof(OptimizerJsonConverter))]
    public abstract class Optimizer : DataCalcObject, ICloneable
    {
        [DataMember]
        [Browsable(false)]
        public string TypeName
        {
            get { return this.GetType().Name; }
        }

        [Browsable(false)]
        public CancellationTokenSource CancellationTokenSource { get; set; }

        [Browsable(false)]
        public ObservableCollection<StrategyBase> Strategies { get; set; }

        [Visible(false, PropertyVisibility.Optimizer )]
        public ObservableCollection<OptimizerParameter> OptimizerParameters { get; set; }

        [Browsable(false)]
        public StrategyBase Strategy { get; set; }

        [Browsable(false)]
        public ObservableCollection<Task> OptimizationTasks { get; private set; }

        [Browsable(false)]
        public Task LastTask { get; private set; }

        [Browsable(false)]
        public StrategyBase LastStrategy { get; set; }

        [Browsable(false)]
        public object CustomValue { get; set; }

        [Browsable(false)]
        public event EventHandler OnTaskCompleted;

        private List<OptimizerParameter> _checkedParameters;

        private StrategyBase _worstTopStrategy;

        private Queue<Dictionary<string, object>> _tvmParamsQueue;

        private int _strategiesCount;
        private List<StrategyBase> StrategyList { get; set; }

        private int _removedStrategiesByMinTrades;

        [Browsable(false)]
        public int RemovedStrategiesByMinTrades
        {
            get { return _removedStrategiesByMinTrades; }
            set
            {
                if (_removedStrategiesByMinTrades != value)
                {
                    _removedStrategiesByMinTrades = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _removedStrategiesByDrawDown;

        [Browsable(false)]
        public int RemovedStrategiesByDrawDown
        {
            get { return _removedStrategiesByDrawDown; }
            set
            {
                if (_removedStrategiesByDrawDown != value)
                {
                    _removedStrategiesByDrawDown = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _allStrategies;
        [Browsable(false)]
        public int AllStrategies
        {
            get { return _allStrategies; }
            set
            {
                if (_allStrategies != value)
                {
                    _allStrategies = value;
                    OnPropertyChanged();
                }
            }
        }
        

        [Browsable(false)] 
        public virtual long CombinationCount
        {
            set { }
            get
            { 
                return DefaultCombinationCount;
            }
        }


        [Browsable(false)]
        public  long DefaultCombinationCount
        {
            set { }
            get
            {
                _checkedParameters = OptimizerParameters.Where(x => x.IsChecked).ToList();
                long allCount = 1;
                foreach (var parameter in _checkedParameters)
                {
                    var count = parameter.CombinationCount;
                    if (count > 0)
                    {
                        allCount = allCount * count;
                    }
                }

                return allCount;
            }
        }


        private object _observableCollectionLock;
        private object _lockObject;


        [DataMember]
        [Browsable(false)]
        public int MinTrades { get; set; }

        [DataMember]
        [Browsable(false)]
        public double DrawDownLevel { get; set; }

        [DataMember]
        [Browsable(false)]
        public int KeepBestNumber { get; set; }

        [DataMember]
        [Browsable(false)]
        public int TaskBatchSize { get; set; }

        [Browsable(false)]
        public TimeSpan RemainingTime { get; set; }

        [Browsable(false)]
        public long AvgTaskTime { get; set; }
        

        private long _allTaskTime;


        public void Init(StrategyBase strategy, ObservableCollection<OptimizerParameter>  parameters = null)
        {
            MinTrades = 0;

            DrawDownLevel = 0;

            RemovedStrategiesByMinTrades = 0;

            RemovedStrategiesByDrawDown = 0;

            KeepBestNumber = 10;

            TaskBatchSize = 10;

            RemainingTime = new TimeSpan(0, 0, 0);

            _concurrencySemaphore = new SemaphoreSlim(TaskBatchSize);

            OptimizerParameters = new ObservableCollection<OptimizerParameter>();

            Strategy = strategy;

            CreateParams(parameters);
        }

        public void CreateParams(ObservableCollection<OptimizerParameter> parameters = null)
        {
            OptimizerParameters.Clear();

            if (parameters == null)
            {
                if (Strategy.TwmPropertyTypes != null)
                {
                    foreach (var propertyType in Strategy.TwmPropertyTypes)
                    {
                        var type = propertyType.Value.Type;
                        var displayName = propertyType.Value.DisplayName;
                        var value = Strategy.GetTwmPropertyValue(propertyType.Key);

                        OptimizerParameter parameter = null;
                        if (type == typeof(int))
                        {
                            int defaultValue = (int?) value ?? 0;
                            parameter = new IntegerOptimizerParameter(propertyType.Key, type, defaultValue)
                                {DisplayName = displayName};
                        }
                        else if (type == typeof(double))
                        {
                            double defaultValue = (double?) value ?? 0.0;
                            parameter = new DoubleOptimizerParameter(propertyType.Key, type, defaultValue)
                                {DisplayName = displayName};
                        }
                        else if (type.IsEnum)
                        {
                            parameter = new EnumOptimizerParameter(propertyType.Key, type, value)
                                {DisplayName = displayName};


                        }
                        else if (type == typeof(bool))
                        {
                            bool defaultValue = (bool?) value ?? false;
                            parameter = new BoolOptimizerParameter(propertyType.Key, type, defaultValue)
                                {DisplayName = displayName};
                        }

                        if (parameter != null)
                        {
                            OptimizerParameters.Add(parameter);
                            parameter.PropertyChanged += Parameter_PropertyChanged;
                        }
                    }
                }
            }
            else
            {
                foreach (var parameter in parameters)
                {
                    

                    OptimizerParameters.Add(parameter);
                    parameter.PropertyChanged += Parameter_PropertyChanged;
                }
            }
        }

        
        

        private void Parameter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CombinationCount")
            {
                OnPropertyChanged("CombinationCount");
                OnPropertyChanged("DefaultCombinationCount");
            }
        }


        public override void SetDataCalcContext(DataCalcContext dataCalcContext)
        {
            DataCalcContext = dataCalcContext;
            Reset();
            if (DataCalcContext != null)
                State = DataCalcContext.State;
        }

        private SemaphoreSlim _concurrencySemaphore;


        public abstract Task OnOptimize();


        public void RunIteration()
        {
            try
            {
                if (TaskBatchSize > 0)
                {
                    _concurrencySemaphore.Wait(CancellationTokenSource.Token);
                }

                LastTask = Task.Run(async () => await Iterate(CancellationTokenSource), CancellationTokenSource.Token);
                OptimizationTasks.Add(LastTask);
              
            }
            catch (OperationCanceledException)
            {

            }
            catch(Exception ex)
            {
                throw new Exception("Run iteration exception", ex.InnerException);
            }
        }

        public void EnqueueParams()
        {
            lock (_lockObject)
            {
                _tvmParamsQueue.Enqueue(GetParamValues());
            }
        }


        public Dictionary<string, object> DequeueParams()
        {
            lock (_lockObject)
            {
                return _tvmParamsQueue.Dequeue();
            }
        }


        private async Task Iterate(CancellationTokenSource cancellationTokenSource)
        {
            StrategyBase strategy = null;
            try
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                cancellationTokenSource.Token.ThrowIfCancellationRequested();

                var dataCalcContext = new DataCalcContext(DataCalcContext.Connection)
                {
                    Candles = DataCalcContext.Candles,
                    Instruments = DataCalcContext.Instruments.ToList(),
                    IsOptimization = true,
                    DataSeriesCash = DataCalcContext.DataSeriesCash
                };


                dataCalcContext.SetParams(DataCalcContext.GetParams());

                strategy = (StrategyBase)Strategy.Clone();
                strategy.Init();
                lock (_lockObject)
                {
                    strategy.SetOptimizerParams(DequeueParams());
                    strategy.CustomValue = CustomValue;
                }

                strategy.SetDataCalcContext(dataCalcContext);
                strategy.SetTwmProperties();
                strategy.IsOptimization = true;

                strategy.SetLocalId();
                strategy.SetState(State.SetDefaults);
                dataCalcContext.Strategies.Add(strategy);
                strategy.Enabled = true;

                await dataCalcContext.ExecuteStrategy(cancellationTokenSource);
                cancellationTokenSource.Token.ThrowIfCancellationRequested();

                strategy.SystemPerformance = new SystemPerformance(new[] { strategy }, isOptimization:true);
                await strategy.SystemPerformance.Calculate(cancellationTokenSource.Token);

                strategy.OptimizationFitness.OnCalculatePerformanceValue(strategy);
                stopWatch.Stop();
                lock (_observableCollectionLock)
                {
                    AllStrategies++;
                    LastStrategy = strategy;
                    var milliseconds = stopWatch.ElapsedMilliseconds;
                    _allTaskTime += milliseconds;
                    AvgTaskTime = _allTaskTime / AllStrategies;

                    OnTaskCompleted?.BeginInvoke(this, new EventArgs(), null, null);

                    if (MinTrades > 0)
                    {
                        if (strategy.Trades < MinTrades)
                        {
                            RemovedStrategiesByMinTrades++;
                            strategy.Clear();
                            return;
                        }
                    }

                    if (DrawDownLevel > 0)
                    {
                        if (strategy.MaxDrawDown < DrawDownLevel*-1)
                        {
                            RemovedStrategiesByDrawDown++;
                            strategy.Clear();
                            return;
                        }
                    }

                    var existsStrategy = StrategyList.FirstOrDefault(x => x.OptimizerParamsHashCode == strategy.OptimizerParamsHashCode);

                    if (existsStrategy != null)
                    {
                        if (existsStrategy.CustomValue is double oldValue && strategy.CustomValue is double newValue)
                        {
                            if (newValue > oldValue)
                            {
                                existsStrategy.CustomValue = strategy.CustomValue;
                            }
                        }
                        return;
                    }


                    if (_strategiesCount < KeepBestNumber)
                    {
                        Application.Current.Dispatcher.InvokeAsync(() => { Strategies.Add(strategy); });
                        StrategyList.Add(strategy);
                        if (_worstTopStrategy == null)
                            _worstTopStrategy = strategy;
                        else
                        {
                            if (_worstTopStrategy.OptimizationFitness.Value > strategy.OptimizationFitness.Value)
                            {
                                _worstTopStrategy = strategy;
                            }
                        }

                        _strategiesCount++;
                    }
                    else
                    {
                        if (_worstTopStrategy.OptimizationFitness.Value < strategy.OptimizationFitness.Value)
                        {
                            var itemForRemove = _worstTopStrategy;
                            Application.Current.Dispatcher.InvokeAsync(() => { Strategies.Remove(itemForRemove); });
                            StrategyList.Remove(itemForRemove);
                            itemForRemove.Clear();
                            Application.Current.Dispatcher.InvokeAsync(() => { Strategies.Add(strategy); });
                            StrategyList.Add(strategy);
                            _worstTopStrategy = StrategyList.OrderBy(x => x.OptimizationFitness.Value).First();
                        }
                        else
                        {
                            strategy.Clear();
                        }
                    }

                    if (dataCalcContext.ExtraDataSeries.Any())
                    {
                        foreach (var dataSeries in dataCalcContext.ExtraDataSeries)
                        {
                            if (!DataCalcContext.DataSeriesCash.ContainsKey(dataSeries.DataSeriesCode))
                                DataCalcContext.DataSeriesCash.Add(dataSeries.DataSeriesCode, dataSeries.Candles);
                        }
                    }

                }
            }
            catch(Exception)
            {
                lock (_observableCollectionLock)
                {
                    LastStrategy = strategy;
                }

                throw;
            }
            finally
            {

                if (TaskBatchSize > 0)
                {
                    _concurrencySemaphore.Release();
                }
            }
            
        }


        public Dictionary<string, object> GetParamValues()
        {
            return _checkedParameters.ToDictionary(x => x.Name, y => y.CurrentValue);
        }


        public void Reset()
        {
            RemovedStrategiesByMinTrades = 0;
            RemovedStrategiesByDrawDown = 0;
            AllStrategies = 0;
            _allTaskTime = 0;
            AvgTaskTime = 0;

            _concurrencySemaphore = new SemaphoreSlim(TaskBatchSize);

            _checkedParameters = null;

            _observableCollectionLock = new object();

            _lockObject = new object();

            if (Strategies != null)
            {
                Application.Current.Dispatcher.Invoke(() => { Strategies.Clear(); });
                BindingOperations.EnableCollectionSynchronization(Strategies, _observableCollectionLock);
            }

            _worstTopStrategy = null;
            lock (_lockObject)
            {
                _tvmParamsQueue = new Queue<Dictionary<string, object>>();
            }

            OptimizationTasks = new ObservableCollection<Task>();

            _strategiesCount = 0;
            StrategyList = new List<StrategyBase>();
        }

        public void SetParams()
        {
            if (_checkedParameters == null)
            {
                _checkedParameters = OptimizerParameters.Where(x => x.IsChecked).ToList();
            }
        }

        public object Clone()
        {
            Optimizer optimizer = (Optimizer) MemberwiseClone();
            optimizer.OptimizerParameters = new ObservableCollection<OptimizerParameter>();
            foreach (var parameter in OptimizerParameters)
            {
                optimizer.OptimizerParameters.Add((OptimizerParameter) parameter.Clone());
            }

            return optimizer;
        }

        public void Clear()
        {
            Strategy?.Clear();
            LastStrategy = null;
            StrategyList?.Clear();
            Strategies?.Clear();
        }

        public void CopyParamsFrom(Optimizer optimizer)
        {
            OptimizerParameters = optimizer.OptimizerParameters;
        }
    }
}