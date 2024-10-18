using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Twm.Core.Controllers;
using Twm.Core.DataCalc.Performance;
using Twm.Core.Enums;
using Twm.Core.Interfaces;
using Twm.Core.ViewModels;
using Twm.ViewModels.Strategies.Optimizer;

namespace Twm.ViewModels.Strategies.Performance.RiskLevels
{
    public class RiskLevelsViewModel : ViewModelBase
    {
        private readonly OptimizerTestViewModel _optimizerTestViewModel;

        public ObservableCollection<RiskLevelViewModel> RiskLevels { get; set; }

        public ObservableCollection<PeriodRiskLevelViewModel> PeriodRiskLevels { get; set; }

        public Dictionary<AnalyticalFeature, bool> AnalyticalFeatureChecked { get; set; }

        public ObservableCollection<SimSummaryItemViewModel> SimSummaryItems { get; set; }


        


        private bool _isChecked;

        public bool ISChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    _osChecked = !value;
                    OnPropertyChanged();
                    OnPropertyChanged("OSChecked");
                }
            }
        }


        private bool _osChecked;

        public bool OSChecked
        {
            get { return _osChecked; }
            set
            {
                if (_osChecked != value)
                {
                    _osChecked = value;
                    _isChecked = !value;
                    OnPropertyChanged();
                    OnPropertyChanged("ISChecked");
                }
            }
        }


        private bool _applyRiskLevels;

        public bool ApplyRiskLevels
        {
            get { return _applyRiskLevels; }
            set
            {
                if (_applyRiskLevels != value)
                {
                    _applyRiskLevels = value;
                    DoApply(value);
                    OnPropertyChanged();
                }
            }
        }


        private bool _applyGlobalRiskLevels;

        public bool ApplyGlobalRiskLevels
        {
            get { return _applyGlobalRiskLevels; }
            set
            {
                if (_applyGlobalRiskLevels != value)
                {
                    _applyGlobalRiskLevels = value;
                    DoGlobalApply(value);
                    OnPropertyChanged();
                }
            }
        }

        public RiskLevelsViewModel(OptimizerTestViewModel test)
        {
            _optimizerTestViewModel = test;
            RiskLevels = new ObservableCollection<RiskLevelViewModel>
            {
                new RiskLevelViewModel(AnalyticalFeature.MaxDrawDown),
                new RiskLevelViewModel(AnalyticalFeature.MaxDrawDownDays),
                new RiskLevelViewModel(AnalyticalFeature.MaxConsLoss),
                new RiskLevelViewModel(AnalyticalFeature.LargestLoosingTrade),
                new RiskLevelViewModel(AnalyticalFeature.MaxMae)
            };

            PeriodRiskLevels = new ObservableCollection<PeriodRiskLevelViewModel>();
            SimSummaryItems = new ObservableCollection<SimSummaryItemViewModel>();
            ISChecked = true;
        }

        private List<Task> _periodTasks;

        private async void DoApply(bool value)
        {
            if (value)
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                PeriodRiskLevels.Clear();

                _optimizerTestViewModel.StartPerformanceCalc = false;

                if (_optimizerTestViewModel.ParentViewModel is OptimizerViewModel optimizerViewModel)
                {
                    optimizerViewModel.ApplyRiskLevels = true;
                }

                AnalyticalFeatureChecked = RiskLevels.ToDictionary(x => x.Feature, y => y.IsChecked);
                OnPropertyChanged("AnalyticalFeatureChecked");

                _periodTasks = new List<Task>();

                foreach (var period in _optimizerTestViewModel.Periods)
                {
                    if (period.OsPerformance == null || period.IsLive)
                        continue;


                    var osRiskLevels = new List<IRiskLevel>();
                    var simRiskLevels = new List<IRiskLevel>();
                    foreach (var painLevel in RiskLevels)
                    {
                        if (painLevel.IsChecked)
                        {
                            var newRiskLevel = (IRiskLevel) painLevel.Clone();
                            newRiskLevel.SetPainValue(period.IsPerformance.Summary);
                            osRiskLevels.Add(newRiskLevel);

                            if (SystemOptions.Instance.CalculateSimulation)
                            {
                                newRiskLevel = (IRiskLevel)painLevel.Clone();
                                newRiskLevel.SetPainValue(period.IsPerformance.Summary);
                                simRiskLevels.Add(newRiskLevel);
                            }
                        }
                    }

                    var periodRiskLevelViewModel = new PeriodRiskLevelViewModel() {Name = period.PeriodName};
                    periodRiskLevelViewModel.InitValues(RiskLevels.ToList<IRiskLevel>(), osRiskLevels, simRiskLevels,
                        period.IsPerformance.Summary);

                    PeriodRiskLevels.Add(periodRiskLevelViewModel);


                    period.ApplyRiskLevels = true;

                    period.OsPerformance.IsLoaded = false;
                    period.SelectedStrategy.CalcOSPerformanceTask =
                        period.OsPerformance.ReCalculate(cancellationTokenSource.Token, osRiskLevels);


                    Task totalTask;
                    if (SystemOptions.Instance.CalculateSimulation)
                    {
                        period.SimPerformance.IsLoaded = false;
                        period.SelectedStrategy.CalcSimPerformanceTask =
                            period.SimPerformance.ReCalculate(cancellationTokenSource.Token, simRiskLevels);

                        totalTask = period.CalcTotalPerformance(period.SelectedStrategy, cancellationTokenSource.Token, true).ContinueWith(
                            t => SetValues(periodRiskLevelViewModel, period.OsPerformance.StartTradeCount,
                                period.OsPerformance.Summary, period.SimPerformance.StartTradeCount,
                                period.SimPerformance.Summary),
                            TaskContinuationOptions.OnlyOnRanToCompletion); ;
                    }
                    else
                    {
                        totalTask = period.CalcTotalPerformance(period.SelectedStrategy, cancellationTokenSource.Token, true).ContinueWith(
                            t => SetValues(periodRiskLevelViewModel, period.OsPerformance.StartTradeCount,
                                period.OsPerformance.Summary),
                            TaskContinuationOptions.OnlyOnRanToCompletion); ;
                    }

                    

                    _periodTasks.Add(totalTask);
                }

                await Task.WhenAll(_periodTasks);
                _periodTasks.Clear();
                foreach (var painLevel in RiskLevels)
                {
                    if (painLevel.IsChecked)
                    {
                        var isSum = PeriodRiskLevels.Sum(x => x.ISValues[painLevel.Feature] ?? 0);
                        painLevel.ISValue = isSum / _optimizerTestViewModel.Periods.Count;

                        var osSum = PeriodRiskLevels.Sum(x => x.OSValues[painLevel.Feature] ?? 0);
                        painLevel.OSValue = osSum / _optimizerTestViewModel.Periods.Count;

                        if (SystemOptions.Instance.CalculateSimulation)
                        {
                            var simSum = PeriodRiskLevels.Sum(x => x.SimValues[painLevel.Feature] ?? 0);
                            painLevel.SimValue = simSum / _optimizerTestViewModel.Periods.Count;
                        }
                    }
                }

                if (SystemOptions.Instance.CalculateSimulation)
                {
                    SimSummaryItems.Clear();

                    var simPassPercent = (PeriodRiskLevels.Sum(x => x.IsCheck ? 1.0 : 0) / PeriodRiskLevels.Count) * 100;

                    SimSummaryItems.Add(new SimSummaryItemViewModel() { Name = "% Pass", Value = simPassPercent });

                    var maxSimTrades = PeriodRiskLevels.Max(x => x.PainTrades);

                    SimSummaryItems.Add(new SimSummaryItemViewModel() { Name = "Max trades", Value = maxSimTrades });

                    var minSimTrades = PeriodRiskLevels.Min(x => x.PainTrades);

                    SimSummaryItems.Add(new SimSummaryItemViewModel() { Name = "Min trades", Value = minSimTrades });

                    var failedSimPeriods = PeriodRiskLevels.Where(x => !x.IsCheck).ToList();
                    var avgTradesBeforeRisk = failedSimPeriods.Sum(x => x.PainTrades) / failedSimPeriods.Count;

                    SimSummaryItems.Add(new SimSummaryItemViewModel() { Name = "Average Trades before Risk", Value = avgTradesBeforeRisk });

                    var maxDDSim = PeriodRiskLevels.Min(x => x.SimValues[AnalyticalFeature.MaxDrawDown] ?? 0);
                    SimSummaryItems.Add(new SimSummaryItemViewModel() { Name = "Max DD", Value = maxDDSim });

                    var avgDDSim = PeriodRiskLevels.Sum(x => x.SimValues[AnalyticalFeature.MaxDrawDown] ?? 0) / PeriodRiskLevels.Count;
                    SimSummaryItems.Add(new SimSummaryItemViewModel() { Name = "Average Max DD", Value = avgDDSim });
                }


            }
            else
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                _optimizerTestViewModel.StartPerformanceCalc = false;

                if (_optimizerTestViewModel.ParentViewModel is OptimizerViewModel optimizerViewModel)
                {
                    optimizerViewModel.ApplyRiskLevels = false;
                }
                _periodTasks.Clear();

                foreach (var period in _optimizerTestViewModel.Periods)
                {
                    if (period.OsPerformance == null || period.IsLive)
                        continue;

                    period.ApplyRiskLevels = false;

                    period.OsPerformance.IsLoaded = false;
                    period.SelectedStrategy.CalcOSPerformanceTask =
                        period.OsPerformance.ReCalculate(cancellationTokenSource.Token);
                    if (SystemOptions.Instance.CalculateSimulation)
                    {
                        period.SimPerformance.IsLoaded = false;
                        period.SelectedStrategy.CalcSimPerformanceTask =
                            period.SimPerformance.ReCalculate(cancellationTokenSource.Token);
                    }


                    var totalTask = period.CalcTotalPerformance(period.SelectedStrategy, cancellationTokenSource.Token, true);
                    _periodTasks.Add(totalTask);

                }

                await Task.WhenAll(_periodTasks);

                _optimizerTestViewModel.CalculateEvent.Wait(cancellationTokenSource.Token);
                foreach (var painLevel in RiskLevels)
                {
                    if (painLevel.IsChecked)
                    {
                        painLevel.ISValue = null;
                        painLevel.OSValue = null;
                        painLevel.PainValue = 0;
                    }
                }
            }
            _optimizerTestViewModel.Performance.RefreshSection();
            
        }

        /*private void SetOSValue(PeriodRiskLevelViewModel periodRiskLevel, int osTradeCount, Summary summary)
        {
            periodRiskLevel.SetOSValues(osTradeCount, summary);
        }


        private void SetSimValue(PeriodRiskLevelViewModel periodRiskLevel, int simTradeCount, Summary summary)
        {
            periodRiskLevel.SetSimValues(simTradeCount, summary);
        }*/

        private void SetValues(PeriodRiskLevelViewModel periodRiskLevel, int osTradeCount, Summary osSummary, int simTradeCount = 0, Summary simSummary = null)
        {
            periodRiskLevel.SetValues(osTradeCount, osSummary, simTradeCount, simSummary);
        }


        private async void DoGlobalApply(bool value)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            if (value)
            {
                _optimizerTestViewModel.StartPerformanceCalc = false;

                var painLevels = new List<IRiskLevel>();
                foreach (var painLevel in RiskLevels)
                {
                    if (painLevel.IsChecked)
                    {
                        var newRiskLevel = (IRiskLevel) painLevel.Clone();
                        if (painLevel.IsChecked)
                            newRiskLevel.SetGlobalPainValue(painLevel.ISValue ?? 0);
                        else
                            newRiskLevel.SetGlobalPainValue(painLevel.OSValue ?? 0);
                        painLevels.Add(newRiskLevel);
                    }
                }


                await _optimizerTestViewModel.CalcTotalPerformance(cancellationTokenSource.Token, true, painLevels);

                var globalPaintTrades =
                    _optimizerTestViewModel.Performance.Summary.GetValue<int>(AnalyticalFeature.Trades);

                int tradeCount = 0;
                foreach (var periodRiskLevel in PeriodRiskLevels)
                {
                    if (globalPaintTrades - periodRiskLevel.PainTrades >= 0)
                    {
                        tradeCount += periodRiskLevel.PainTrades;
                        periodRiskLevel.GlobalPainTrades = tradeCount;
                        periodRiskLevel.IsGlobalCheck = true;
                        globalPaintTrades = globalPaintTrades - periodRiskLevel.PainTrades;
                    }
                    else
                    {
                        periodRiskLevel.GlobalPainTrades = tradeCount + globalPaintTrades;
                        periodRiskLevel.IsGlobalCheck = false;
                        break;
                    }
                }
            }
            else
            {
                _optimizerTestViewModel.StartPerformanceCalc = false;
                await _optimizerTestViewModel.CalcTotalPerformance(cancellationTokenSource.Token, true);

                foreach (var periodRiskLevel in PeriodRiskLevels)
                {
                    periodRiskLevel.GlobalPainTrades = null;
                    periodRiskLevel.IsGlobalCheck = false;
                }
            }
        }
    }
}