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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Twm.Chart.Interfaces;
using Twm.Classes;
using Twm.Core.Classes;
using Twm.Core.Classes.UI;
using Twm.Core.Controllers;
using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Performance;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Core.Interfaces;
using Twm.Core.Managers;
using Twm.Core.Market;
using Twm.Core.Messages;
using Twm.Core.UI.Windows;
using Twm.Core.UI.Windows.Presets;
using Twm.Core.ViewModels;
using Twm.Core.ViewModels.DataSeries;
using Twm.Core.ViewModels.Presets;
using Twm.DB.DAL.Repositories.Presets;
using Twm.Model.Model;
using Twm.ViewModels.Charts;
using Twm.ViewModels.Presets;
using Twm.ViewModels.Strategies.Validator;
using Twm.Windows.Strategies;
using Twm.Windows.Tools;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;



namespace Twm.ViewModels.Strategies.Optimizer
{
    [DataContract]
    public class OptimizerPeriodViewModel : ViewModelBase, IOptimizerItem, ICloneable
    {
        public OptimizerTestViewModel Test { get; set; }

        public DataCalcContext ISDataCalcContext { get; set; }
        public DataCalcContext OSDataCalcContext { get; set; }
        public DataCalcContext SimDataCalcContext { get; set; }

        public DataSeriesParamsViewModel IsDataSeriesParams { get; set; }
        public DataSeriesParamsViewModel OsDataSeriesParams { get; set; }
        public DataSeriesParamsViewModel SimDataSeriesParams { get; set; }

        public ObservableCollection<StrategyBase> Strategies { get; set; }

        public ObservableCollection<StrategyViewModel> StrategyViewModels { get; set; }

        public ICollectionView StrategiesView { get; set; }

        public GridColumnCustomization StrategiesColumnCustomization { get; set; }

        public ObservableCollection<ParameterValuesViewModel> MatrixViewModel { get; set; }

        public ICollectionView MatrixView { get; set; }

        public ColumnConfig ColumnConfig { get; set; }


        private bool _isLive;
        [DataMember]
        public bool IsLive
        {
            get { return _isLive; }
            set
            {
                if (_isLive != value)
                {
                    _isLive = value;
                }
            }
        }


        private readonly object _busyLock = new object();


        private bool _applyRiskLevels;

        public bool ApplyRiskLevels
        {
            get { return _applyRiskLevels; }
            set
            {
                if (_applyRiskLevels != value)
                {
                    _applyRiskLevels = value;
                    OnPropertyChanged();
                }
            }
        }


        private int _number;

        [DataMember]
        public int Number
        {
            get { return _number; }
            set
            {
                if (_number != value)
                {
                    _number = value;
                    OnPropertyChanged();
                    OnPropertyChanged("DisplayName");
                    OnPropertyChanged("PeriodName");
                }
            }
        }


        public event EventHandler<MessageEventArgs> OnCalculating;


        public int RemovedStrategiesByDrawDown
        {
            get
            {
                if (PeriodStrategy?.Optimizer == null)
                    return 0;
                return PeriodStrategy.Optimizer.RemovedStrategiesByDrawDown;
            }
        }

        public int RemovedStrategiesByMinTrades
        {
            get
            {
                if (PeriodStrategy?.Optimizer == null)
                    return 0;
                return PeriodStrategy.Optimizer.RemovedStrategiesByMinTrades;
            }
        }


        public int AllStrategies
        {
            get
            {
                if (PeriodStrategy == null || PeriodStrategy.Optimizer == null)
                    return 0;

                return PeriodStrategy.Optimizer.AllStrategies;
            }
        }


        public StrategyBase Strategy
        {
            get { return Test.Strategy; }
            set
            {
                if (Test.Strategy != value)
                {
                    Test.Strategy = value;
                    OnPropertyChanged();
                    ParentViewModel?.UpdateProperty("Strategy");
                }
            }
        }


        private StrategyPerformanceSection? _currentSection;

        public StrategyPerformanceSection? CurrentSection
        {
            get { return _currentSection; }
            set
            {
                if (_currentSection != value)
                {
                    SelectedStrategy?.IsPerformance?.SelectSection(value);
                    SelectedStrategy?.OsPerformance?.SelectSection(value);
                    SelectedStrategy?.TotalPerformance?.SelectSection(value);
                    SelectedStrategy?.SimPerformance?.SelectSection(value);

                    _currentSection = value;
                    OnPropertyChanged();
                }
            }
        }


        private StrategyViewModel _selectedStrategy;

        public StrategyViewModel SelectedStrategy
        {
            get { return _selectedStrategy; }
            set
            {
                if (value == null)
                {
                    IsEmptyResult = true;
                    IsPerformance = null;
                    OsPerformance = null;
                    SimPerformance = null;
                    TotalPerformance = null;
                    _selectedStrategy = value;
                    OnPropertyChanged();
                    OnPropertyChanged("AllStrategies");
                    OnPropertyChanged("RemovedStrategies");
                    return;
                }

                if (value != _selectedStrategy)
                {
                    var prevStrategy = _selectedStrategy;
                    _selectedStrategy = value;
                    OnPropertyChanged();
                    OnPropertyChanged("AllStrategies");
                    OnPropertyChanged("RemovedStrategies");
                    Task.Run(() => SelectStrategy(_selectedStrategy, prevStrategy), PeriodStrategy.Optimizer.CancellationTokenSource.Token);
                }
            }
        }


