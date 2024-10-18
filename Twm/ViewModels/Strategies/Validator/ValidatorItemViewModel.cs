using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Twm.Chart.Classes;
using Twm.Core.Classes;
using Twm.Core.Controllers;
using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Performance;
using Twm.Core.Enums;
using Twm.Core.ViewModels;
using Twm.Core.ViewModels.DataSeries;
using Twm.ViewModels.Charts;

namespace Twm.ViewModels.Strategies.Validator
{
    public class ValidatorItemViewModel : ViewModelBase, ICloneable
    {

        public ValidatorItemViewModel InstrumentList { get; set; }

        public ObservableCollection<object> Items { get; set; }

        public bool IsPortfolio { get; set; }

        public DataCalcContext DataCalcContext { get; set; }

        public GraphType? CurrentGraphType { get; set; }



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
                    OnPropertyChanged("DisplayName");
                }
            }

        }

        private int? _num;
        public int? Num
        {
            get { return _num; }
            set
            {
                if (value != _num)
                {
                    _num = value;
                    OnPropertyChanged(); 
                    OnPropertyChanged("DisplayName");
                }
            }

        }

        public string DisplayName
        {
            get
            {
                if (IsPortfolio)
                {
                    return Name;
                }
                else
                {
                    if (Num != null)
                        return Num + " " +InstrumentSeriesParams.DisplayName;

                    return InstrumentSeriesParams.DisplayName;
                }
            } 

        }


        public bool IsInstrumentParamValid
        {
            get { return string.IsNullOrEmpty(InstrumentSeriesParams.Error); }
        }

        public Type ObjectType { get; set; }


        private DataSeriesParamsViewModel _instrumentSeriesParams;

        public DataSeriesParamsViewModel InstrumentSeriesParams
        {
            get { return _instrumentSeriesParams; }

            set
            {
                if (value != _instrumentSeriesParams)
                {
                    _instrumentSeriesParams = value;
                    OnPropertyChanged();
                    OnPropertyChanged("DisplayName");
                }
            }

        }


        private StrategyBase _strategy;
        public StrategyBase Strategy
        {
            get
            {
                if (IsPortfolio)
                {
                    return _strategy;
                }
                else
                {
                    if (InstrumentList.ApplyToAllInstruments)
                    {
                        if (InstrumentList.Strategy != null)
                        {
                            InstrumentList.Strategy.DataSeriesSeriesParams = InstrumentSeriesParams;
                        }

                        return InstrumentList.Strategy;
                    }
                    return _strategy;
                }
            }
            set
            {
                if (_strategy != value)
                {

                  
                    _strategy = value;
                    OnPropertyChanged();
                    ParentViewModel?.UpdateProperty("StrategyName");
                    ParentViewModel?.UpdateProperty("IsRunEnable");
                    DataCalcContext.Strategies.Clear();
                    if (_strategy != null)
                        DataCalcContext.Strategies.Add(_strategy);
                    IsValid = false;
                    foreach (var item in Items.OfType<ValidatorItemViewModel>())
                    {
                        item.IsValid = false;
                    }
                    
                }
            }
        }

      

        public void ReloadStrategy(StrategyBase strategy)
        {
            _strategy = strategy;
            OnPropertyChanged("Strategy");
            ParentViewModel?.UpdateProperty("StrategyName");
            ParentViewModel?.UpdateProperty("IsRunEnable");
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


        private bool _isValid;

        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    OnPropertyChanged();
                    ParentViewModel?.UpdateProperty("PerformanceVisibility");
                }
            }
        }


        public string StrategyName
        {
            get { return _strategy.DisplayName; }
            

        }

        private StrategyPerformanceViewModel _performanceViewModel;
        public StrategyPerformanceViewModel Performance
        {
            get { return _performanceViewModel; }
            set
            {
                if (_performanceViewModel != value)
                {
                    _performanceViewModel = value;
                    OnPropertyChanged();
                    ParentViewModel?.UpdateProperty("SelectedValidatorItem");
                    ParentViewModel?.UpdateProperty("PerformanceVisibility");
                }
            }
        }



        private bool _applyToAllInstruments;

        public bool ApplyToAllInstruments
        {
            get { return _applyToAllInstruments; }

            set
            {
                if (_applyToAllInstruments != value)
                {
                    _applyToAllInstruments = value;
                    IsValid = false;
                    OnPropertyChanged();
                    ParentViewModel?.UpdateProperty("IsRunEnable");
                    ParentViewModel?.UpdateProperty("StrategyName");
                    ParentViewModel?.UpdateProperty("IsStrategySelectEnable");
                    foreach (var item in Items.OfType<ValidatorItemViewModel>())
                    {
                        item.UpdateProperty("Strategy");
                        item.IsValid = false;
                    }
                }
            }
        }


        private CancellationTokenSource _ctsCalc;

        public OperationCommand RunCommand { get; set; }

        public ValidatorItemViewModel()
        {
            Items = new ObservableCollection<object>();
            InstrumentSeriesParams = new DataSeriesParamsViewModel();
            InstrumentSeriesParams.PropertyChanged += InstrumentParam_PropertyChanged;
            DataCalcContext = new DataCalcContext(){IsValidation = true};

            if (!Session.Instance.DataCalcContexts.Contains(DataCalcContext))
                Session.Instance.DataCalcContexts.Add(DataCalcContext);
            RunCommand = new OperationCommand(RunStrategy);


        }

        private void InstrumentParam_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("IsInstrumentParamValid");
        }

        private void RunStrategy(object obj)
        {
            _ctsCalc = new CancellationTokenSource();
            Task.Run(() => RunStrategy(_ctsCalc.Token), _ctsCalc.Token);
            
        }

        private async  Task RunStrategy(CancellationToken cancellationToken)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ParentViewModel.IsBusy = true;
                ParentViewModel.Message = "Calculating performance...";
                ParentViewModel.SubMessage = "";
            });

            try
            {
                if (IsPortfolio)
                {
                    var count = Items.Count;
                    var num = 1;
                    var strategies = new List<StrategyBase>();
                    foreach (var item in Items.OfType<ValidatorItemViewModel>())
                    {
                        item.Num = num;
                        var originalStrategy = item.DataCalcContext.Strategies.FirstOrDefault();

                        StrategyBase strategy;
                        if (ApplyToAllInstruments)
                        {
                            item.DataCalcContext.Strategies.Clear();
                            var portfolioStrategy = DataCalcContext.Strategies.FirstOrDefault();
                            strategy = (StrategyBase)portfolioStrategy.Clone();
                            strategy.SetLocalId();
                            strategy.SetDataCalcContext(item.DataCalcContext);
                            strategy.CopyPanePlots(portfolioStrategy);
                            item.DataCalcContext.Strategies.Add(strategy);
                        }
                        else
                        {
                            if (item.Strategy != originalStrategy)
                                item.Strategy = originalStrategy;
                            strategy = item.Strategy;
                        }


                        if (strategy != null)
                        {
                            await ExecuteStrategy(item, strategy, cancellationToken);
                            strategy.Enabled = true;
                            strategy.Tag = num;
                            strategies.Add(strategy);
                        }
                        else
                        {
                            item.IsValid = false;
                        }

                        if (ApplyToAllInstruments)
                        {
                            if (originalStrategy != null)
                            {
                                item.DataCalcContext.Strategies.Clear();
                                item.DataCalcContext.Strategies.Add(originalStrategy);
                            }
                        }

                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            ParentViewModel.SubMessage = $"Instrument {num} of {count}";
                        });

                        num++;
                    }

                    if (strategies.Any())
                    {

                        var po = new PerformanceOptions()
                        {
                            IsPortfolio = true,
                            ParentViewModel = this,
                            ExcludeSections = new List<object> {StrategyPerformanceSection.Orders, StrategyPerformanceSection.Chart }
                        };

                        Performance = new StrategyPerformanceViewModel(strategies.ToArray(),null, po);
                        await Performance.Calculate(cancellationToken);
                        IsValid = true;
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            ParentViewModel.SubMessage = $"Portfolio";
                        });
                        Application.Current.Dispatcher.Invoke(() =>
                            Performance.SummaryView.Refresh()
                        );


                    }

                }
                else
                {

                    var originalStrategy = DataCalcContext.Strategies.FirstOrDefault();

                    StrategyBase strategy;
                    if (InstrumentList.ApplyToAllInstruments)
                    {
                        DataCalcContext.Strategies.Clear();
                        var portfolioStrategy = InstrumentList.DataCalcContext.Strategies.FirstOrDefault();
                        strategy = (StrategyBase)portfolioStrategy.Clone();
                        strategy.SetDataCalcContext(DataCalcContext);
                        strategy.CopyPanePlots(portfolioStrategy);
                        strategy.Init();
                        DataCalcContext.Strategies.Add(strategy);
                    }
                    else
                    {
                        if (Strategy != originalStrategy)
                            Strategy = originalStrategy;
                        strategy = Strategy;

                    }

                    await ExecuteStrategy(this, strategy, cancellationToken);
                    

                    strategy.Enabled = true;

                    if (InstrumentList.ApplyToAllInstruments)
                    {
                        if (originalStrategy != null)
                        {
                            DataCalcContext.Strategies.Clear();
                            DataCalcContext.Strategies.Add(originalStrategy);
                        }
                    }

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Execute strategy script error: " + "\r\n" + ex.Message);
                LogController.Print(ex.Message);
                LogController.Print(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    LogController.Print(ex.InnerException.StackTrace);
                }
            }

            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ParentViewModel.IsBusy = false;
                });
            }

        }


        public StrategyBase GetOriginalStrategy()
        {
            return _strategy;
        }

        public async Task ExecuteStrategy(ValidatorItemViewModel item, StrategyBase strategy, CancellationToken cancellationToken)
        {
            
            {
                if (strategy.Instrument == null)
                {
                    strategy.SetDataCalcContext(item.DataCalcContext);
                }

                if (strategy.GetDataCalcContext() == item.DataCalcContext)
                {
                    item.DataCalcContext.Strategies.Clear();
                    //strategy.SetDataCalcContext(item.DataCalcContext);
                    item.DataCalcContext.Strategies.Add(strategy);
                }

                strategy.Enabled = true;

                var isRerun = item?.Performance != null;
                var selectedItem = item?.Performance?.CurrentSection;
                var visibleRange = IntRange.Undefined;
                var candleWidth = item?.Performance?.ChartViewModel.Chart.CandleWidth;
                var candleGap = item?.Performance?.ChartViewModel.Chart.CandleGap;

                if (isRerun)
                {
                    Application.Current.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal, (Action) delegate
                        {
                            // Update UI component here
                            visibleRange =
                                new IntRange(item.Performance.ChartViewModel.Chart.VisibleCandlesRange.Start_i,
                                    item.Performance.ChartViewModel.Chart.VisibleCandlesRange.Count);
                        });
                }

                ;



                item.Performance =
                    new StrategyPerformanceViewModel(new[] {strategy}, null,
                        new PerformanceOptions()
                        {
                           
                            ParentViewModel = this
                        })
                    {


                        ChartViewModel = new ChartViewModel(item.InstrumentSeriesParams, item.DataCalcContext)
                    };
                item.DataCalcContext.Chart = item.Performance.ChartViewModel.Chart;
                strategy.SetChart(item.Performance.ChartViewModel.Chart);
                //strategy.ViewModel = item.Performance.ChartViewModel;
                strategy.ViewModel = ParentViewModel;



                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    strategy.SynchronizeChart(false);
                });

                /*Application.Current.Dispatcher.Invoke(

                    System.Windows.Threading.DispatcherPriority.Normal,
                    (Action) delegate { strategy.SynchronizeChart(false); }

                );*/

                await item.DataCalcContext.ExecuteStrategy(_ctsCalc);
                await item.Performance.Calculate(cancellationToken);

                Application.Current.Dispatcher.Invoke(()=>
                item.Performance.SummaryView.Refresh()
                );



                item.IsValid = true;

                await Task.Delay(200);
                Application.Current.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal, (Action) delegate
                    {
                        if (isRerun)
                        {
                            item.Performance.SelectSection(selectedItem);
                            Task.Run(async () => { await Task.Delay(200); }, cancellationToken).ContinueWith(t =>
                            {
                                item.Performance.ChartViewModel.Chart.VisibleCandlesRange = visibleRange;
                                item.Performance.ChartViewModel.Chart.InitialCandleGap = candleGap.Value;
                                item.Performance.ChartViewModel.Chart.InitialCandleWidth = candleWidth.Value;
                                item.Performance.ChartViewModel.Chart.IsLoaded = false;
                                item.Performance.ChartViewModel.Chart.OnLoad();

                            }, TaskScheduler.FromCurrentSynchronizationContext());
                        }
                    });
            }
           

        }

       

        public void Invalid()
        {
            ValidatorItemViewModel instrumentList = this;
            if (!IsPortfolio)
            {
                instrumentList = InstrumentList;
            }


            instrumentList.IsValid = false;
            var validatorIems = instrumentList.Items.OfType<ValidatorItemViewModel>();
            foreach (var item in validatorIems)
            {
                item.IsValid = false;
            }
        }

        public object Clone()
        {
            var obj = (ValidatorItemViewModel)MemberwiseClone();
            obj.InstrumentSeriesParams = (DataSeriesParamsViewModel)InstrumentSeriesParams.Clone();
            return obj;
        }
    }
}