using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Twm.Chart.Annotations;
using Twm.Chart.Classes;
using Twm.Core.Classes;
using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Performance;
using Twm.Core.Enums;
using Twm.Core.Interfaces;
using Twm.Core.Market;
using Twm.Core.ViewModels;
using Twm.ViewModels.Charts;
using Twm.ViewModels.Strategies.Optimizer;
using Twm.ViewModels.Strategies.Performance.Analysis;
using Twm.ViewModels.Strategies.Performance.RiskLevels;
using Twm.ViewModels.Strategies.Validator;
using Newtonsoft.Json;

namespace Twm.ViewModels.Strategies
{
    public class StrategyPerformanceViewModel : SystemPerformance, INotifyPropertyChanged, IDisposable
    {
        public bool IsLoaded { get; set; }
        public ViewModelBase ParentViewModel { get; set; }


        [JsonIgnore]
        public ICollectionView TradesView { get; set; }


        [JsonIgnore]
        public ICollectionView SummaryView { get; set; }


        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isVirtualizing;
        public bool IsVirtualizing
        {
            get { return _isVirtualizing; }
            set
            {
                if (_isVirtualizing != value)
                {
                    _isVirtualizing = value;
                    OnPropertyChanged();
                }
            }
        }

        private VirtualizationMode _virtualizationMode;
        public VirtualizationMode VirtualizationMode
        {
            get { return _virtualizationMode; }
            set
            {
                if (_virtualizationMode != value)
                {
                    _virtualizationMode = value;
                    OnPropertyChanged();
                }
            }
        }
        

        private string _message;

        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _subMessage;


        public string SubMessage
        {
            get { return _subMessage; }
            set
            {
                if (_subMessage != value)
                {
                    _subMessage = value;
                    OnPropertyChanged();
                }
            }
        }


        public string Name { get; private set; }


        private object _selectedTrade;

        public object SelectedTrade
        {
            get { return _selectedTrade; }

            set
            {
                if (_selectedTrade != value)
                {
                    _selectedTrade = value;
                    OnPropertyChanged();
                }
            }
        }

        private object _selectedOrder;

        public object SelectedOrder
        {
            get { return _selectedOrder; }

            set
            {
                if (_selectedOrder != value)
                {
                    _selectedOrder = value;
                    OnPropertyChanged();
                }
            }
        }


       


        public object SectionModel
        {
            get
            {
                if (CurrentSection == null)
                    return null;

             

                if (CurrentSection != StrategyPerformanceSection.Chart)
                    return this;

                




                return ChartViewModel;
            }
        }


        private AnalysisViewModel _analysis;

        public AnalysisViewModel Analysis
        {
            get { return _analysis; }
            set
            {
                if (_analysis!= value)
                {
                    _analysis = value;
                    OnPropertyChanged();
                }
            }

        }

        public OperationCommand SelectTradesCommand { get; private set; }


        public ChartViewModel ChartViewModel { get; set; }

        public RiskLevelsViewModel RiskLevelsViewModel { get; set; }

        public bool IsOptimizerTestParent
        {
            get
            {
                if (ParentViewModel is OptimizerTestViewModel optimizerTestViewModel)
                {
                    return true;
                }

                return false;
            }
        }


        public bool IsShowTradeSource
        {
            get
            {
                if (ParentViewModel == null)
                {
                    return true;
                }

                return false;
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
                    SelectSection(value);
                    if (ParentViewModel != null)
                    {
                        if (ParentViewModel is ValidatorItemViewModel validatorItemViewModel)
                        {
                            if (validatorItemViewModel.ParentViewModel is ValidatorViewModel validatorViewModel)
                            {
                                validatorViewModel.CurrentSection = value;
                            }
                        }
                        if (ParentViewModel is OptimizerPeriodViewModel optimizerPeriodViewModel)
                        {
                            optimizerPeriodViewModel.CurrentSection = value;
                            if (optimizerPeriodViewModel.ParentViewModel is OptimizerViewModel optimizerViewModel)
                            {
                                optimizerViewModel.CurrentSection = value;
                            }
                        }

                        if (ParentViewModel is OptimizerTestViewModel optimizerTestViewModel)
                        {
                            if (optimizerTestViewModel.ParentViewModel is OptimizerViewModel optimizerViewModel)
                            {
                                optimizerViewModel.CurrentSection = value;
                            }
                        }
                    }
                }
            }
        }