        private async Task SelectStrategy(StrategyViewModel strategy, StrategyViewModel prevStrategy)
        {
            IsBusy = true;
            Message = "Loading...";
            try
            {
                if (prevStrategy != null)
                {
                    strategy.SelectedTabIndex = prevStrategy.SelectedTabIndex;
                }

                if (strategy.IsPerformance == null)
                {
                    await Task.Delay(10);
                    var task = CalcISPerformance(strategy, CurrentSection,
                        //PeriodStrategy.Optimizer.CancellationTokenSource
                        new CancellationTokenSource()
                        );
                    strategy.CalcISPerformanceTask = task;
                }
                else
                {
                    strategy.IsPerformance.SelectSection(CurrentSection);
                    IsPerformance = strategy.IsPerformance;
                }


                if (!IsLive)
                {
                    if (strategy.OsPerformance == null)
                    {
                        var task = CalcOSPerformance(strategy, CurrentSection,
                            PeriodStrategy.Optimizer.CancellationTokenSource);
                        strategy.CalcOSPerformanceTask = task;
                    }
                    else
                    {
                        strategy.OsPerformance.SelectSection(CurrentSection);
                        OsPerformance = strategy.OsPerformance;
                    }

                    if (SystemOptions.Instance.CalculateSimulation)
                    {
                        if (strategy.SimPerformance == null)
                        {
                            var task = CalcSimPerformance(strategy, CurrentSection,
                                PeriodStrategy.Optimizer.CancellationTokenSource);
                            strategy.CalcSimPerformanceTask = task;
                        }
                        else
                        {
                            strategy.SimPerformance.SelectSection(CurrentSection);
                            SimPerformance = strategy.SimPerformance;
                        }
                    }

                    if (strategy.TotalPerformance == null)
                    {
                        await CalcTotalPerformance(strategy, PeriodStrategy.Optimizer.CancellationTokenSource.Token);
                    }
                    else
                    {
                        strategy.TotalPerformance.SelectSection(CurrentSection);
                        TotalPerformance = strategy.TotalPerformance;
                        Test.CalcTotalPerformance(PeriodStrategy.Optimizer.CancellationTokenSource.Token);
                    }
                }
                else
                {
                    Test.CalcTotalPerformance(PeriodStrategy.Optimizer.CancellationTokenSource.Token, false);
                    OnCalculating?.BeginInvoke(this, new MessageEventArgs("", "", "Completed"), null, null);
                }
            }
            finally
            {
                PeriodStrategy.Optimizer.CancellationTokenSource = new CancellationTokenSource();

                IsBusy = false;
            }


        }


        public bool IsPresetSaveEnable
        {
            get { return SelectedStrategy != null; }
        }


        private Visibility? _simPerformanceVisibility;

        public Visibility? SimPerformanceVisibility
        {
            get
            {
                if (_simPerformanceVisibility == null)
                {
                    if (SystemOptions.Instance.CalculateSimulation && !IsLive)
                    {
                        _simPerformanceVisibility = Visibility.Visible;
                    }
                    else
                        _simPerformanceVisibility = Visibility.Collapsed;
                }

                return _simPerformanceVisibility;
            }
            set
            {
                if (_simPerformanceVisibility != value)
                {
                    _simPerformanceVisibility = value;
                    OnPropertyChanged();
                }
            }
        }


        private StrategyPerformanceViewModel _isPerformance;

        public StrategyPerformanceViewModel IsPerformance
        {
            get { return _isPerformance; }
            set
            {
                if (_isPerformance != value)
                {
                    _isPerformance = value;
                    OnPropertyChanged();
                }
            }
        }

        private StrategyPerformanceViewModel _osPerformance;

        public StrategyPerformanceViewModel OsPerformance
        {
            get { return _osPerformance; }
            set
            {
                if (_osPerformance != value)
                {
                    _osPerformance = value;
                    OnPropertyChanged();
                    _selectedStrategy?.UpdatePerformanceProperties();
                }
            }
        }


        private StrategyPerformanceViewModel _simPerformance;

        public StrategyPerformanceViewModel SimPerformance
        {
            get { return _simPerformance; }
            set
            {
                if (_simPerformance != value)
                {
                    _simPerformance = value;
                    OnPropertyChanged();
                    _selectedStrategy?.UpdatePerformanceProperties();
                }
            }
        }


        private StrategyPerformanceViewModel _totalPerformance;

        public StrategyPerformanceViewModel TotalPerformance
        {
            get { return _totalPerformance; }
            set
            {
                if (_totalPerformance != value)
                {
                    _totalPerformance = value;
                    OnPropertyChanged();
                }
            }
        }


        private DateTime _iSStartDate;

        [DataMember]
        public DateTime ISStartDate
        {
            get { return _iSStartDate; }
            set
            {
                if (value != _iSStartDate)
                {
                    _iSStartDate = value;
                    _isDays = (int) (ISEndDate - ISStartDate).TotalDays;
                    _isPercent = (int) ((_isDays * 100) / (_isDays + _osDays));
                    _osPercent = 100 - _isPercent;
                    _totalDays = _isDays + _osDays;
                    OnPropertyChanged();
                    OnPropertyChanged("DisplayName");
                    OnPropertyChanged("ISDays");
                    OnPropertyChanged("ISPercent");
                    OnPropertyChanged("OSPercent");
                    OnPropertyChanged("TotalDays");
                }
            }
        }

        private DateTime _isEndDate;

        [DataMember]
        public DateTime ISEndDate
        {
            get { return _isEndDate; }
            set
            {
                if (value != _isEndDate)
                {
                    _isEndDate = value;
                    _oSStartDate = _isEndDate.AddDays(1);
                    _isDays = (int) (ISEndDate - ISStartDate).TotalDays;
                    _osDays = (int)(OSEndDate - OSStartDate).TotalDays;
                    _isPercent = (int) ((_isDays * 100) / (_isDays + _osDays));
                    _osPercent = 100 - _isPercent;
                    OnPropertyChanged();
                    OnPropertyChanged("DisplayName");
                    OnPropertyChanged("OSDays");
                    OnPropertyChanged("ISDays");
                    OnPropertyChanged("ISPercent");
                    OnPropertyChanged("OSStartDate");
                    OnPropertyChanged("OSPercent");
                }
            }
        }


