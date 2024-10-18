using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Twm.Core.Classes;
using Twm.Core.DataCalc;
using Twm.Core.DataCalc.Optimization;
using Twm.Core.DataCalc.Performance;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Core.Market;
using Twm.Core.ViewModels;
using Twm.Windows.Charts;
using Twm.Windows.Strategies;

namespace Twm.ViewModels.Strategies
{
    public class StrategyViewModel : ViewModelBase
    {
        private readonly StrategyBase _dataModel;

        private StrategyPerformanceWindow _strategyPerformanceWindow;


        private StrategyPerformanceViewModel _strategyPerformanceViewModel;

        public ChartWindow ChartWindow { get; set; }


        public StrategyBase Strategy
        {
            get { return _dataModel; }
        }


        public Dictionary<string, object> StrategyValues
        {
            get { return Strategy.GetTwmPropertyValues(); }
        }


        public string Name
        {
            get { return _dataModel.Name; }
            set
            {
                if (_dataModel.Name != value)
                {
                    _dataModel.Name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string AccountName
        {
            get
            {
                if (Strategy.Account != null)
                    return Strategy.Account.AccountType.Description();
                return "";
            }
        }

        public string ConnectionName
        {
            get
            {
                if (Strategy.Account != null)
                    return Strategy.Account.Connection.Name;
                return "";
            }
        }

        public string Instrument
        {
            get { return _dataModel.Instrument.Symbol; }
        }

        public string Id
        {
            get { return _dataModel.LocalId; }
        }


        public string DataSeries
        {
            get { return _dataModel.DataSeries; }
        }


        public string Parameters
        {
            get { return _dataModel.Parameters; }
        }


        public string HintParameters
        {
            get { return _dataModel.HintParameters; }
        }


        public Position LastPosition
        {
            get { return _dataModel.LastPosition; }
        }

        public double? Realized
        {
            get { return _dataModel.Realized; }
        }

        public double? Unrealized
        {
            get { return _dataModel.Unrealized; }
        }


        public OptimizationFitness OptimizationFitness
        {
            get { return _dataModel.OptimizationFitness; }
        }


        #region IS Performance fields

        public double? ISNetProfit
        {
            get { return _dataModel.NetProfit; }
        }

        public double? ISCommissionValue
        {
            get { return _dataModel.CommissionValue; }
        }

        public double? ISProfitFactor
        {
            get { return _dataModel.ProfitFactor; }
        }

        public double? ISSharpe
        {
            get { return _dataModel.Sharpe; }
        }

        public int? ISEquityHighs
        {
            get { return _dataModel.EquityHighs; }
        }

        public double? ISMaxDrawDown
        {
            get { return _dataModel.MaxDrawDown; }
        }

        public double? ISMaxDrawDownDays
        {
            get { return _dataModel.MaxDrawDownDays; }
        }

        public DateTime? ISStartDate
        {
            get { return _dataModel.StartDate; }
        }

        public DateTime? ISEndDate
        {
            get { return _dataModel.EndDate; }
        }

        public int? ISMaxConsLoss
        {
            get { return _dataModel.MaxConsLoss; }
        }

        public int? ISMaxConsWins
        {
            get { return _dataModel.MaxConsWins; }
        }

        public int? ISTrades
        {
            get { return _dataModel.Trades; }
        }

        public double? ISWinPercent
        {
            get { return _dataModel.WinPercent; }
        }

        public int? ISAverageTradesInYear
        {
            get { return _dataModel.AverageTradesInYear; }
        }

        public int? ISAverageTradeDurationInDays
        {
            get { return _dataModel.AverageTradeDurationInDays; }
        }

        public double? ISAverageTradeProfit
        {
            get { return _dataModel.AverageTradeProfit; }
        }

        public double? ISAverageWinningTrade
        {
            get { return _dataModel.AverageWinningTrade; }
        }

        public double? ISLargestWinningTrade
        {
            get { return _dataModel.LargestWinningTrade; }
        }

        public double? ISAverageLoosingTrade
        {
            get { return _dataModel.AverageLoosingTrade; }
        }

        public double? ISLargestLoosingTrade
        {
            get { return _dataModel.LargestLoosingTrade; }
        }

        public object CustomValue
        {
            get { return _dataModel.CustomValue; }
        }

        #endregion

        #region OS Performance fields
        public double? OSNetProfit
        {
            get { return OsPerformance?.Summary?.GetValue(AnalyticalFeature.NetProfitSum); }
        }

        public double? OSCommissionValue
        {
            get { return OsPerformance?.Summary?.GetValue(AnalyticalFeature.Commission); }
        }

        public double? OSProfitFactor
        {
            get { return OsPerformance?.Summary?.GetValue(AnalyticalFeature.ProfitFactor); }
        }

        public double? OSSharpe
        {
            get { return OsPerformance?.Summary?.GetValue(AnalyticalFeature.Sharpe); }
        }

        public int? OSEquityHighs
        {
            get { return OsPerformance?.Summary?.GetValue<int>(AnalyticalFeature.EquityHighs); }
        }

        public double? OSMaxDrawDown
        {
            get { return OsPerformance?.Summary?.GetValue(AnalyticalFeature.MaxDrawDown); }
        }

        public double? OSMaxDrawDownDays
        {
            get { return OsPerformance?.Summary?.GetValue<double>(AnalyticalFeature.MaxDrawDownDays); }
        }

        public DateTime? OSStartDate
        {
            get { return OsPerformance?.Summary?.GetValue<DateTime>(AnalyticalFeature.StartDate); }
        }


        public DateTime? OSEndDate
        {
            get { return OsPerformance?.Summary?.GetValue<DateTime>(AnalyticalFeature.EndDate); }
        }

        public int? OSMaxConsLoss
        {
            get { return OsPerformance?.Summary?.GetValue<int>(AnalyticalFeature.MaxConsLoss); }
        }

        public int? OSMaxConsWins
        {
            get { return OsPerformance?.Summary?.GetValue<int>(AnalyticalFeature.MaxConsWins); }
        }

        public int? OSTrades
        {
            get { return OsPerformance?.Summary?.GetValue<int>(AnalyticalFeature.Trades); }
        }

        public double? OSWinPercent
        {
            get { return OsPerformance?.Summary?.GetValue(AnalyticalFeature.TradesInProfit); }
        }

        public int? OSAverageTradesInYear
        {
            get { return OsPerformance?.Summary?.GetValue<int>(AnalyticalFeature.AverageTradesInYear); }
        }

        public int? OSAverageTradeDurationInDays
        {
            get { return OsPerformance?.Summary?.GetValue<int>(AnalyticalFeature.AverageTradeDurationInDays); }
        }

        public double? OSAverageTradeProfit
        {
            get { return OsPerformance?.Summary?.GetValue<double>(AnalyticalFeature.AverageTradeProfit); }
        }

        public double? OSAverageWinningTrade
        {
            get { return OsPerformance?.Summary?.GetValue<double>(AnalyticalFeature.AverageWinningTrade); }
        }

        public double? OSLargestWinningTrade
        {
            get { return OsPerformance?.Summary?.GetValue<double>(AnalyticalFeature.LargestWinningTrade); }
        }

        public double? OSAverageLoosingTrade
        {
            get { return OsPerformance?.Summary?.GetValue<double>(AnalyticalFeature.AverageLoosingTrade); }
        }

        public double? OSLargestLoosingTrade
        {
            get { return OsPerformance?.Summary?.GetValue<double>(AnalyticalFeature.LargestLoosingTrade); }
        }

        #endregion

        #region Performance fields

        private string GetPerformanceField(double? isValue, double? osValue, string format = "F0")
        {
            if (osValue != null)
                return $"{(isValue ?? 0).ToString(format)} ({(osValue ?? 0).ToString(format)})";

            return (isValue ?? 0).ToString(format);
        }

        private string GetPerformanceField(DateTime? isValue, DateTime? osValue, string format = "F0")
        {
            if (osValue != null)
            {
                if (isValue != null)
                    return $"{((isValue??DateTime.MinValue).ToString("dd.MM.yyyy"))} ({(osValue??DateTime.MinValue).ToString("dd.MM.yyyy")})";

                return $"- ({(osValue ?? DateTime.MinValue).ToString("dd.MM.yyyy")})";
            }

            if (isValue != null)
                return $"{((isValue ?? DateTime.MinValue).ToString("dd.MM.yyyy"))} (-)";

            return "- (-)";
        }

        public double PerformanceValue
        {
            get { return _dataModel.PerformanceValue; }
        }

        public string NetProfit
        {
            get
            {
                return GetPerformanceField(ISNetProfit, OSNetProfit);
            }
        }

        public string CommissionValue
        {
            get
            {
                return GetPerformanceField(ISCommissionValue, OSCommissionValue);
            }
        }

        public string ProfitFactor
        {
            get
            {
                return GetPerformanceField(ISProfitFactor, OSProfitFactor, "F2");
            }
        }

        public string Sharpe
        {
            get { return GetPerformanceField(ISSharpe, OSSharpe, "F2"); }
        }

        public string EquityHighs
        {
            get { return GetPerformanceField(ISEquityHighs, OSEquityHighs); }
        }

        public string MaxDrawDown
        {
            get { return GetPerformanceField(ISMaxDrawDown, OSMaxDrawDown); }
        }

        public string MaxDrawDownDays
        {
            get { return GetPerformanceField(ISMaxDrawDownDays, OSMaxDrawDownDays); }
        }

        public string StartDate
        {
            get { return GetPerformanceField(ISStartDate, OSStartDate); }
        }


        public string EndDate
        {
            get { return GetPerformanceField(ISEndDate, OSEndDate); }
        }



        public string MaxConsLoss
        {
            get { return GetPerformanceField(ISMaxConsLoss, OSMaxConsLoss); }
        }

        public string MaxConsWins
        {
            get { return GetPerformanceField(ISMaxConsWins, OSMaxConsWins); }
        }

        public string Trades
        {
            get { return GetPerformanceField(ISTrades, OSTrades); }
        }

        public string WinPercent
        {
            get { return GetPerformanceField(ISWinPercent, OSWinPercent); }
        }



        public string AverageTradesInYear
        {
            get { return GetPerformanceField(ISAverageTradesInYear, OSAverageTradesInYear); }
        }

        public string AverageTradeDurationInDays
        {
            get { return GetPerformanceField(ISAverageTradeDurationInDays, OSAverageTradeDurationInDays); }
        }

        public string AverageTradeProfit
        {
            get { return GetPerformanceField(ISAverageTradeProfit, OSAverageTradeProfit); }
        }

        public string AverageWinningTrade
        {
            get { return GetPerformanceField(ISAverageWinningTrade, OSAverageWinningTrade); }
        }

        public string LargestWinningTrade
        {
            get { return GetPerformanceField(ISLargestWinningTrade, OSLargestWinningTrade); }
        }

        public string AverageLoosingTrade
        {
            get { return GetPerformanceField(ISAverageLoosingTrade, OSAverageLoosingTrade); }
        }

        public string LargestLoosingTrade
        {
            get { return GetPerformanceField(ISLargestLoosingTrade, OSLargestLoosingTrade); }
        }






        #endregion

        public State State
        {
            get { return _dataModel.State; }
            set
            {
                if (_dataModel.State != value)
                {
                    _dataModel.State = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Enabled
        {
            get { return _dataModel.Enabled; }
            set
            {
                if (_dataModel.Enabled != value)
                {
                    _dataModel.Enabled = value;

                    if (!_dataModel.Enabled)
                    {
                        if (IsPerformanceVisible)
                            TogglePerformance();


                        if (App.Strategies.IsPerformanceVisible &&
                            App.Strategies.PerformanceStrategies.Contains(_dataModel))
                        {
                            App.Strategies.PerformancePortfolioCommand.Execute(null);
                        }
                    }

                    OnPropertyChanged();
                }
            }
        }


        private StrategyPerformanceViewModel _performanceViewModel;

        public StrategyPerformanceViewModel PerformanceViewModel
        {
            get { return _performanceViewModel; }
            set
            {
                if (_performanceViewModel != value)
                {
                    _performanceViewModel = value;
                    OnPropertyChanged();
                }
            }
        }

      

        public Task CalcISPerformanceTask { get; set; }
        public Task CalcOSPerformanceTask { get; set; }
        public Task CalcSimPerformanceTask { get; set; }


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


        private bool _isPerformanceVisible;

        public bool IsPerformanceVisible
        {
            get { return _isPerformanceVisible; }
            set
            {
                if (_isPerformanceVisible != value)
                {
                    _isPerformanceVisible = value;
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


        public OperationCommand ShowChartCommand { get; set; }

        public StrategyViewModel(StrategyBase strategy)
        {
            _dataModel = strategy;
            SelectedTabIndex = 0;
            ShowChartCommand = new OperationCommand(ShowChart);
            _dataModel.PropertyChanged += _dataModel_PropertyChanged;


        }

        private void _dataModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LastPosition")
            {
                OnPropertyChanged("LastPosition");
            }
            else if (e.PropertyName == "Unrealized")
            {
                OnPropertyChanged("Unrealized");
            }
            else if (e.PropertyName == "Realized")
            {
                OnPropertyChanged("Realized");
            }
            else if (e.PropertyName == "Enabled")
            {
                OnPropertyChanged("Enabled");
            }
        }


        public void Clear()
        {
            _dataModel.PropertyChanged -= _dataModel_PropertyChanged;
        }

        public void Remove()
        {
            var dataCalcContext = _dataModel.GetDataCalcContext();
            var strategies = dataCalcContext.Strategies.OfType<ScriptBase>().ToList();
            strategies.Remove(_dataModel);
            dataCalcContext.SynchronizeStrategies(strategies);
            ChartWindow?.Destroy();
        }


        private void ShowChart(object obj)
        {
            if (ChartWindow != null && !ChartWindow.IsVisible)
            {
                ChartWindow.Visibility = Visibility.Visible;
            }
        }


        public async void TogglePerformance(StrategyPerformanceSection section = StrategyPerformanceSection.Summary, TradeSourceType tradeSourceType = TradeSourceType.Total)
        {
            if (_strategyPerformanceWindow == null ||
                _strategyPerformanceWindow.IsClosed)
            {
                var po = new PerformanceOptions()
                {
                    IsPortfolio = false,
                    ExcludeSections = new List<object>() { StrategyPerformanceSection.Chart}
                };

                _strategyPerformanceViewModel = new StrategyPerformanceViewModel(new[] { _dataModel }, null, po);
                await _strategyPerformanceViewModel.SelectTradeSourceType(tradeSourceType);
                _strategyPerformanceViewModel.CurrentSection = section;

                _strategyPerformanceWindow = new StrategyPerformanceWindow(_strategyPerformanceViewModel);
            }

            if (_strategyPerformanceWindow.IsVisible)
            {
                _strategyPerformanceWindow.Hide();
                IsPerformanceVisible = false;
            }
            else
            {
                await _strategyPerformanceViewModel.SelectTradeSourceType(tradeSourceType);
                _strategyPerformanceViewModel.CurrentSection = section;
                _strategyPerformanceWindow.Show();
                IsPerformanceVisible = true;
            }
        }


        public void Stop()
        {
            _dataModel.Stop();
            OnPropertyChanged("Enabled");
        }


        public void Start()
        {
            _dataModel.Start();
            OnPropertyChanged("Enabled");
        }

        public void UpdatePerformanceProperties()
        {
            OnPropertyChanged("PerformanceValue");
            OnPropertyChanged("NetProfit");

            OnPropertyChanged("CommissionValue");
            OnPropertyChanged("ProfitFactor");
            OnPropertyChanged("Sharpe");
            OnPropertyChanged("EquityHighs");
            OnPropertyChanged("MaxDrawDown");
            OnPropertyChanged("MaxDrawDownDays");
            OnPropertyChanged("StartDate");
            OnPropertyChanged("EndDate");
            OnPropertyChanged("Trades");
            OnPropertyChanged("WinPercent");
            OnPropertyChanged("MaxConsLoss");
            OnPropertyChanged("MaxConsWins");

            OnPropertyChanged("AverageTradesInYear");
            OnPropertyChanged("AverageTradeDurationInDays");
            OnPropertyChanged("AverageTradeProfit");
            OnPropertyChanged("AverageWinningTrade");
            OnPropertyChanged("LargestWinningTrade");
            OnPropertyChanged("AverageLoosingTrade");
            OnPropertyChanged("LargestLoosingTrade");

        }

    }
}