        private TradeSourceType? _currentTradeSourceType;
        public TradeSourceType? CurrentTradeSourceType
        {
            get { return _currentTradeSourceType; }
            set
            {
                if (_currentTradeSourceType != value)
                {
                     SelectTradeSourceType(value);
                }
            }
        }

        public ObservableCollection<StrategyPerformanceSection> Sections { get; set; }

        public ObservableCollection<TradeSourceType> TradeSourceTypes { get; set; }


        public StrategyBase Strategy
        {
            get { return Strategies.FirstOrDefault(); }
        }



        public StrategyPerformanceViewModel(StrategyBase[] strategies, ITrade[] trades, PerformanceOptions po) : base(strategies, trades, po.IsPortfolio)
        {
            IsVirtualizing = true;
            VirtualizationMode = VirtualizationMode.Recycling;
            TradesView = CollectionViewSource.GetDefaultView(Trades);
            Name = SetName(strategies, po.IsPortfolio);
            ParentViewModel = po.ParentViewModel;


            if (Strategies != null)
                foreach (var strategy in Strategies)
                {
                    strategy.PropertyChanged += StrategyPropertyChanged;
                }

            FillSections(po.ExcludeSections);
            _currentSection = StrategyPerformanceSection.Summary;
            if (po.CreateAnalysis)
            {
                Analysis = new AnalysisViewModel(IsPortfolio) {ParentViewModel = po.ParentViewModel};
            }

            



            FillTradeSourceTypes();
            _currentTradeSourceType = TradeSourceType.Total;

            SelectTradesCommand = new OperationCommand(SelectTrades);
            OnPropertyChanged("IsOptimizerTestParent");

            SummaryView = CollectionViewSource.GetDefaultView(Summary.SummaryItems);
        }