        private DateTime _oSStartDate;

        [DataMember]
        public DateTime OSStartDate
        {
            get { return _oSStartDate; }
            set
            {
                if (value != _oSStartDate)
                {
                    _oSStartDate = value;
                    _isEndDate = _oSStartDate.AddDays(-1);
                    _isDays = (int) (ISEndDate - ISStartDate).TotalDays;
                    _osDays = (int) (OSEndDate - OSStartDate).TotalDays;
                    _isPercent = (int) ((_isDays * 100) / (_isDays + _osDays));
                    _osPercent = 100 - _isPercent;
                    OnPropertyChanged();
                    OnPropertyChanged("DisplayName");
                    OnPropertyChanged("OSDays");
                    OnPropertyChanged("ISDays");
                    OnPropertyChanged("ISEndDate");
                    OnPropertyChanged("ISPercent");
                    OnPropertyChanged("OSPercent");
                }
            }
        }

        private DateTime _osEndDate;

        [DataMember]
        public DateTime OSEndDate
        {
            get { return _osEndDate; }
            set
            {
                if (value != _osEndDate)
                {
                    _osEndDate = value;
                    _osDays = (int) (OSEndDate - OSStartDate).TotalDays;
                    _totalDays = _isDays + _osDays;
                    _isPercent = (int) ((_isDays * 100) / _totalDays);
                    _osPercent = 100 - _isPercent;

                    OnPropertyChanged();
                    OnPropertyChanged("OSDays");
                    OnPropertyChanged("DisplayName");
                    OnPropertyChanged("ISPercent");
                    OnPropertyChanged("OSPercent");
                    OnPropertyChanged("TotalDays");
                }
            }
        }


        private int _isDays;

        public int ISDays
        {
            get { return _isDays; }
            set
            {
                if (value != _isDays)
                {
                    _isDays = value;
                    _iSStartDate = ISEndDate.AddDays(-value);
                    _totalDays = _isDays + _osDays;
                    _isPercent = (int) ((_isDays * 100) / _totalDays);
                    _osPercent = 100 - _isPercent;

                    OnPropertyChanged();
                    OnPropertyChanged("ISStartDate");
                    OnPropertyChanged("ISPercent");
                    OnPropertyChanged("OSPercent");
                    OnPropertyChanged("TotalDays");
                }
            }
        }

        private int _osDays;

        public int OSDays
        {
            get { return _osDays; }
            set
            {
                if (value != _osDays)
                {
                    _osDays = value;
                    _oSStartDate = OSEndDate.AddDays(-value);
                    _isEndDate = _oSStartDate.AddDays(-1);
                    _isDays = _totalDays - _osDays;
                    _isPercent = (int) ((_isDays * 100) / _totalDays);
                    _osPercent = 100 - _isPercent;
                    OnPropertyChanged();
                    OnPropertyChanged("ISEndDate");
                    OnPropertyChanged("OSStartDate");
                    OnPropertyChanged("ISDays");
                    OnPropertyChanged("ISPercent");
                    OnPropertyChanged("OSPercent");
                }
            }
        }


        private int _totalDays;

        public int TotalDays
        {
            get { return _totalDays; }
            set
            {
                if (value != _totalDays)
                {
                    _totalDays = value;
                    _isDays = _totalDays - _osDays;
                    _iSStartDate = ISEndDate.AddDays(-(_isDays-1));
                    _isPercent = (int) ((_isDays * 100) / (_isDays + _osDays));
                    _osPercent = 100 - _isPercent;
                    OnPropertyChanged();
                    OnPropertyChanged("ISDays");
                    OnPropertyChanged("ISStartDate");
                    OnPropertyChanged("ISPercent");
                    OnPropertyChanged("OSPercent");
                }
            }
        }


        private int _isPercent;

        public int ISPercent
        {
            get { return _isPercent; }
            set
            {
                if (value != _isPercent)
                {
                    _isPercent = value;
                    _osPercent = 100 - _isPercent;
                    _isDays = _totalDays * _isPercent / 100;
                    _osDays = _totalDays - _isDays;
                    _isEndDate = ISStartDate.AddDays(_isDays);
                    _oSStartDate = _isEndDate.AddDays(1);
                    OnPropertyChanged();
                    OnPropertyChanged("OSPercent");
                    OnPropertyChanged("OSDays");
                    OnPropertyChanged("ISDays");
                    OnPropertyChanged("ISEndDate");
                    OnPropertyChanged("OSStartDate");
                }
            }
        }

        private int _osPercent;

        public int OSPercent
        {
            get { return _osPercent; }
            set
            {
                if (value != _osPercent)
                {
                    _osPercent = value;
                    _isPercent = 100 - _osPercent;
                    _isDays = _totalDays * _isPercent / 100;
                    _osDays = _totalDays - _isDays;
                    _isEndDate = ISStartDate.AddDays(_isDays);
                    _oSStartDate = _isEndDate.AddDays(1);
                    OnPropertyChanged();
                    OnPropertyChanged("ISPercent");
                    OnPropertyChanged("OSDays");
                    OnPropertyChanged("ISDays");
                    OnPropertyChanged("ISEndDate");
                    OnPropertyChanged("OSStartDate");
                }
            }
        }


        private string _name;

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


