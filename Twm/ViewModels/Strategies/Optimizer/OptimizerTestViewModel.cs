using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Twm.Core.Classes.UI;
using Twm.Core.Controllers;
using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Performance;
using Twm.Core.Enums;
using Twm.Core.Interfaces;
using Twm.Core.Market;
using Twm.Core.ViewModels;

namespace Twm.ViewModels.Strategies.Optimizer
{
    [DataContract]
    public class OptimizerTestViewModel : ViewModelBase, IOptimizerItem, ICloneable
    {
        [DataMember] public ObservableCollection<OptimizerPeriodViewModel> Periods { get; set; }

        public ObservableCollection<StrategyBase> Strategies { get; set; }

        public DataCalcContext DataCalcContext { get; set; }

        public ObservableCollection<TaskViewModel> TaskViewModels { get; set; }

        public ICollectionView TasksView { get; set; }

        public ObservableCollection<ParameterValuesViewModel> MatrixViewModel { get; set; }

        public ICollectionView MatrixView { get; set; }

        public ColumnConfig ColumnConfig { get; set; }



        private string _name;

        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }


        public GraphType? CurrentGraphType { get; set; }


        private bool _isOptimizeRunning;

        public bool IsOptimizeRunning
        {
            get { return _isOptimizeRunning; }
            set
            {
                if (_isOptimizeRunning != value)
                {
                    _isOptimizeRunning = value;
                    OnPropertyChanged();
                }
            }
        }


        private StrategyBase _strategy;

        public StrategyBase Strategy
        {
            get { return _strategy; }
            set
            {
                if (_strategy != value)
                {
                    SetStrategy(value);
                }
            }
        }


        public void SetStrategy(StrategyBase strategy, bool recreateOptimizerParams = true)
        {
            _strategy = strategy;
            OnPropertyChanged("Strategy");

            ParentViewModel?.UpdateProperty("StrategyName");
            ParentViewModel?.UpdateProperty("IsRunEnable");
            DataCalcContext?.Strategies.Clear();
            if (_strategy != null)
            {
                DataCalcContext?.Strategies.Add(_strategy);
                if (_strategy.Optimizer == null)
                    _strategy.CreateOptimizer();
                else
                {
                    if (recreateOptimizerParams)
                        _strategy.Optimizer.CreateParams();
                }

                if (_strategy.OptimizationFitness == null)
                    _strategy.CreateOptimizationFitness();
                SelectedStrategy = new StrategyViewModel(_strategy);
            }

            ParentViewModel?.UpdateProperty("Strategy");
        }

        private StrategyViewModel _selectedStrategy;

        public StrategyViewModel SelectedStrategy
        {
            get { return _selectedStrategy; }
            set
            {
                if (value != _selectedStrategy)
                {
                    _selectedStrategy = value;


                    OnPropertyChanged();
                }
            }
        }


        private bool _isExpanded;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StrategyName
        {
            get { return _strategy.DisplayName; }
        }

        public Visibility TopExpanderVisibility => Visibility.Collapsed;
        public bool StartPerformanceCalc { get; set; }

        private readonly object _lock = new object();
        public StrategyPerformanceViewModel Performance;


        public OptimizerTestViewModel()
        {
            Periods = new ObservableCollection<OptimizerPeriodViewModel>();
            DataCalcContext = new DataCalcContext();


            TaskViewModels = new ObservableCollection<TaskViewModel>();
            TasksView = CollectionViewSource.GetDefaultView(TaskViewModels);

            MatrixViewModel = new ObservableCollection<ParameterValuesViewModel>();
            MatrixView = CollectionViewSource.GetDefaultView(MatrixViewModel);

            ColumnConfig = new ColumnConfig();
            ColumnConfig.Clear();
        }


        public bool? CheckStrategyBrowsableProperty(string propertyName)
        {
            //TODO: Check this (Enabled)
            if (Strategy.IsTwmProperty(propertyName) || propertyName == "Enabled")
            {
                return false;
            }

            //TODO: Check this
            if (propertyName == "DataSeriesSeriesParams")
            {
                return true;
            }

            return null;
        }