        private void SelectTrades(object parameter)
        {
            var trade = SelectedTrade as Trade;
            if (trade == null)
            {
                return;
            }

            var chartViewModel = ChartViewModel;
            if (ParentViewModel is OptimizerTestViewModel optimizerTestViewModel)
            {
                if (optimizerTestViewModel.ParentViewModel is OptimizerViewModel optimizerViewModel)
                {
                    if (optimizerViewModel.IsOptimizerViewer)
                        return;

                    var period = optimizerTestViewModel.Periods.FirstOrDefault(x=>x.Number == trade.PeriodNum);

                    if (period != null)
                    {
                        optimizerViewModel.SelectedOptimizerItem = period;

                        period.SelectedTabIndex = 1;
                        chartViewModel = period.OsPerformance.ChartViewModel;
                        period.OsPerformance.CurrentSection = StrategyPerformanceSection.Chart;
                    }
                }
            }
            else
            {
                CurrentSection = StrategyPerformanceSection.Chart;
            }

            Task.Run(() =>
            {
                Task.Delay(650);
            }).ContinueWith(task =>
            {
                chartViewModel.Chart.VisibleCandlesRange = new IntRange((int)(trade.EntryBar) - 1, chartViewModel.Chart.VisibleCandlesRange.Count);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }


        private void MoveToTrades()
        {

        }


        private string SetName(StrategyBase[] strategies, bool isPortfolio)
        {
            var name = "Performance";

            if (isPortfolio)
            {
                name = "Portfolio strategy performance";
            }

            if (strategies != null && strategies.Any())
            {
                name = "Strategy performance: " + Strategies[0].FullName;
            }

            return name;
        }

        private void FillSections(List<object> excludeSections)
        {
            Sections = new ObservableCollection<StrategyPerformanceSection>();

            var values = Enum.GetValues(typeof(StrategyPerformanceSection));

            foreach (var value in values)
            {
                if (!excludeSections.Contains(value))
                    Sections.Add((StrategyPerformanceSection) value);
            }
        }

        private void FillTradeSourceTypes()
        {
            TradeSourceTypes = new ObservableCollection<TradeSourceType>();

            var values = Enum.GetValues(typeof(TradeSourceType));

            foreach (var value in values)
            {
                TradeSourceTypes.Add((TradeSourceType)value);
            }
        }


        private async void StrategyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Enabled" && sender is StrategyBase strategy)
            {
                if (strategy.Enabled)
                {
                    await Calculate(new CancellationToken());
                }
                else
                {
                    Clear();
                    IsLoaded = false;
                }
            }
        }

        public override void Clear()
        {
            base.Clear();
            Analysis.Clear();
            if (Strategies != null)
                foreach (var strategy in Strategies)
                {
                    strategy.PropertyChanged -= StrategyPropertyChanged;
                }
        }

        public override void Destroy()
        {
            base.Destroy();
            Clear();
            Strategies = null;
            if (ChartViewModel != null)
            {
                ChartViewModel.Chart?.Destroy();
                ChartViewModel.Chart = null;
                ChartViewModel = null;
            }

        }

        public override async Task Calculate(CancellationToken cancellationToken, IEnumerable<IRiskLevel> painLevels = null, TradeSourceType? tradeSourceType = TradeSourceType.Total)
        {
            if (IsLoaded)
                return;

            if ((Strategies == null || !Strategies.Any()) && (AllTrades == null /*|| !AllTrades.Any()*/))
            {
                return;
            }

            await base.Calculate(cancellationToken, painLevels, tradeSourceType);

            #region Analysis

            var parameters = new AnalysisParameters();

            if (Strategies != null)
            {
                parameters.Strategies = Strategies.ToList();
                var strategy = Strategies.FirstOrDefault();
               
            }

            parameters.IsPortfolio = IsPortfolio;

            if (cancellationToken.IsCancellationRequested)
                return;
            
            Analysis?.FetchData(Trades.Where(x => x.IsClosed), parameters);
            

            #endregion


            #region Chart

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ChartViewModel != null)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    ChartViewModel.Chart.Symbol = Strategies[0].Instrument.Symbol;
                    double.TryParse(Strategies[0].Instrument.PriceIncrements.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var tickSize);
                    ChartViewModel.Chart.TickSize = tickSize;
                    BindingOperations.SetBinding(ChartViewModel.Chart,
                        Twm.Chart.Classes.Chart.CandlesSourceProperty,
                        new Binding()
                            {Source = ChartViewModel.DataCalcContext, Path = new PropertyPath("Candles")});

                    ChartViewModel.Chart.OnLoad();
                }
            });

            #endregion

            IsLoaded = true;
        }

        public  async Task ReCalculate(CancellationToken cancellationToken,
            IEnumerable<IRiskLevel> painLevels = null)
        {
            IsLoaded = false;
            base.Reset();
            Clear();
            await Calculate(cancellationToken, painLevels);
        }


        public void SelectSection(StrategyPerformanceSection? section)
        {
            if (_currentSection != section)
            {
                if (section == null || !Sections.Contains((StrategyPerformanceSection) section))
                    section = Sections.FirstOrDefault();

                _currentSection = section;
                
                OnPropertyChanged("CurrentSection");
                OnPropertyChanged("SectionModel");
            }
        }

        public async Task SelectTradeSourceType(TradeSourceType? tradeSourceType)
        {
            if (_currentTradeSourceType != tradeSourceType)
            {
                if (tradeSourceType == null || !TradeSourceTypes.Contains((TradeSourceType)tradeSourceType))
                    tradeSourceType = TradeSourceTypes.FirstOrDefault();

                _currentTradeSourceType = tradeSourceType;
                OnPropertyChanged("CurrentTradeSourceType");

                IsLoaded = false;
                Reset();
                Clear();
                await Calculate(new CancellationToken(), null, _currentTradeSourceType);

               //TODO: Fake update
                var cs = CurrentSection;
                _currentSection = null;
                OnPropertyChanged("CurrentSection");
                OnPropertyChanged("SectionModel");
                _currentSection = cs;
                OnPropertyChanged("CurrentSection");
                OnPropertyChanged("SectionModel");

            }
        }

        public void UpdateProperty(string name)
        {
            OnPropertyChanged(name);
        }


        public void RefreshSection()
        {
            if (CurrentSection == StrategyPerformanceSection.Summary)
            {
                Application.Current.Dispatcher.Invoke(() => { SummaryView.Refresh(); });
            }
            else if (CurrentSection == StrategyPerformanceSection.Trades)
            { 
                Application.Current.Dispatcher.Invoke(() => { TradesView.Refresh(); });
            }
        }

        public void Dispose()
        {
            foreach (var strategy in Strategies)
            {
                strategy.PropertyChanged -= StrategyPropertyChanged;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}