        public string DisplayName
        {
            get
            {
                if (IsLive)
                {
                    return $"IS{Number}:   {ISStartDate:dd.MM.yyyy} - {ISEndDate:dd.MM.yyyy}";
                }
                else
                {
                    return $"IS{Number}:   {ISStartDate:dd.MM.yyyy} - {ISEndDate:dd.MM.yyyy} \r\n" +
                           $"OS{Number}: {OSStartDate:dd.MM.yyyy} - {OSEndDate:dd.MM.yyyy}";
                }
            }
        }

        public string PeriodName
        {
            get { return $"Period {Number}"; }
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


        private int _selectedTabIndex;

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    OnPropertyChanged();
                }
            }
        }


        public Visibility TopExpanderVisibility => Visibility.Visible;

        public bool IsEmptyResult { get; set; }

        public OperationCommand RunCommand { get; set; }

        public OperationCommand OptimizerPeriodPresetsSaveCommand { get; set; }
        public OperationCommand OptimizerPeriodSettingsCommand { get; set; }

        public OperationCommand OpenInValidatorISCommand { get; set; }
        public OperationCommand OpenInValidatorOSCommand { get; set; }

        public OperationCommand ViewParameterListCommand { get; set; }


        public OptimizerPeriodViewModel()
        {
            ISDataCalcContext = new DataCalcContext() {IsOptimization = true};

            ISStartDate = new DateTime(2021, 05, 01);
            ISEndDate = new DateTime(2021, 05, 20);

            OSStartDate = new DateTime(2021, 05, 21);
            OSEndDate = new DateTime(2021, 05, 31);

            Strategies = new ObservableCollection<StrategyBase>();
            StrategyViewModels = new ObservableCollection<StrategyViewModel>();

            MatrixViewModel = new ObservableCollection<ParameterValuesViewModel>();
            MatrixView = CollectionViewSource.GetDefaultView(MatrixViewModel);

            ColumnConfig = new ColumnConfig();
            ColumnConfig.Clear();


            OptimizerPeriodPresetsSaveCommand = new OperationCommand(OptimizerPeriodPresetSave);

            OptimizerPeriodSettingsCommand = new OperationCommand(OptimizerPeriodStrategiesSettings);

            OpenInValidatorISCommand = new OperationCommand(OpenInValidatorIS);

            OpenInValidatorOSCommand = new OperationCommand(OpenInValidatorOS);

            ViewParameterListCommand = new OperationCommand(ViewParameterList);


            
            var strategiesColumnCustomization = SystemOptions.Instance.ViewOptions["OptimizerPeriodStrategies"].Clone();

            StrategiesColumnCustomization = (GridColumnCustomization)strategiesColumnCustomization;
        }

        private void ViewParameterList(object obj) 
        {
            if (SelectedStrategy != null)
            {
                var strategyParamListWindow = new StrategyParamListWindow(SelectedStrategy);
                strategyParamListWindow.ShowDialog();
            }
        }

        private void OptimizerPeriodStrategiesSettings(object obj)
        {
            var columns = StrategiesColumnCustomization.Columns.ToList();

            var gridColumnInfos = new List<GridColumnInfo>();
            foreach (var column in columns)
            {
                gridColumnInfos.Add((GridColumnInfo) column.Clone());
            }

            var viewSettingsWindow = new ViewSettingsWindow(gridColumnInfos);
            if (viewSettingsWindow.ShowDialog() == true)
            {
                foreach (var column in StrategiesColumnCustomization.Columns)
                {
                    var gridColumnInfo = gridColumnInfos.Find(x => x.Name == column.Name);
                    column.Visibility = gridColumnInfo.Visibility;
                }

                StrategiesColumnCustomization.ReloadColumns();
                StrategiesColumnCustomization.UpdateProperty("Columns");
                StrategiesColumnCustomization.SaveLayout();
            }
        }

        public void InitDataCalcContext(DataCalcContext dataCalcContext,
            DataCalcPeriodType dataCalcPeriodType = DataCalcPeriodType.InSample)
        {
            var dataSeriesParamViewModel = new DataSeriesParamsViewModel();
            var dataSeriesParam = Test.DataCalcContext.GetParams();

            dataSeriesParamViewModel.Instrument = dataSeriesParam.Instrument;
            dataSeriesParamViewModel.DataSeriesType = dataSeriesParam.DataSeriesType;
            dataSeriesParamViewModel.DataSeriesValue = dataSeriesParam.DataSeriesValue;
            dataSeriesParamViewModel.SelectedTimeFrameBase = TimeFrameBase.CustomRange;
            dataSeriesParamViewModel.PeriodStart = ISStartDate;
            dataCalcContext.DataCalcPeriodType = dataCalcPeriodType;
            switch (dataCalcPeriodType)
            {
                case DataCalcPeriodType.InSample:
                    dataCalcContext.Name = "InSample";

                    dataSeriesParamViewModel.PeriodEnd = ISEndDate.AddDays(1);
                    IsDataSeriesParams = dataSeriesParamViewModel;
                    break;
                case DataCalcPeriodType.OutSample:
                    dataCalcContext.Name = "OutSample";
                    dataSeriesParamViewModel.PeriodEnd = OSEndDate.AddDays(1);
                    OsDataSeriesParams = dataSeriesParamViewModel;
                    break;
                case DataCalcPeriodType.Simulation:
                    dataCalcContext.Name = "Simulation";

                    var periodEnd = _optimizationEndDate;
                    periodEnd = TimeZoneInfo.ConvertTime(periodEnd, SystemOptions.Instance.TimeZone);
                    dataSeriesParamViewModel.PeriodEnd = periodEnd;                    
                    SimDataSeriesParams = dataSeriesParamViewModel;
                    break;
            }

            dataCalcContext.SetParams(dataSeriesParamViewModel);
        }


        public StrategyBase PeriodStrategy { get; set; }

        public async Task OptimizeStrategy(StrategyBase strategy, CancellationTokenSource tokenSource)
        {
            IsEmptyResult = false;
            await SetViewSource(Strategies);
            Application.Current.Dispatcher.Invoke(() => { Strategies.Clear(); });

            ISDataCalcContext.DataSeriesCash = new Dictionary<string, IEnumerable<ICandle>>();
            PeriodStrategy = (StrategyBase) strategy.Clone();
            PeriodStrategy.Optimizer = (Core.DataCalc.Optimization.Optimizer) strategy.Optimizer.Clone();
            PeriodStrategy.Optimizer.Reset();
            PeriodStrategy.Optimizer.Strategies = Strategies;
            PeriodStrategy.Optimizer.SetDataCalcContext(ISDataCalcContext);
            PeriodStrategy.Optimizer.SetState(State.SetDefaults);
            PeriodStrategy.Optimizer.SetParams();
            PeriodStrategy.Optimizer.CancellationTokenSource = tokenSource;


            OnCalculating?.BeginInvoke(this, new MessageEventArgs("Optimization", "", "Running"), null, null);
            await ISDataCalcContext.OptimizeStrategy(PeriodStrategy);
            OnCalculating?.BeginInvoke(this, new MessageEventArgs("Optimization end", "", "Running"), null, null);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                StrategyViewModels.Clear();
                foreach (var st in Strategies)
                {
                    StrategyViewModels.Add(new StrategyViewModel(st));
                }
            });

            await SetViewSource(StrategyViewModels);
            StrategiesView.MoveCurrentToFirst();
            SelectedStrategy = (StrategyViewModel) StrategiesView.CurrentItem;

            if (SelectedStrategy == null)
            {
                //if (!IsLive)
                    await Test.CalcTotalPerformance(PeriodStrategy.Optimizer.CancellationTokenSource.Token);
                
                OnCalculating?.BeginInvoke(this, new MessageEventArgs("", "", "Completed"), null, null);
            }
        }


        public async Task SetViewSource(object source)
        {
            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
            {
                StrategiesView = CollectionViewSource.GetDefaultView(source);
                StrategiesView.SortDescriptions.Add(new SortDescription("OptimizationFitness.Value",
                    ListSortDirection.Descending));
                StrategiesView.Refresh();
                OnPropertyChanged("StrategiesView");
            }));
        }


        private async Task LoadData(DataCalcContext dataCalcContext, DataSeriesParams dataSeriesSeriesParams,
            CancellationToken token)
        {
            lock (_busyLock)
            {
                IsBusy = true;
            }

            var historicalDataManager = new HistoricalDataManager(dataCalcContext);
            try
            {
                historicalDataManager.RaiseMessageEvent += HistoricalDataManagerRaiseMessageEvent;


                var historicalCandles = await historicalDataManager.GetData(dataSeriesSeriesParams, token);

                token.ThrowIfCancellationRequested();

                Application.Current.Dispatcher.Invoke(() =>
                    {
                        var candleSource = new ObservableCollection<ICandle>();

                        foreach (var historicalCandle in historicalCandles)
                        {
                            token.ThrowIfCancellationRequested();

                            candleSource.Add(Session.Mapper.Map<IHistoricalCandle, ICandle>(historicalCandle));
                        }

                        dataCalcContext.Candles = candleSource;
                    }
                );
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



        private async Task CheckAndDownloadDataFromService(DataCalcContext dataCalcContext, DataSeriesParams dataSeriesSeriesParams,
            CancellationToken token)
        {
            lock (_busyLock)
            {
                IsBusy = true;
            }

            var historicalDataManager = new HistoricalDataManager(dataCalcContext);
            try
            {
                historicalDataManager.RaiseMessageEvent += HistoricalDataManagerRaiseMessageEvent;


                await historicalDataManager.GetData(dataSeriesSeriesParams, token, false);

                token.ThrowIfCancellationRequested();

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

        public void OnCancelOptimize()
        {
            OnCalculating?.Invoke(this, new MessageEventArgs("", "", "Canceling"));
        }

        public void OnCanceledOptimize()
        {
            OnCalculating?.Invoke(this, new MessageEventArgs("", "", "Canceled"));
        }

        public void OnErrorOptimize(string message, string subMessage)
        {
            OnCalculating?.Invoke(this, new MessageEventArgs(message, subMessage, "Error"));
        }

        private void HistoricalDataManagerRaiseMessageEvent(object sender, Core.Messages.MessageEventArgs e)
        {
            Message = e.Message;
            SubMessage = e.SubMessage;
            e.Status = "Running";
            e.Tag = "DataLoad";
            OnCalculating?.Invoke(sender, e);
        }

        public bool? CheckStrategyBrowsableProperty(string propertyName)
        {
            return Test?.CheckStrategyBrowsableProperty(propertyName);
        }

        private DateTime _optimizationEndDate;
        public async Task Optimize(CancellationTokenSource ctsOptimize, Barrier barrier, bool isLastPeriod, DateTime periodEnd)
        {
            try
            {
                _optimizationEndDate = periodEnd;
                InitDataCalcContext(ISDataCalcContext);

                if (barrier != null && !isLastPeriod)
                {
                    barrier.SignalAndWait(ctsOptimize.Token);
                }

                var dataSeriesParam = (DataSeriesParamsViewModel)IsDataSeriesParams.Clone();
                dataSeriesParam.PeriodEnd = OSEndDate;
                ISDataCalcContext.Connection = Session.GetConnection(dataSeriesParam.Instrument.ConnectionId);

                if (SystemOptions.Instance.CalculateSimulation)
                {                    
                    periodEnd = TimeZoneInfo.ConvertTimeFromUtc(periodEnd, SystemOptions.Instance.TimeZone);
                    dataSeriesParam.PeriodEnd = periodEnd;
                }

                if (barrier != null)
                {
                    await CheckAndDownloadDataFromService(ISDataCalcContext, dataSeriesParam, ctsOptimize.Token);
                }

                await LoadData(ISDataCalcContext, IsDataSeriesParams, ctsOptimize.Token);

                if (barrier != null && isLastPeriod)
                {
                    /*using (var context = App.DbContextFactory.GetContext())
                    {
                        var historicalMetaData = context.HistoricalMetaDatas.FirstOrDefault(
                            x => x.Symbol == IsDataSeriesParams.Instrument.Symbol &&
                                 x.DataType == "Candle" &&
                                 x.DataSeriesValue == IsDataSeriesParams.DataSeriesValue &&
                                 x.DataSeriesType == IsDataSeriesParams.DataSeriesType.ToString());

                        if (historicalMetaData == null ||
                            (!IsLive && historicalMetaData.PeriodEnd < OSEndDate.AddDays(1).ToUniversalTime().Date)||
                            (IsLive && historicalMetaData.PeriodEnd < ISEndDate.AddDays(1).ToUniversalTime().Date) 
                            )
                        {
                            if (MessageBox.Show(
                                    "Not enough data for this instrument." + "\r\n" +
                                    "Do you want to continue?", "Сonfirmation",
                                    MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                            {
                                ctsOptimize.Cancel();
                                barrier.SignalAndWait(ctsOptimize.Token);
                                return;
                            }
                        }
                    }*/


                    barrier.SignalAndWait(ctsOptimize.Token);
                }


                ctsOptimize.Token.ThrowIfCancellationRequested();

                OSDataCalcContext = null;
                SimDataCalcContext = null;
                await OptimizeStrategy(Test.Strategy, ctsOptimize);
            }
            catch (OperationCanceledException)
            {
                var t = 0;
            }
            catch (Exception)
            {
                ctsOptimize.Cancel();
                throw;
            }
        }


        private async Task CalcISPerformance(StrategyViewModel strategy, StrategyPerformanceSection? section,
            CancellationTokenSource cancellationTokenSource)
        {
            var dataCalcContext = new DataCalcContext(ISDataCalcContext.Connection)
            {
                Candles = ISDataCalcContext.Candles,
                Instruments = ISDataCalcContext.Instruments
            };

            dataCalcContext.SetParams(ISDataCalcContext.GetParams());

            var po = new PerformanceOptions
            {
                IsPortfolio = false, 
                ParentViewModel = this
            };
            strategy.IsPerformance = new StrategyPerformanceViewModel(new[] {strategy.Strategy}, null, po)
            {
                ChartViewModel = new ChartViewModel(IsDataSeriesParams, dataCalcContext)
            };

            /*await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                strategy.IsPerformance.IsBusy = true;
                strategy.IsPerformance.Message = "Loading...";
            });*/

            try
            {
                strategy.Strategy.SetDataCalcContext(dataCalcContext);
                dataCalcContext.Strategies.Add(strategy.Strategy);
                dataCalcContext.Chart = strategy.IsPerformance.ChartViewModel.Chart;
                strategy.Strategy.SetChart(strategy.IsPerformance.ChartViewModel.Chart);
                await dataCalcContext.ExecuteStrategy(cancellationTokenSource);
                await strategy.IsPerformance.Calculate(cancellationTokenSource.Token);
                strategy.Strategy.SystemPerformance = strategy.IsPerformance;
                strategy.IsPerformance.SelectSection(CurrentSection);
                IsPerformance = strategy.IsPerformance;
            }
            finally
            {
                //strategy.IsPerformance.IsBusy = false;
            }
        }


        private async Task CalcOSPerformance(StrategyViewModel strategy, StrategyPerformanceSection? section,
            CancellationTokenSource cancellationTokenSource)
        {
            /*Application.Current.Dispatcher.Invoke(() =>
            {
                IsBusy = true;
                Message = "Loading...";
            });*/

            if (OSDataCalcContext == null)
            {
                OSDataCalcContext = new DataCalcContext();
                InitDataCalcContext(OSDataCalcContext, DataCalcPeriodType.OutSample);
                OSDataCalcContext.Connection = Session.GetConnection(OsDataSeriesParams.Instrument.ConnectionId);
                await LoadData(OSDataCalcContext, OsDataSeriesParams,
                    PeriodStrategy.Optimizer.CancellationTokenSource.Token);
            }

            cancellationTokenSource.Token.ThrowIfCancellationRequested();

            var dataCalcContext = new DataCalcContext(OSDataCalcContext.Connection)
            {
                Candles = OSDataCalcContext.Candles,
                Instruments = OSDataCalcContext.Instruments,
                DataCalcPeriodType = OSDataCalcContext.DataCalcPeriodType
            };
            dataCalcContext.SetParams(OSDataCalcContext.GetParams());
            var newStrategy = (StrategyBase) strategy.Strategy.Clone();
            newStrategy.Init();
            newStrategy.SetDataCalcContext(dataCalcContext);
            newStrategy.SetTwmProperties();
            newStrategy.SetLocalId();
            dataCalcContext.Strategies.Add(newStrategy);
            newStrategy.Enabled = true;

            var po = new PerformanceOptions
            {
                IsPortfolio = false, 
                ParentViewModel = this
            };
            strategy.OsPerformance = new StrategyPerformanceViewModel(new[] {newStrategy}, null, po)
            {
                ChartViewModel = new ChartViewModel(OsDataSeriesParams, dataCalcContext)
            };
            /*Application.Current.Dispatcher.Invoke(() =>
            {
                strategy.OsPerformance.IsBusy = true;
                strategy.OsPerformance.Message = "Loading...";
            });*/
            cancellationTokenSource.Token.ThrowIfCancellationRequested();

            try
            {
                OnCalculating?.BeginInvoke(this, new MessageEventArgs("Os performance calculating", "", "Running"),
                    null, null);
                dataCalcContext.Chart = strategy.OsPerformance.ChartViewModel.Chart;
                newStrategy.SetChart(strategy.OsPerformance.ChartViewModel.Chart);

                await dataCalcContext.ExecuteStrategy(cancellationTokenSource);

                newStrategy.ClearTrades(OSStartDate);
                strategy.OsPerformance.ChartViewModel.ClearTrades(OSStartDate);

                var osCandle = dataCalcContext.Candles.FirstOrDefault(x => x.t >= OSStartDate);
                if (osCandle != null)
                {
                    var index = dataCalcContext.Candles.IndexOf(osCandle);
                    newStrategy.Draw.LineVertical("OSLine", index, Brushes.Red, 3, DashStyles.Solid);
                }

                await strategy.OsPerformance.Calculate(cancellationTokenSource.Token);

                strategy.OsPerformance.SelectSection(CurrentSection);
                OsPerformance = strategy.OsPerformance;
                OnCalculating?.BeginInvoke(this, new MessageEventArgs("Os performance calculated", "", "Running"), null,
                    null);
            }
            finally
            {
                /*await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    strategy.OsPerformance.IsBusy = false;
                });*/
            }
        }


        private async Task CalcSimPerformance(StrategyViewModel strategy, StrategyPerformanceSection? section,
            CancellationTokenSource cancellationTokenSource)
        {
            if (SimDataCalcContext == null)
            {
                SimDataCalcContext = new DataCalcContext();
                InitDataCalcContext(SimDataCalcContext, DataCalcPeriodType.Simulation);
                SimDataCalcContext.Connection = Session.GetConnection(SimDataSeriesParams.Instrument.ConnectionId);
                await LoadData(SimDataCalcContext, SimDataSeriesParams,
                    PeriodStrategy.Optimizer.CancellationTokenSource.Token);
            }

            cancellationTokenSource.Token.ThrowIfCancellationRequested();

            var dataCalcContext = new DataCalcContext(SimDataCalcContext.Connection)
            {
                Candles = SimDataCalcContext.Candles,
                Instruments = SimDataCalcContext.Instruments,
                DataCalcPeriodType = SimDataCalcContext.DataCalcPeriodType
            };
            dataCalcContext.SetParams(SimDataCalcContext.GetParams());

            var newStrategy = (StrategyBase) strategy.Strategy.Clone();
            newStrategy.Init();
            newStrategy.SetDataCalcContext(dataCalcContext);
            newStrategy.SetTwmProperties();
            newStrategy.SetLocalId();
            dataCalcContext.Strategies.Add(newStrategy);
            newStrategy.Enabled = true;

            var po = new PerformanceOptions
            {
                IsPortfolio = false, 
                ParentViewModel = this
            };
            strategy.SimPerformance = new StrategyPerformanceViewModel(new[] {newStrategy}, null, po)
            {
                ChartViewModel = new ChartViewModel(SimDataSeriesParams, dataCalcContext)
            };
            /*Application.Current.Dispatcher.Invoke(() =>
            {
                strategy.SimPerformance.IsBusy = true;
                strategy.SimPerformance.Message = "Loading...";
            });*/
            cancellationTokenSource.Token.ThrowIfCancellationRequested();

            try
            {
                OnCalculating?.BeginInvoke(this, new MessageEventArgs("Sim performance calculating", "", "Running"),
                    null, null);
                dataCalcContext.Chart = strategy.SimPerformance.ChartViewModel.Chart;
                newStrategy.SetChart(strategy.SimPerformance.ChartViewModel.Chart);

                await dataCalcContext.ExecuteStrategy(cancellationTokenSource);


                newStrategy.ClearTrades(OSEndDate.AddDays(1));
                strategy.SimPerformance.ChartViewModel.ClearTrades(OSEndDate.AddDays(1));

                var osCandle = dataCalcContext.Candles.FirstOrDefault(x => x.t >= OSStartDate);
                if (osCandle != null)
                {
                    var index = dataCalcContext.Candles.IndexOf(osCandle);
                    newStrategy.Draw.LineVertical("OSLine", index, Brushes.Red, 3, DashStyles.Solid);
                }

                var simCandle = dataCalcContext.Candles.FirstOrDefault(x => x.t >= OSEndDate.AddDays(1));
                if (simCandle != null)
                {
                    var index = dataCalcContext.Candles.IndexOf(simCandle);
                    newStrategy.Draw.LineVertical("SimLine", index, Brushes.Red, 3, DashStyles.Solid);
                }

                await strategy.SimPerformance.Calculate(cancellationTokenSource.Token);
                strategy.SimPerformance.SelectSection(CurrentSection);
                SimPerformance = strategy.SimPerformance;

                OnCalculating?.BeginInvoke(this, new MessageEventArgs("Sim performance calculated", "", "Running"),
                    null,
                    null);
            }
            finally
            {
                /*await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    strategy.SimPerformance.IsBusy = false;
                });*/
            }
        }


        public async Task CalcTotalPerformance(StrategyViewModel strategy, CancellationToken cancellationToken,
            bool reCalcTotal = false)
        {
            var po = new PerformanceOptions
            {
                IsPortfolio = false,
                ParentViewModel = this,
                ExcludeSections = new List<object>
                {
                    StrategyPerformanceSection.Chart, StrategyPerformanceSection.Orders
                }
            };

            var trades = new List<ITrade>();

            List<Task> tasks = new List<Task>
                {strategy.CalcISPerformanceTask, strategy.CalcOSPerformanceTask, strategy.CalcSimPerformanceTask};
            tasks.RemoveAll(x => x == null);


            await Task.WhenAll(tasks);


            trades.AddRange(strategy.IsPerformance.Trades);
            trades.AddRange(strategy.OsPerformance.Trades);
            if (SystemOptions.Instance.CalculateSimulation)
            {
                trades.AddRange(strategy.SimPerformance.Trades);
                //allTradesPerformance = new StrategyPerformanceViewModel(null, trades.ToArray(), po);
            }

            var allTrades = trades.Select(x => new Trade().CopyFrom(x)).ToList();

            allTrades.ForEach(x => x.Profit += x.Commission);

            strategy.TotalPerformance = new StrategyPerformanceViewModel(null, allTrades.ToArray(), po);
           

            try
            {
                OnCalculating?.BeginInvoke(this, new MessageEventArgs("Total performance calculating", "", "Running"),
                    null, null);
                await strategy.TotalPerformance.Calculate(cancellationToken);
                strategy.TotalPerformance.SelectSection(CurrentSection);
                TotalPerformance = strategy.TotalPerformance;


                /*if (SystemOptions.Instance.CalculateSimulation && allTradesPerformance != null)
                {
                    await allTradesPerformance.Calculate(cancellationToken);
                    strategy.SimPerformance.Analysis = allTradesPerformance.Analysis;
                    SimPerformance = strategy.SimPerformance;
                }*/

                OnCalculating?.BeginInvoke(this, new MessageEventArgs("Matrix building", "", "Running"), null, null);

                await FillMatrix();

                Test.CalcTotalPerformance(cancellationToken, reCalcTotal);

                OnCalculating?.BeginInvoke(this, new MessageEventArgs("", "", "Completed"), null, null);
            }
            finally
            {
                PeriodStrategy.Optimizer.CancellationTokenSource = new CancellationTokenSource();

                await Application.Current.Dispatcher.InvokeAsync(() => { IsBusy = false; });
            }
        }


        private async Task FillMatrix()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MatrixViewModel.Clear();
                ColumnConfig.Clear();
                var i = 0;
                foreach (StrategyViewModel bestStrategy in StrategiesView)
                {
                    ColumnConfig.Headers[i] = (i + 1).ToString();
                    ColumnConfig.Widths[i] = 100;


                    var strategyValues = bestStrategy.Strategy.GetTwmPropertyValues();


                    foreach (var strategyValue in strategyValues)
                    {
                        ParameterValuesViewModel parameterValuesViewModel;

                        if (i == 0)
                        {
                            parameterValuesViewModel = new ParameterValuesViewModel {Name = strategyValue.Key};
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

                    i++;
                }
            });
        }

        private async void OptimizerPeriodPresetSave(object obj)
        {
            if (SelectedStrategy != null)
            {
                var presetsViewModel = new PresetsViewModel(PresetType.Strategy);
                presetsViewModel.FetchData(SelectedStrategy.Strategy.Guid.ToString());
                presetsViewModel.PresetName = SelectedStrategy.Name;

                var presetsWindow = new PresetsWindow(presetsViewModel);
                if (presetsWindow.ShowDialog() == true)
                {
                    Preset preset = null;
                    if (presetsViewModel.SelectedPreset != null)
                        preset = presetsViewModel.SelectedPreset.DataModel;
                    await SaveStrategyPreset(SelectedStrategy, preset, presetsViewModel.PresetName);
                }
            }
        }

        private async void OpenInValidatorIS(object obj)
        {
            var name = $"{Test.Name} IS{Number}";

            await LoadInValidator(name, ISStartDate, ISEndDate);
        }


        private async void OpenInValidatorOS(object obj)
        {
            var name = $"{Test.Name} OS{Number}";

            await LoadInValidator(name, ISStartDate, OSEndDate);
        }


        private async Task LoadInValidator(string name, DateTime startDate, DateTime endDate)
        {
            var dataParams = Test.DataCalcContext.GetParams();

            if (SelectedStrategy != null)
            {
                var validatorViewModel = new ValidatorViewModel();
                await validatorViewModel.LoadByStrategy(name, SelectedStrategy, dataParams, startDate, endDate);
                var validatorWindow = new ValidatorWindow(validatorViewModel);
                validatorWindow.Show();
            }
        }

        private async Task SaveStrategyPreset(StrategyViewModel strategyViewModel, Preset preset, string name)
        {
            using (var context = Session.Instance.DbContextFactory.GetContext())
            {
                var repository = new PresetRepository(context);
                var validatorStrategyPreset = new ValidatorStrategyPreset(strategyViewModel.Strategy);

                if (preset == null)
                {
                    preset = new Preset()
                    {
                        Name = name,
                        Guid = strategyViewModel.Strategy.Guid.ToString(),
                        Type = (int) PresetType.Strategy,
                        Data = JsonHelper.ToJson(new PresetObject<ValidatorStrategyPreset>()
                            {Object = validatorStrategyPreset, PresetType = PresetType.Strategy})
                    };
                    await repository.Add(preset);
                }
                else
                {
                    preset.Data = JsonHelper.ToJson(new PresetObject<ValidatorStrategyPreset>()
                        {Object = validatorStrategyPreset, PresetType = PresetType.Strategy});
                    await repository.Update(preset);
                }

                await repository.CompleteAsync();
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}