        public ManualResetEventSlim CalculateEvent { get; set; }


        public async Task CalcTotalPerformance(CancellationToken cancellationToken, bool reCalcTotal = false, IEnumerable<IRiskLevel> painLevels = null)
        {
            if (cancellationToken.IsCancellationRequested)
                return;


            var trades = new List<ITrade>();

            
            lock (_lock)
            {
                if (StartPerformanceCalc)
                    return;
                                
                var notLivePeriod = Periods.Where(x => !x.IsLive);

                foreach (var period in notLivePeriod)
                {
                    if (period.SelectedStrategy == null && period.IsEmptyResult)
                        continue;

                    if (!SystemOptions.Instance.CalculateSimulation)
                    {
                        if (period.SelectedStrategy?.CalcOSPerformanceTask == null ||
                            !period.SelectedStrategy.CalcOSPerformanceTask.IsCompleted)
                            return;
                    }
                    else
                    {
                        if (period.SelectedStrategy?.CalcSimPerformanceTask == null ||
                            !period.SelectedStrategy.CalcSimPerformanceTask.IsCompleted)
                            return;
                    }


                    foreach (var trade in period.SelectedStrategy.OsPerformance.Trades)
                    {
                        trade.PeriodNum = period.Number;
                    }

                    trades.AddRange(period.SelectedStrategy.OsPerformance.Trades);
                }

                StartPerformanceCalc = true;
            }

            if (trades.Any())
            {
                if (reCalcTotal)
                {
                    Performance.IsLoaded = false;
                    Performance.Clear();
                    Performance.Reset();
                    Performance.SetAllTrades(trades.ToArray());
                }
                else
                {
                    var po = new PerformanceOptions
                    {
                        IsPortfolio = false,
                        ParentViewModel = this,
                        ExcludeSections = new List<object>
                            {StrategyPerformanceSection.Chart, StrategyPerformanceSection.Orders}
                    };
                    Performance = new StrategyPerformanceViewModel(null, trades.ToArray(), po);
                }



                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Performance.IsBusy = true;
                    Performance.Message = "Loading...";
                });

                try
                {
                    await Performance.Calculate(cancellationToken, painLevels);
                    SelectedStrategy.TotalPerformance = Performance;


                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        MatrixViewModel.Clear();
                        ColumnConfig.Clear();
                        var i = 0;
                        var firstStrategy = true;
                        foreach (var period in Periods)
                        {
                            ColumnConfig.Headers[i] = (i + 1).ToString();
                            ColumnConfig.Widths[i] = 100;


                            if (period.SelectedStrategy != null)
                            {

                                var strategyValues = period.SelectedStrategy.Strategy.GetTwmPropertyValues();

                                foreach (var strategyValue in strategyValues)
                                {
                                    ParameterValuesViewModel parameterValuesViewModel;

                                    if (firstStrategy)
                                    {
                                        parameterValuesViewModel = new ParameterValuesViewModel
                                            {Name = strategyValue.Key};
                                        MatrixViewModel.Add(parameterValuesViewModel);
                                    }
                                    else
                                    {
                                        parameterValuesViewModel =
                                            MatrixViewModel.FirstOrDefault(x => x.Name == strategyValue.Key);
                                    }

                                    if (strategyValue.Value is double val)
                                    {
                                        parameterValuesViewModel.Values[i] = Math.Round(val, 2);
                                    }
                                    else
                                        parameterValuesViewModel.Values[i] = strategyValue.Value;
                                    parameterValuesViewModel.Visibilities[i] = Visibility.Visible;
                                }

                                firstStrategy = false;
                            }

                            i++;
                        }
                    });


                }
                finally
                {
                    lock (_lock)
                    {
                        StartPerformanceCalc = false;
                    }

                    Performance.IsBusy = false;

                    CalculateEvent?.Set();
                }
            }
            else
            {
                lock (_lock)
                {
                    StartPerformanceCalc = false;
                }
                CalculateEvent?.Set();
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}