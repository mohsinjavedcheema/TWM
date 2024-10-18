using System;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Twm.Chart.Classes;
using Twm.Chart.DrawObjects;
using Twm.Chart.Interfaces;
using Twm.Core.Attributes;
using Twm.Core.Classes;
using Twm.Core.DataCalc.Commissions;
using Twm.Core.DataCalc.Optimization;
using Twm.Core.DataCalc.Performance;
using Twm.Core.DataProviders.ExchangeMockUp;
using Twm.Core.ViewModels.ScriptObjects;
using Twm.Core.Controllers;
using Twm.Core.CustomProperties.Editors;
using Twm.Core.Enums;
using Twm.Core.Market;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Task = System.Threading.Tasks.Task;
using OrderChangeEventArgs = Twm.Core.Classes.OrderChangeEventArgs;



namespace Twm.Core.DataCalc
{
    [DataContract]
    public abstract class StrategyBase : ScriptBase, ICloneable
    {
        private Position _lastPosition;

        /// <summary>
        /// Represents position related information that pertains to an instance of a strategy.
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public Position LastPosition
        {
            get { return _lastPosition; }
            set
            {
                if (_lastPosition != value)
                {
                    _lastPosition = value;
                    _lastMarketPosition = value.MarketPosition;
                    OnPropertyChanged();
                }
            }
        }


        private MarketPosition _lastMarketPosition;


        private double? _unrealized;

        [XmlIgnore]
        [Browsable(false)]
        public double? Unrealized
        {
            get { return _unrealized; }

            set
            {
                if (_unrealized != value)
                {
                    _unrealized = value;
                    OnPropertyChanged();
                    LastPosition.Unrealized = value;
                    // Account.UpdateUnrealized();
                }
            }
        }


        private double? _realized;

        [XmlIgnore]
        [Browsable(false)]
        public double? Realized
        {
            get { return _realized; }

            set
            {
                if (_realized != value)
                {
                    _realized = value;
                    OnPropertyChanged();
                    //Account?.UpdateRealized();
                }
            }
        }


        private Account _account;

        [Browsable(false)]
        public Account Account
        {
            get { return _account; }

            set
            {
                if (_account != value)
                {
                    _account = value;
                    OnPropertyChanged();
                }
            }
        }


        private List<Account> _accounts;

        [Category("Main")]
        [Display(Name = "Account", GroupName = "Main", Order = 3)]
        [Visible(false, PropertyVisibility.Validator, PropertyVisibility.ShortValidator, PropertyVisibility.Optimizer)]
        public List<Account> Accounts
        {
            get
            {
                if (_accounts == null)
                    _accounts = new List<Account>();
                var selectedAccount = Account;
                _accounts.Clear();

                foreach (var account in Session.Instance.Accounts)
                {
                    if (!account.IsActive || account.IsPlayback)
                        continue;

                    if (account.Connection.Id == Connection.Id || account.AccountType != AccountType.LocalPaper)
                    {
                        _accounts.Add(account);
                    }
                }

                Account = selectedAccount;

                if (!_accounts.Contains(Account))
                    Account = _accounts.FirstOrDefault();
                return _accounts;
            }
        }

        public void CalculateLivePositionStatistics(ICandle tick)
        {
            Unrealized = GetUnrealizedProfitLoss(tick);
        }

        public double GetUnrealizedProfitLoss(ICandle tick)
        {
            if (_lastMarketPosition != MarketPosition.Flat)
            {
                var openTrades = LastPosition.Trades.Where(x => !x.IsClosed).ToList();

                var profitSum = 0d;

                for (int i = 0; i < openTrades.Count; i++)
                {
                    var profit = 0d;

                    if (openTrades[i].MarketPosition == MarketPosition.Long)
                    {
                        profit = (tick.C - openTrades[i].EntryPrice) * Multiplier;
                    }

                    if (openTrades[i].MarketPosition == MarketPosition.Short)
                    {
                        profit = (openTrades[i].EntryPrice - tick.C) * Multiplier;
                    }

                    profitSum += profit;
                }

                return profitSum;
            }

            return 0;
        }

        #region Quationable methods

        [XmlIgnore]
        [Browsable(false)]
        public double LastPositionAverageEntryPrice
        {
            get { return LastPosition.Trades.Average(e => e.EntryPrice); }
        }

        [XmlIgnore]
        [Browsable(false)]
        public bool WorkingStopMarketOrdersExist
        {
            get
            {
                return _orderBook.Any(o => o.OrderState == OrderState.Working &&
                                           o.OrderType == OrderType.StopMarket);
                ;
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        public bool WorkingLimitOrdersExist
        {
            get
            {
                return _orderBook.Any(o => o.OrderState == OrderState.Working &&
                                           o.OrderType == OrderType.Limit);
                ;
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        public int WorkingOrderInitBarNumber
        {
            get
            {
                var orders = _orderBook.Where(o => o.OrderState == OrderState.Working).ToList();

                if (orders.Any())
                {
                    return orders[orders.Count - 1].OrderInitBarNumber;
                }

                return 0;
            }
        }

        #endregion

        public void CancelOrder(Order order, bool isRemove = false)
        {
            if (order.OrderState == OrderState.Cancelled || order.OrderState == OrderState.Filled)
                return;

            order.OrderState = OrderState.Cancelled;

            if (State == State.Playback || State == State.RealTime)
            {
                if (Account.AccountType == AccountType.LocalPaper)
                {
                    Session.Instance.GetMockup(Connection).CancelOrder((Order)order.Clone(), isRemove);
                }
                else if (Account.AccountType == AccountType.ServerPaper || Account.AccountType == AccountType.Broker)
                {
                    Print("STRATEGYBASE cancelling order GUID: " + order.Guid +
                          " Name: " + order.Name +
                          " State: " + order.OrderState +
                          " Qnt: " + order.Quantity);

                    Account.CancelOrder(order.Guid);
                }
            }
        }


        [Browsable(false)][XmlIgnore] public ObservableCollection<Position> Positions { get; private set; }


        [Browsable(false)]
        public string Parameters
        {
            get
            {
                var values = string.Join(", ", TwmPropertyValues.Values.Select(x => x.ToString()));
                return $"({values})";
            }
        }

        [Browsable(false)]
        public string HintParameters
        {
            get
            {
                var propertyValues = GetTwmPropertyValues();

                var result = "";
                foreach (var propertyValue in propertyValues)
                {
                    result = result + propertyValue.Key + ": " + propertyValue.Value + "\r\n";
                }


                return result.TrimEnd('\r', '\n');
            }
        }


        private bool _enabled;

        [Category("Main")]
        [Visible(false, PropertyVisibility.Validator, PropertyVisibility.ShortValidator)]
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    StartStopStrategy(value);
                    OnPropertyChanged();
                }
            }
        }


        private string _testName;

        [Category("Main")]
        [Visible(false, PropertyVisibility.Everywhere)]
        public string TestName
        {
            get { return _testName; }
        }


       

       

        private CancellationTokenSource _ctsExecuteScript;


        [Browsable(false)] public List<ISeriesValue<double>> Data { get; set; }


        [Browsable(false)]
        public string DisplayName
        {
            get
            {
                var values = string.Join(", ", TwmPropertyValues.Values.Select(x =>
                (x == null) ? "" : x.ToString()
                ));
                return $"{GetType().Name}({values})";
            }
        }

        [Browsable(false)]
        public string FullName
        {
            get { return $"{DisplayName} {Instrument.Symbol}{DataSeries}"; }
        }

        #region Commissions

        private ScriptObjectItemViewModel _commissionType;

        [Browsable(false)]
        public ScriptObjectItemViewModel CommissionType
        {
            get { return _commissionType; }
            set
            {
                if (value != _commissionType)
                {
                    _commissionType = value;
                    CreateCommission();
                    OnPropertyChanged();
                }
            }
        }



        public void CreateCommission()
        {
            if (CommissionType != null)
            {
                dynamic commission = Activator.CreateInstance(CommissionType.ObjectType);
                Commission = commission;
            }
        }


        [Category("Commissions")]
        [Display(Name = "Type", GroupName = "Commissions", Order = 1)]
        public List<ScriptObjectItemViewModel> CommissionTypes
        {
            get { return BuildController.Instance.CommissionTypes; }
        }

        private Commission _commission;

        [ExpandableObject]
        [Category("Commissions")]
        [Display(Name = "Parameters", GroupName = "Commissions", Order = 2)]
        [NotifyParentProperty(true)]
        public Commission Commission
        {
            get { return _commission; }
            set
            {
                if (_commission != value)
                {
                    _commission = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion       

        #region Optimization properties

        private ScriptObjectItemViewModel _optimizerType;

        [Browsable(false)]
        public ScriptObjectItemViewModel OptimizerType
        {
            get { return _optimizerType; }
            set
            {
                if (value != _optimizerType)
                {
                    _optimizerType = value;
                    CreateOptimizer();
                    OnPropertyChanged();
                }
            }
        }

        public void CreateOptimizer(bool copyParameters = true)
        {
            if (OptimizerType != null)
            {
                dynamic optimizer = Activator.CreateInstance(OptimizerType.ObjectType);
                ObservableCollection<OptimizerParameter> parameters = null;
                int? keepBestNumber = null;
                int? taskBatchSize = null;
                int? minTrades = null;
                double? drawDownLevel = null;
                if (Optimizer != null)
                {
                    if (copyParameters)
                        parameters = Optimizer.OptimizerParameters;
                    keepBestNumber = Optimizer.KeepBestNumber;
                    taskBatchSize = Optimizer.TaskBatchSize;
                    minTrades = Optimizer.MinTrades;
                    drawDownLevel = Optimizer.DrawDownLevel;
                }

                Optimizer = optimizer;
                Optimizer.Init(this, parameters);

                if (keepBestNumber != null)
                {
                    Optimizer.KeepBestNumber = (int)keepBestNumber;
                }

                if (taskBatchSize != null)
                {
                    Optimizer.TaskBatchSize = (int)taskBatchSize;
                }

                if (minTrades != null)
                {
                    Optimizer.MinTrades = (int)minTrades;
                }

                if (drawDownLevel != null)
                {
                    Optimizer.DrawDownLevel = (double)drawDownLevel;
                }
            }
        }


        [Category("Optimize")]
        [Display(Name = "Optimizer types", GroupName = "Optimize", Order = 1)]
        [Visible(true, PropertyVisibility.Optimizer)]
        public List<ScriptObjectItemViewModel> OptimizerTypes
        {
            get { return BuildController.Instance.OptimizerStrategyTypes; }
        }


        [Browsable(false)] public OptimizationFitness OptimizationFitness { get; set; }


        [Browsable(false)] public ScriptObjectItemViewModel OptimizationFitnessType { get; set; }

        [Category("Optimize")]
        [Display(Name = "Optimize on", GroupName = "Optimize", Order = 2)]
        [VisibleAttribute(true, PropertyVisibility.Optimizer)]
        public List<ScriptObjectItemViewModel> OptimizationFitnessTypes
        {
            get { return BuildController.Instance.OptimizationFitnessTypes; }
        }

        private Optimizer _optimizer;

        [Editor(typeof(OptimizerParametersEditor), typeof(OptimizerParametersEditor))]
        [Category("Optimize")]
        [ExpandableObject]
        [Display(Name = "Parameters", GroupName = "Optimize", Order = 3)]
        [Visible(true, PropertyVisibility.Optimizer)]
        public Optimizer Optimizer
        {
            get { return _optimizer; }
            set
            {
                if (_optimizer != value)
                {
                    _optimizer = value;
                    OnPropertyChanged();
                }
            }
        }


        [Category("Optimize")]
        [Display(Name = "Min trades IS", GroupName = "Optimize", Order = 4)]
        [Visible(true, PropertyVisibility.Optimizer)]
        public int MinTrades
        {
            get
            {
                if (Optimizer == null)
                {
                    return 0;
                }

                return Optimizer.MinTrades;
            }
            set
            {
                if (Optimizer != null)
                {
                    Optimizer.MinTrades = value;
                }
            }
        }

        [Category("Optimize")]
        [Display(Name = "DrawDown level", GroupName = "Optimize", Order = 5)]
        [Visible(true, PropertyVisibility.Optimizer)]
        public double DrawDownLevel
        {
            get
            {
                if (Optimizer == null)
                {
                    return 0.0;
                }

                return Optimizer.DrawDownLevel;
            }
            set
            {
                if (Optimizer != null)
                {
                    Optimizer.DrawDownLevel = value;
                }
            }
        }


        [Category("Optimize")]
        [Display(Name = "Keep best strategy", GroupName = "Optimize", Order = 6)]
        [Visible(true, PropertyVisibility.Optimizer)]
        public int KeepBestNumber
        {
            get
            {
                if (Optimizer == null)
                {
                    return 10;
                }

                return Optimizer.KeepBestNumber;
            }
            set
            {
                if (Optimizer != null)
                {
                    Optimizer.KeepBestNumber = value;
                    OnPropertyChanged(nameof(KeepBestNumber));
                }
            }
        }

        [Category("Optimize")]
        [Display(Name = "Task batch size", GroupName = "Optimize", Order = 7)]
        [Visible(true, PropertyVisibility.Optimizer)]
        public int TaskBatchSize
        {
            get
            {
                if (Optimizer == null)
                {
                    return 10;
                }

                return Optimizer.TaskBatchSize;
            }
            set
            {
                if (Optimizer != null)
                {
                    Optimizer.TaskBatchSize = value;
                }
            }
        }

        public void CreateOptimizationFitness()
        {
            if (OptimizationFitnessType != null)
            {
                dynamic optimizationFitness = Activator.CreateInstance(OptimizationFitnessType.ObjectType);
                OptimizationFitness = optimizationFitness;
            }
        }


        private int _optimizerParamsHashCode;

        [Browsable(false)]
        public int OptimizerParamsHashCode
        {
            get { return _optimizerParamsHashCode; }
        }

        #endregion

        #region Performance fields

        [Browsable(false)]
        public double? NetProfit
        {
            get { return SystemPerformance?.Summary?.GetValue(AnalyticalFeature.NetProfitSum); }
        }


        [Browsable(false)]
        public double? CommissionValue
        {
            get { return SystemPerformance?.Summary?.GetValue(AnalyticalFeature.Commission); }
        }

        [Browsable(false)]
        public double? ProfitFactor
        {
            get { return SystemPerformance?.Summary?.GetValue(AnalyticalFeature.ProfitFactor); }
        }

        [Browsable(false)]
        public double? Sharpe
        {
            get { return SystemPerformance?.Summary?.GetValue(AnalyticalFeature.Sharpe); }
        }

        [Browsable(false)]
        public int? EquityHighs
        {
            get { return SystemPerformance?.Summary?.GetValue<int>(AnalyticalFeature.EquityHighs); }
        }


        [Browsable(false)]
        public double? MaxDrawDown
        {
            get { return SystemPerformance?.Summary?.GetValue(AnalyticalFeature.MaxDrawDown); }
        }

        [Browsable(false)]
        public double? MaxDrawDownDays
        {
            get { return SystemPerformance?.Summary?.GetValue<double>(AnalyticalFeature.MaxDrawDownDays); }
        }


        [Browsable(false)]
        public DateTime? StartDate
        {
            get { return SystemPerformance?.Summary?.GetValue<DateTime>(AnalyticalFeature.StartDate); }
        }

        [Browsable(false)]
        public DateTime? EndDate
        {
            get { return SystemPerformance?.Summary?.GetValue<DateTime>(AnalyticalFeature.EndDate); }
        }


        [Browsable(false)]
        public int? MaxConsLoss
        {
            get { return SystemPerformance?.Summary?.GetValue<int>(AnalyticalFeature.MaxConsLoss); }
        }

        [Browsable(false)]
        public int? MaxConsWins
        {
            get { return SystemPerformance?.Summary?.GetValue<int>(AnalyticalFeature.MaxConsWins); }
        }


        [Browsable(false)]
        public int? Trades
        {
            get { return SystemPerformance?.Summary?.GetValue<int>(AnalyticalFeature.Trades); }
        }

        [Browsable(false)]
        public double? WinPercent
        {
            get { return SystemPerformance?.Summary?.GetValue(AnalyticalFeature.TradesInProfit); }
        }

        [Browsable(false)]
        public int? AverageTradesInYear
        {
            get { return SystemPerformance?.Summary?.GetValue<int>(AnalyticalFeature.AverageTradesInYear); }
        }

        [Browsable(false)]
        public int? AverageTradeDurationInDays
        {
            get { return SystemPerformance?.Summary?.GetValue<int>(AnalyticalFeature.AverageTradeDurationInDays); }
        }


        [Browsable(false)]
        public double? AverageTradeProfit
        {
            get { return SystemPerformance?.Summary?.GetValue(AnalyticalFeature.AverageTradeProfit); }
        }


        [Browsable(false)]
        public double? AverageWinningTrade
        {
            get { return SystemPerformance?.Summary?.GetValue(AnalyticalFeature.AverageWinningTrade); }
        }

        [Browsable(false)]
        public double? LargestWinningTrade
        {
            get { return SystemPerformance?.Summary?.GetValue(AnalyticalFeature.LargestWinningTrade); }
        }

        [Browsable(false)]
        public double? AverageLoosingTrade
        {
            get { return SystemPerformance?.Summary?.GetValue(AnalyticalFeature.AverageLoosingTrade); }
        }


        [Browsable(false)]
        public double? LargestLoosingTrade
        {
            get { return SystemPerformance?.Summary?.GetValue(AnalyticalFeature.LargestLoosingTrade); }
        }

        [Browsable(false)]
        public double PerformanceValue
        {
            get { return OptimizationFitness.Value; }
        }

        private object _customValue;

        [Browsable(false)]
        public object CustomValue
        {
            get { return _customValue; }
            set
            {
                if (_customValue != value)
                {
                    _customValue = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        [Browsable(false)] public SystemPerformance SystemPerformance { get; set; }


        protected StrategyBase()
        {
            _values = new List<ISeries<double>>();
            OptimizerType =
                BuildController.Instance.OptimizerStrategyTypes.FirstOrDefault(x => x.Name == "Default Optimizer");
            OptimizationFitnessType = BuildController.Instance.OptimizationFitnessTypes.FirstOrDefault();
           
            CommissionType = BuildController.Instance.CommissionTypes.FirstOrDefault(x => x.Name == "No commission");
            Account = Session.Instance.Accounts.FirstOrDefault();

            SetLocalId();
            InitializePositions();
        }

        private void InitializePositions()
        {
            Positions = new ObservableCollection<Position>();
            Positions.CollectionChanged += Positions_CollectionChanged;


            //Add fake position
            Positions.Add(new Position()
            {
                MarketPosition = MarketPosition.Flat,
                Trades = new List<Trade>(),
                EnabledStrategyGui = LocalId
            });
        }

       

       

        private void Positions_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                LastPosition = e.NewItems.Cast<Position>().LastOrDefault();
                /*if ((State == State.Playback || State == State.RealTime) && LastPosition != null)
                { 
                    //Account.AddPosition(LastPosition);
                    LastPosition.PropertyChanged += LastPosition_PropertyChanged;
                }*/
            }
        }

        /*private void LastPosition_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MarketPosition" || e.PropertyName == "Quantity")
            {
                OnPropertyChanged("LastPosition");
                if (sender is Position position)
                {
                    if (position.Name == "")
                    {
                        Account.RemovePosition(position);
                    }
                }
            }
        }*/

        public void SetLocalId()
        {
            LocalId = Guid.NewGuid().ToString();
        }


        /// <summary>
        /// Executed before indicator set state Configured
        /// </summary>
        public override void BeforeConfigured()
        {
        }


        private void StartStopStrategy(bool start)
        {
            if (IsTemporary)
                return;

            if (start)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }


        private Timer _heartbeatTimer;

        private int _dataCheckCounter;

        public async void Start()
        {
            DataCalcContext.CreateToken();


            Task.Run(() => ExecuteScript(), DataCalcContext.CancellationTokenSourceCalc.Token).ContinueWith(
                x => { State = State.Calculated; }, TaskContinuationOptions.OnlyOnRanToCompletion);

            LogController.Print(
                $"Strategy enabled: Name: {Name}; Symbol: {Instrument.Symbol}; TimeFrame: {DataSeries}; LocalId: {LocalId}; Guid: {Guid}");
           
        }


        public long _currentTickNo;
        private async void SendHeartbeat(object state)
        {
            if ( Account != null && !Account.IsPlayback)
            {

                _dataCheckCounter++;
                string warning = "";
                if (_dataCheckCounter == 15)
                {
                    _dataCheckCounter = 0;

                    if (DataCalcContext.CurrentTickNo != _currentTickNo)
                    {
                        _currentTickNo = DataCalcContext.CurrentTickNo;
                    }
                    else
                    {
                        //Warning
                        warning = "Warning: Data is not updated";
                    }

                }
                
            }
        }


      

        public void Stop()
        {
            _ctsExecuteScript?.Cancel();
            DataCalcContext?.CancelCalc();
            DataCalcContext?.ClearStrategyObjects(this);
            State = State.Undefined;
            Chart?.ReCalc_VisibleCandlesExtremums();
            Chart?.ChartControl.Invalidate();
            Reset();
            Realized = null;
            

            if (Connection != null)
                Session.Instance.GetMockup(Connection).OnOrderStatusChanged -= OnOrderUpdateMockUp;
            //Session.Instance.GetMockup(Connection).OnPositionChanged -= ExchangeMockUp_OnPositionChanged;
            //Session.Instance.GetMockup(Connection).OnAccountChanged -= ExchangeMockUp_OnAccountChanged;

            if (Account != null)
            {
                Account.OnOrdersChanged -= OnOrderUpdateServer;
                Account.OnPositionsChanged -= OnPositionsUpdateServer;
            }


            if ((!DataCalcContext.IsValidation && !DataCalcContext.IsOptimization))
                LogController.Print(
                    $"Strategy disabled: Name: {Name}; Symbol: {Instrument.Symbol}; TimeFrame: {DataSeries}; LocalId: {LocalId}; Guid: {Guid}");
        }

        private void OnOrderUpdateMockUp(object sender, ExchangeOrderStatusEventArgs e)
        {
            HandlePositions(e.Order);
        }

        private void HandlePositions(Order incomingOrder)
        {
            lock (_lockObj)
            {

                var order = _orderBook.FirstOrDefault(x => x.Guid == incomingOrder.Guid);

                if (order == null)
                    return;

                order.FillPrice = incomingOrder.FillPrice;
                order.OrderFillDate = incomingOrder.OrderFillDate;
                order.Quantity = incomingOrder.Quantity;
                order.RealId = incomingOrder.RealId;
                order.OrderState = incomingOrder.OrderState;
            }

            if (incomingOrder.OrderState != OrderState.Filled)
                return;


            if (incomingOrder.OrderType == OrderType.Market)
            {
                HandleMarketOrderPlaybackSimSubmission(incomingOrder);
            }
            else
            {
                if (!Positions.Any() || _lastMarketPosition == MarketPosition.Flat)
                {
                    CreateNewPosition(incomingOrder);
                    OnExecutionUpdate(incomingOrder);
                }
                else if (_lastMarketPosition != MarketPosition.Flat)
                {
                    if (incomingOrder.Quantity >= LastPosition.Quantity)
                    {
                        CloseAll(incomingOrder);
                    }
                    else if (incomingOrder.Quantity < LastPosition.Quantity)
                    {
                        PartialClose(incomingOrder);
                    }

                    OnExecutionUpdate(incomingOrder);
                }
            }

        }

        private async Task ExecuteScript()
        {
            await Chart?.Dispatcher.InvokeAsync(() =>
            {
                if (ViewModel != null)
                {
                    ViewModel.IsBusy = true;
                    ViewModel.Message = "Calculating...";
                }
            }
            );
            try
            {
                await DataCalcContext.ExecuteStrategy(DataCalcContext.CancellationTokenSourceCalc);
                await Chart?.Dispatcher.InvokeAsync(() =>
                {
                    Chart?.ReCalc_VisibleCandlesExtremums();
                    Chart?.ChartControl?.Invalidate();
                }
                );
            }
            catch (Exception ex)
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
                await Chart?.Dispatcher.InvokeAsync(() =>
                {
                    if (ViewModel != null)
                    {
                        ViewModel.IsBusy = false;
                    }
                }
                );
            }
        }


        public DataCalcContext GetDataCalcContext()
        {
            return DataCalcContext;
        }


        public bool EqualsInput(ISeries<double> series)
        {
            if (Input == null && series == null)
                return true;

            if (Input != null && series == null)
                return false;

            if (Input == null && series != null)
                return false;

            if (Input.Count != series.Count)
                return false;

            for (int i = 0; i < Input.Count; i++)
            {
                if (Input.GetValueAt(i) != series.GetValueAt(i))
                    return false;
            }

            return true;
        }


        public object Clone()
        {
            var strategy = (StrategyBase)MemberwiseClone();

            strategy.Commission = (Commission)Commission.Clone();
          

            strategy.PanePlots = new Dictionary<FrameworkElement, List<Plot>>();

            foreach (var panePlot in PanePlots)
            {
                strategy.PanePlots.Add(panePlot.Key, panePlot.Value);
            }


            strategy._values = new List<ISeries<double>>();

            strategy._lockObj = new object();

            strategy.CreateOptimizationFitness();

            return strategy;
        }


        public override void OnAfterBarUpdate()
        {
            foreach (var value in _values)
            {
                value.SetValid(0);
            }
        }

        public virtual void OnOrderUpdate(Order order)
        {
        }

        public override void Reset()
        {
            base.Reset();

            if (State == State.Playback || State == State.RealTime)
            {
                if (_orderBook != null)
                {
                    foreach (var order in _orderBook)
                    {
                        CancelOrder(order, true);
                    }
                }
            }

            if (Account != null)
            {
                var posForRemove = Account.Positions.Where(i => i.EnabledStrategyGui == LocalId).ToList();

                foreach (var position in posForRemove)
                {
                    Account.Positions.Remove(position);
                }
            }




            InitializePositions();


            if (_orderBook != null)
                foreach (var order in _orderBook)
                {
                    order.PropertyChanged -= Order_PropertyChanged;

                }

            _orderBook = new List<Order>();
            _workingOrders = new List<Order>();
            _internalPositionNumber = 0;
            _internalOrderId = 0;
            _internalTradeNumber = 1;
            Chart?.Reset();
            Unrealized = null;
        }

        private List<Order> _workingOrders;

        public override void Clear()
        {
            RemovePlots();
            RemovePanes();
            PanePlots.Clear();
        }


        public virtual void Init()
        {
        }


        public override string ToString()
        {
            return DisplayName;
        }


        public IEnumerable<Trade> GetAllTrades(TradeSourceType? tradeSourceType = TradeSourceType.Total)
        {
            if (tradeSourceType == TradeSourceType.Total || tradeSourceType == null)
                return Positions.SelectMany(x => x.Trades);

            if (tradeSourceType == TradeSourceType.Historical)
                return Positions.Where(x => !x.IsRealTime).SelectMany(x => x.Trades);

            return Positions.Where(x => x.IsRealTime).SelectMany(x => x.Trades);
        }

        public void ClearTrades(DateTime date)
        {
            foreach (var position in Positions)
            {
                position.Trades.RemoveAll(x => x.EntryTime < date);
            }
        }


        public IEnumerable<Trade> GetLastTrades()
        {
            var trades = Positions.SelectMany(x => x.Trades).Where(x => x.IsClosed && !x.IsProcessed).ToList();
            trades.ForEach(x => x.IsProcessed = true);
            return trades;
        }


        public IEnumerable<Order> GetOrders(TradeSourceType? tradeSourceType = TradeSourceType.Total)
        {
            if (_orderBook == null)
                return new List<Order>();

            if (tradeSourceType == TradeSourceType.Total)
                return _orderBook;

            if (tradeSourceType == TradeSourceType.Historical)
                return _orderBook.Where(x => x.OrderEnvironment == OrderEnvironment.LocalHistorical);

            return _orderBook.Where(x => x.OrderEnvironment != OrderEnvironment.LocalHistorical);
        }

        public void CalculateMaeMfe()
        {
            if (_lastMarketPosition == MarketPosition.Flat)
                return;

            var trades = LastPosition.Trades;
            if (!trades.Any())
                return;

            var curHigh = High[0];
            var curLow = Low[0];

            var openTrade = trades.FirstOrDefault(x => !x.IsClosed);

            if (openTrade != null)
            {
                if (openTrade.MaxHighPriceWhileOpen < curHigh)
                    openTrade.MaxHighPriceWhileOpen = curHigh;

                if (openTrade.MinLowPriceWhileOpen == 0)
                {
                    openTrade.MinLowPriceWhileOpen = curLow;
                }
                else if (openTrade.MinLowPriceWhileOpen > curLow)
                {
                    openTrade.MinLowPriceWhileOpen = curLow;
                }
            }
        }

        #region Histroical Order Routing

        private int _internalOrderId;
        private int _internalPositionNumber;
        private int _internalTradeNumber;
        private List<Order> _orderBook;
        private object _lockObj = new object();

        public void BeforeBarUpdate()
        {
            CheckOrderBook();
        }


        private void CheckOrderBook()
        {
            if (State != State.Historical)
                return;


            if (!_workingOrders.Any())
                return;

            if (!Positions.Any() || _lastMarketPosition == MarketPosition.Flat)
            {
                NoPositionCheckOrders(_workingOrders);
            }
            else if (_lastMarketPosition != MarketPosition.Flat)
            {
                ActivePositionCheckStopOrders(_workingOrders);
                ActivePositionCheckLimitOrders(_workingOrders);
            }
        }

        private bool CurrentBarIsUp()
        {
            var open = DataCalcContext.Candles[DataCalcContext.CurrentBar - 1].O;
            var close = DataCalcContext.Candles[DataCalcContext.CurrentBar - 1].C;

            if (open < close)
                return true;

            return false;
        }

        private List<Order> GetWorkingStopOrders()
        {
            return _orderBook.Where(o => o.OrderState == OrderState.Working &&
                                         o.OrderType == OrderType.StopMarket).ToList();
        }

        private List<Order> GetWorkingLimitOrders()
        {
            return _orderBook.Where(o => o.OrderState == OrderState.Working &&
                                         o.OrderType == OrderType.Limit).ToList();
        }

        private double GetCurrentBarHigh()
        {
            return DataCalcContext.Candles[DataCalcContext.CurrentBar - 1].H;
        }

        private double GetCurrentBarLow()
        {
            return DataCalcContext.Candles[DataCalcContext.CurrentBar - 1].L;
        }

        

        private void NoPositionCheckOrders(List<Order> workingOrders)
        {
            for (int i = 0; i < workingOrders.Count; i++)
            {
                var order = workingOrders[i];

                if (order.OrderType == OrderType.StopMarket)
                {
                    if (order.OrderAction == OrderAction.Buy && order.HistoricalOrderDirection == HistoricalOrderDirection.BreakOut)
                    {
                        ProcessStopMarketOrder(order,  workingOrders, true);
                    }
                    else if (order.OrderAction == OrderAction.SellShort && order.HistoricalOrderDirection == HistoricalOrderDirection.PullBack)
                    {
                        ProcessStopMarketOrder(order,  workingOrders, true);
                    }
                    else if (order.OrderAction == OrderAction.SellShort && order.HistoricalOrderDirection == HistoricalOrderDirection.BreakOut)
                    {
                        ProcessStopMarketOrder(order,  workingOrders, false);
                    }
                    else if (order.OrderAction == OrderAction.Buy && order.HistoricalOrderDirection == HistoricalOrderDirection.PullBack)
                    {
                        ProcessStopMarketOrder(order,  workingOrders, false);
                    }
                    else if (order.HistoricalOrderDirection == HistoricalOrderDirection.Equal)
                    {
                         FillOrder(order, workingOrders, DataCalcContext.Candles[GetIndex(OrderType.Market)].O);
                    }
                }
                else if (order.OrderType == OrderType.Limit)
                {
                    if (order.OrderAction == OrderAction.Buy)
                    {
                        if (Low[0] <= order.LimitPrice && Open[0] >= order.LimitPrice)
                        {
                            FillOrder(order, workingOrders, order.LimitPrice);
                        }
                        else if (Open[0] < order.LimitPrice)
                        {
                            FillOrder(order, workingOrders, DataCalcContext.Candles[GetIndex(OrderType.Limit)].O);
                        }
                    }
                    else if (order.OrderAction == OrderAction.SellShort)
                    {
                        if (High[0] >= order.LimitPrice &&  Open[0] <= order.LimitPrice)
                        {
                            FillOrder(order, workingOrders, order.LimitPrice);
                        }
                        else if (Open[0] > order.LimitPrice)
                        {
                            FillOrder(order, workingOrders, DataCalcContext.Candles[GetIndex(OrderType.Limit)].O);
                        }
                    }

                }
                else if (order.OrderType == OrderType.StopLimit)
                {
                    //not supported for historical execution
                }
            }
        }

        private void ProcessStopMarketOrder(Order order, List<Order> workingOrders, bool marketGoingUp)
        {
            if (marketGoingUp)
            {
                if (High[0] >= order.TriggerPrice && Open[0] <  order.TriggerPrice)
                {
                    FillOrder(order, workingOrders, order.TriggerPrice);
                }
                else if (Open[0] >  order.TriggerPrice)
                {
                    FillOrder(order, workingOrders, DataCalcContext.Candles[GetIndex(OrderType.StopMarket)].O);
                }
            }
            else if (!marketGoingUp)
            {
                if (Low[0] <= order.TriggerPrice && Open[0] >  order.TriggerPrice)
                {
                    FillOrder(order, workingOrders, order.TriggerPrice);
                }
                else if (Open[0] <  order.TriggerPrice)
                {
                    FillOrder(order, workingOrders, DataCalcContext.Candles[GetIndex(OrderType.StopMarket)].O);
                }
            }
    
        }

        private void FillOrder(Order order, List<Order> workingOrders, double fillPrice)
        {
            order.FillPrice = fillPrice;
            order.OrderState = OrderState.Filled;
            CreateNewPosition(order);
            OnOrderUpdate(order);

            ActivePositionCheckStopOrders(workingOrders);
            ActivePositionCheckLimitOrders(workingOrders);
        }


        private void ActivePositionCheckStopOrders(List<Order> workingOrders)
        {
            if (_lastMarketPosition == MarketPosition.Long)
            {
                for (int i = 0; i < workingOrders.Count; i++)
                {
                    var order = workingOrders[i];
                    if (order.OrderType != OrderType.StopMarket)
                        continue;

                    if (order.OrderAction == OrderAction.Sell && Low[0] <= order.StopPrice)
                    {
                        order.FillPrice = order.StopPrice;
                        order.OrderState = OrderState.Filled;
                        order.OrderFillDate = DataCalcContext.Candles[GetIndex(OrderType.StopMarket)].t;

                        if (order.Quantity >= LastPosition.Quantity)
                        {
                            CloseAll(order);
                        }
                        else
                        {
                            PartialClose(order);
                        }

                        OnOrderUpdate(order);
                    }
                }
            }
            else if (_lastMarketPosition == MarketPosition.Short)
            {
                for (int i = 0; i < workingOrders.Count; i++)
                {
                    var order = workingOrders[i];
                    if (order.OrderType != OrderType.StopMarket)
                        continue;

                    if (order.OrderAction == OrderAction.BuyToCover && High[0] >= order.StopPrice)
                    {
                        order.FillPrice = order.StopPrice;
                        order.OrderState = OrderState.Filled;
                        order.OrderFillDate = DataCalcContext.Candles[GetIndex(OrderType.StopMarket)].t;

                        if (order.Quantity >= LastPosition.Quantity)
                        {
                            CloseAll(order);
                        }
                        else
                        {
                            PartialClose(order);
                        }

                        OnOrderUpdate(order);
                    }
                }
            }
        }

        private void ActivePositionCheckLimitOrders(List<Order> workingOrders)
        {
            if (_lastMarketPosition == MarketPosition.Long)
            {
                for (int i = 0; i < workingOrders.Count; i++)
                {
                    var order = workingOrders[i];
                    if (order.OrderType != OrderType.Limit)
                        continue;

                    if (order.OrderAction == OrderAction.Sell && High[0] >= order.LimitPrice)
                    {
                        order.FillPrice = order.LimitPrice;
                        order.OrderState = OrderState.Filled;
                        order.OrderFillDate = DataCalcContext.Candles[GetIndex(OrderType.Limit)].t;

                        if (order.Quantity >= LastPosition.Quantity)
                        {
                            CloseAll(order);
                        }
                        else if (order.Quantity < LastPosition.Quantity)
                        {
                            PartialClose(order);
                        }

                        OnOrderUpdate(order);
                    }
                    else if (order.OrderAction == OrderAction.Buy && Low[0] <= order.LimitPrice)
                    {
                        order.FillPrice = order.LimitPrice;
                        order.OrderState = OrderState.Filled;
                        order.OrderFillDate = DataCalcContext.Candles[GetIndex(OrderType.Limit)].t;
                        ScaleIn(order);
                        OnOrderUpdate(order);

                        
                    }
                }
            }
            else if (_lastMarketPosition == MarketPosition.Short)
            {
                for (int i = 0; i < workingOrders.Count; i++)
                {
                    var order = workingOrders[i];
                    if (order.OrderType != OrderType.Limit)
                        continue;

                    if (order.OrderAction == OrderAction.BuyToCover && Low[0] <= order.LimitPrice)
                    {
                        order.FillPrice = order.LimitPrice;
                        order.OrderState = OrderState.Filled;
                        order.OrderFillDate = DataCalcContext.Candles[GetIndex(OrderType.Limit)].t;

                        if (order.Quantity >= LastPosition.Quantity)
                        {
                            CloseAll(order);
                        }
                        else if (order.Quantity < LastPosition.Quantity)
                        {
                            PartialClose(order);
                        }

                        OnOrderUpdate(order);
                    }
                    else if (order.OrderAction == OrderAction.SellShort && High[0] >= order.LimitPrice)
                    {
                        order.FillPrice = order.LimitPrice;
                        order.OrderState = OrderState.Filled;
                        order.OrderFillDate = DataCalcContext.Candles[GetIndex(OrderType.Limit)].t;
                        ScaleIn(order);
                        OnOrderUpdate(order);

                        
                    }
                }
            }
        }

        public Order SubmitOrder(int selectedBarsInProgress, OrderAction orderAction, OrderType orderType, double quantity,
            double limitPrice, double stopPrice, double triggerPrice, string oco, string signalName, string comment = null)
        {
            var tickSize = TickSize;
            limitPrice = RoundToTickSize(limitPrice, tickSize);
            stopPrice = RoundToTickSize(stopPrice, tickSize);

            if (State == State.Playback || State == State.RealTime)
            {
                if (Account.AccountType == AccountType.LocalPaper)
                {
                    var orderPlayback = SubmitOrderPlaybackSim(selectedBarsInProgress, orderAction, orderType, quantity,
                        limitPrice, stopPrice, oco, signalName, comment = null);

                    Session.Instance.GetMockup(Connection).SubmitOrder((Order)orderPlayback.Clone());

                    return orderPlayback;
                }
                if (Account.AccountType == AccountType.ServerPaper || Account.AccountType == AccountType.Broker)
                {

                    var myOrder = Account.SubmitOrder(orderAction, quantity, signalName, Instrument, orderType, limitPrice, stopPrice, triggerPrice);

                    OrderBookAddOrder(myOrder);
                    OnOrderUpdate(myOrder);

                    return myOrder;
                }
            }


            var order = InitializeHistoricalOrder(selectedBarsInProgress, orderAction, orderType, quantity, limitPrice,
                stopPrice, triggerPrice,
                oco, signalName, comment);

            OrderBookAddOrder(order);
            OnOrderUpdate(order);

            if (order.OrderType == OrderType.Limit || order.OrderType == OrderType.StopMarket || order.OrderType == OrderType.StopLimit)
            {
                HandleStopOrLimitOrderSubmission();
            }

            if (order.OrderType == OrderType.Market)
            {
                HandleMarketOrderHistoricalSubmission(order);
            }

            return order;
        }

        public Order SubmitOrderPlaybackSim(int selectedBarsInProgress, OrderAction orderAction, OrderType orderType,
            double quantity,
            double limitPrice, double stopPrice, string oco, string signalName, string comment = null,
            bool isEntry = false)
        {
            var order = InitializePlayBackSimOrder(selectedBarsInProgress, orderAction, orderType, quantity, limitPrice,
                stopPrice,
                oco, signalName, comment, isEntry);

            lock (_lockObj)
            {
                OrderBookAddOrder(order);
                OnOrderUpdate(order);
            }


            return order;
        }


        private void OrderBookAddOrder(Order order)
        {
            _orderBook.Add(order);
            order.PropertyChanged += Order_PropertyChanged;
            if (State == State.Playback || State == State.RealTime)
            {
                //Account.AddOrder(order);
            }
        }

        private void Order_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "OrderState")
            {
                if (sender is Order order)
                {
                    if (order.OrderState == OrderState.Working)
                    {
                        _workingOrders.Add(order);
                    }
                    else if (_workingOrders.Contains(order))
                    {
                        _workingOrders.Remove(order);
                    }
                }
            }
        }

        private Order InitializeHistoricalOrder(int selectedBarsInProgress, OrderAction orderAction,
            OrderType orderType, double quantity,
            double limitPrice, double stopPrice, double triggerPrice, string oco, string signalName, string comment = null)
        {
            int index = GetIndex(orderType);
            int bar = index + 1;//GetBar(orderType);



            return new Order()
            {
                LimitPrice = limitPrice,
                StopPrice = stopPrice,
                TriggerPrice = triggerPrice,
                Oco = oco,
                Name = signalName,
                Comment = comment,
                Quantity = quantity,
                OrderType = orderType,
                OrderAction = orderAction,
                Id = _internalOrderId++,
                Guid = Guid.NewGuid().ToString(),
                OrderState = OrderState.Initialized,
                Instrument = DataCalcContext.Instruments[selectedBarsInProgress],
                OrderInitDate = DataCalcContext.Candles[index].t,
                OrderInitBarNumber = bar,
                OrderEnvironment = OrderEnvironment.LocalHistorical,
                EnabledStrategyGui = LocalId,
                HistoricalOrderDirection = GetOrderDirection(triggerPrice, orderType, orderAction)
            };

        }

        private HistoricalOrderDirection GetOrderDirection(double triggerPrice, OrderType orderType, OrderAction action)
        {
            var close = DataCalcContext.Candles[DataCalcContext.CurrentBar - 1].C;

            if (orderType == OrderType.StopMarket)
            {
                if (close < triggerPrice)
                {
                    if (action == OrderAction.Buy || action == OrderAction.BuyToCover)
                    {
                        return HistoricalOrderDirection.BreakOut;    
                    }
                    if (action == OrderAction.Sell || action == OrderAction.SellShort)
                    {
                        return HistoricalOrderDirection.PullBack;    
                    }
                }
                else if (close > triggerPrice)
                {
                    if (action == OrderAction.Buy || action == OrderAction.BuyToCover)
                    {
                        return HistoricalOrderDirection.PullBack;    
                    }
                    if (action == OrderAction.Sell || action == OrderAction.SellShort)
                    {
                        return HistoricalOrderDirection.BreakOut;    
                    }
                }
               
                return HistoricalOrderDirection.Equal; 

            }

            if (orderType == OrderType.StopLimit)
            {
                //not supported for historical execution
            }

            return HistoricalOrderDirection.Equal; 

        }

        private Order InitializePlayBackSimOrder(int selectedBarsInProgress, OrderAction orderAction,
            OrderType orderType, double quantity,
            double limitPrice, double stopPrice, string oco, string signalName, string comment = null,
            bool isEntry = false)
        {
            int index = GetIndex(OrderType.Market);
            int bar = index + 1;//GetBar(OrderType.Market);
            return new Order()
            {
                LimitPrice = limitPrice,
                StopPrice = stopPrice,
                Oco = oco,
                Name = signalName,
                Comment = comment,
                Quantity = quantity,
                OrderType = orderType,
                OrderAction = orderAction,
                Id = _internalOrderId++,
                Guid = Guid.NewGuid().ToString(),
                OrderState = OrderState.Initialized,
                Instrument = DataCalcContext.Instruments[selectedBarsInProgress],
                OrderInitDate = DataCalcContext.Candles[DataCalcContext.Candles.Count - 1].t,
                OrderInitBarNumber = bar,
                OrderEnvironment = OrderEnvironment.Realtime,
                EnabledStrategyGui = LocalId
            };


        }

        private void HandleMarketOrderHistoricalSubmission(Order order)
        {
            order = FillMarketOrder(order);

            if (!Positions.Any() || _lastMarketPosition == MarketPosition.Flat)
            {
                CreateNewPosition(order);
            }
            else if (_lastMarketPosition == MarketPosition.Long)
            {
                if (order.OrderAction == OrderAction.Buy)
                {
                    ScaleIn(order);
                }
                else if (order.OrderAction == OrderAction.Sell)
                {
                    ScaleOut(order);
                }
            }
            else if (_lastMarketPosition == MarketPosition.Short)
            {
                if (order.OrderAction == OrderAction.SellShort)
                {
                    ScaleIn(order);
                }
                else if (order.OrderAction == OrderAction.BuyToCover)
                {
                    ScaleOut(order);
                }
            }

            OnOrderUpdate(order);
        }

        private void HandleMarketOrderPlaybackSimSubmission(Order order)
        {
            if (Positions.Count == 0 || _lastMarketPosition == MarketPosition.Flat)
            {
                CreateNewPosition(order);
            }
            else if (_lastMarketPosition == MarketPosition.Long)
            {
                if (order.OrderAction == OrderAction.Buy)
                {
                    ScaleIn(order);
                }
                else if (order.OrderAction == OrderAction.Sell)
                {
                    ScaleOut(order);
                }
            }
            else if (_lastMarketPosition == MarketPosition.Short)
            {
                if (order.OrderAction == OrderAction.SellShort)
                {
                    ScaleIn(order);
                }
                else if (order.OrderAction == OrderAction.BuyToCover)
                {
                    ScaleOut(order);
                }
            }

            OnExecutionUpdate(order);
        }

        private Order FillMarketOrder(Order order)
        {
            var index = GetIndex(order.OrderType);
            var bar = index + 1;//GetBar(order.OrderType);

            order.FillPrice = DataCalcContext.Candles[index].O;

            //if entering on last historical bar use bar close price (open not yet available)
            if (CurrentBar == DataCalcContext.Candles.Count && State == State.Historical)
            {
                order.FillPrice = DataCalcContext.Candles[index].C;
            }

            order.OrderState = OrderState.Filled;
            order.OrderFillDate = DataCalcContext.Candles[index].t;
            order.OrderInitBarNumber = bar;

            //reversal market orders on historical backtest submited from OnOrderUpdate event
            if (order.StopPrice > 0)
            {
                order.FillPrice = order.StopPrice;
                order.OrderFillDate = DataCalcContext.Candles[DataCalcContext.CurrentBar - 1].t;
                order.OrderInitBarNumber = DataCalcContext.CurrentBar;
            }


            return order;
        }

        private void HandleStopOrLimitOrderSubmission()
        {
            _orderBook[_orderBook.Count - 1].OrderState = OrderState.Working;
            OnOrderUpdate(_orderBook[_orderBook.Count - 1]);
        }

        private void CreateNewPosition(Order order)
        {
            var pos = new Position()
            {
                Instrument = order.Instrument,
                Trades = new List<Trade>(),
                EnabledStrategyGui = LocalId
            };

            if (order.OrderAction == OrderAction.Buy)
            {
                pos.MarketPosition = MarketPosition.Long;
                _lastMarketPosition = pos.MarketPosition;
            }

            if (order.OrderAction == OrderAction.SellShort)
            {
                pos.MarketPosition = MarketPosition.Short;
                _lastMarketPosition = pos.MarketPosition;
            }

            int index = GetIndex(order.OrderType);
            int bar = index + 1;//GetBar(order.OrderType);

            pos.Quantity = order.Quantity;
            pos.EntryDate = DataCalcContext.Candles[index].t;
            pos.EntryBarNumber = bar;
            pos.AverageEntryPrice = order.FillPrice;
            pos.PositionNumber = _internalPositionNumber++;

            if (State == State.Playback || State == State.RealTime)
            {
                pos.IsRealTime = true;
            }

            var trade = new Trade()
            {
                StrategyId = LocalId,
                Instrument = Instrument.Symbol,
                Strategy = FullName,
                EntryBar = bar,
                EntryTime = DataCalcContext.Candles[index].t,
                EntryOrder = order,
                EntryPrice = order.FillPrice,
                EntryName = order.Name,
                EntryQuantity = order.Quantity,
                TradeNumber = _internalTradeNumber++,
                MarketPosition = pos.MarketPosition,
                GlobalTradeId = "P" + pos.PositionNumber + "T" + 1,
            };


            pos.Trades.Add(trade);

            Positions.Add(pos);
            AddBarTradeToChart(trade, index);
            AddTradeInfoToChart(trade);
        }


        private void ScaleIn(Order order)
        {
            LastPosition.Quantity += order.Quantity;
            int index = GetIndex(order.OrderType);
            int bar = index + 1;

            var trade = new Trade()
            {
                Strategy = FullName,
                StrategyId = LocalId,
                EntryBar = bar,
                EntryTime = DataCalcContext.Candles[index].t,
                EntryOrder = order,
                EntryPrice = order.FillPrice,
                EntryName = order.Name,
                EntryQuantity = order.Quantity,
                MarketPosition = _lastMarketPosition,
                TradeNumber = LastPosition.Trades[LastPosition.Trades.Count - 1].TradeNumber + 1,
            };

            trade.GlobalTradeId = "P" + LastPosition.PositionNumber + "T" + trade.TradeNumber;

            var entryPrices = LastPosition.Trades.Average(p => p.EntryPrice);
            LastPosition.AverageEntryPrice = entryPrices;

            LastPosition.Trades.Add(trade);
            AddBarTradeToChart(trade, index);
            AddTradeInfoToChart(trade);
        }

        private void ScaleOut(Order order)
        {
            if (order.Quantity >= LastPosition.Quantity)
            {
                CloseAll(order);
            }
            else if (order.Quantity < LastPosition.Quantity)
            {
                PartialClose(order);
            }
        }

        private void CloseAll(Order order)
        {
            int index = GetIndex(order.OrderType);
            int bar = index + 1;//GetBar(order.OrderType);

            LastPosition.MarketPosition = MarketPosition.Flat;
            _lastMarketPosition = MarketPosition.Flat;
            LastPosition.Quantity = 0;
            LastPosition.ExitDate = DataCalcContext.Candles[index].t;
            LastPosition.ExitBarNumber = bar;

            foreach (var trade in LastPosition.Trades)
            {
                if (trade.IsClosed)
                    continue;

                trade.ExitTime = DataCalcContext.Candles[index].t;
                trade.ExitBar = bar;
                trade.ExitPrice = order.FillPrice;
                trade.Profit = CalcHelper.CalcTradeProfit(trade.EntryPrice,
                    order.FillPrice, trade.MarketPosition, trade.EntryQuantity, Instrument.Multiplier);

                trade.Mae = CalcHelper.CalcTradeProfit(trade.EntryPrice,
                    GetMaeMfeVirtualExitPrice(trade, true), trade.MarketPosition, order.Quantity,
                    Instrument.Multiplier);

                trade.Mfe = CalcHelper.CalcTradeProfit(trade.EntryPrice,
                    GetMaeMfeVirtualExitPrice(trade, false), trade.MarketPosition, order.Quantity,
                    Instrument.Multiplier);

                trade.ExitQuantity = trade.EntryQuantity;
                trade.ExitName = order.Name;
                trade.ExitOrder = order;
                trade.IsClosed = true;
                trade.Commission = Commission.GetCommission(trade);
                AddBarTradeToChart(trade, index);
                ChangeTradeInfoToChart(trade);
            }

            OnTradeClosed();
        }

        private void PartialClose(Order order)
        {
            int index = GetIndex(order.OrderType);
            int bar = index + 1;//GetBar(order.OrderType);

            LastPosition.Quantity -= order.Quantity;

            var totalClosed = 0.0;

            foreach (var trade in LastPosition.Trades)
            {
                if (trade.IsClosed)
                    continue;

                if (trade.EntryQuantity + totalClosed <= order.Quantity)
                {
                    trade.ExitTime = DataCalcContext.Candles[index].t;
                    trade.ExitBar = bar;
                    trade.ExitPrice = order.FillPrice;
                    trade.Profit = CalcHelper.CalcTradeProfit(trade.EntryPrice,
                        order.FillPrice, trade.MarketPosition, trade.EntryQuantity, Instrument.Multiplier);

                    trade.Mae = CalcHelper.CalcTradeProfit(trade.EntryPrice,
                        GetMaeMfeVirtualExitPrice(trade, true), trade.MarketPosition, order.Quantity,
                        Instrument.Multiplier);

                    trade.Mfe = CalcHelper.CalcTradeProfit(trade.EntryPrice,
                        GetMaeMfeVirtualExitPrice(trade, false), trade.MarketPosition, order.Quantity,
                        Instrument.Multiplier);

                    trade.ExitQuantity = order.Quantity;
                    trade.ExitName = order.Name;
                    trade.ExitOrder = order;
                    trade.IsClosed = true;
                    trade.Commission = Commission.GetCommission(trade);
                    AddBarTradeToChart(trade, index);
                    ChangeTradeInfoToChart(trade);
                    totalClosed += trade.ExitQuantity;
                }
                else if (trade.EntryQuantity + totalClosed > order.Quantity)
                {
                    var closeNowQnt = order.Quantity - totalClosed;

                    var splitTrade = (Trade)trade.Clone();
                    splitTrade.EntryQuantity = trade.EntryQuantity - closeNowQnt;
                    splitTrade.TradeNumber = trade.TradeNumber + 0.1;
                    splitTrade.GlobalTradeId = "P" + LastPosition.PositionNumber + "T" + splitTrade.TradeNumber;

                    LastPosition.Trades.Add(splitTrade);

                    trade.ExitName = order.Name;
                    //trade.EntryQuantity = closeNowQnt;
                    trade.ExitTime = DataCalcContext.Candles[index].t;
                    trade.ExitBar = bar;
                    trade.ExitPrice = order.FillPrice;
                    trade.Profit = CalcHelper.CalcTradeProfit(trade.EntryPrice,
                        order.FillPrice, trade.MarketPosition, closeNowQnt, Instrument.Multiplier);

                    trade.Mae = CalcHelper.CalcTradeProfit(trade.EntryPrice,
                        GetMaeMfeVirtualExitPrice(trade, true), trade.MarketPosition, closeNowQnt,
                        Instrument.Multiplier);

                    trade.Mfe = CalcHelper.CalcTradeProfit(trade.EntryPrice,
                        GetMaeMfeVirtualExitPrice(trade, false), trade.MarketPosition, closeNowQnt,
                        Instrument.Multiplier);

                    trade.ExitQuantity = order.Quantity;
                    trade.ExitOrder = order;
                    trade.IsClosed = true;
                    trade.Commission = Commission.GetCommission(trade);
                    AddBarTradeToChart(trade, index);
                    ChangeTradeInfoToChart(trade);
                    totalClosed += closeNowQnt;
                }

                if (totalClosed == order.Quantity)
                    break;
            }

            OnTradeClosed();
        }

        private void OnTradeClosed()
        {
            //add realized profit calculation
            //net profit realtime trades
            if (State != State.Historical)
                Realized = GetRealizedPnl();
        }

        public double GetRealizedPnl()
        {
            var closedRealTrades = Positions.Where(t => t.IsRealTime).ToList();
            var profitSum = 0d;

            for (int i = 0; i < closedRealTrades.Count; i++)
            {
                for (int j = 0; j < closedRealTrades[i].Trades.Count; j++)
                {
                    profitSum += closedRealTrades[i].Trades[j].Profit;
                }
            }

            return profitSum;
        }

        #endregion

        #region Add Trades to Chart

        private int GetIndex(OrderType orderType)
        {
            int index = DataCalcContext.CurrentBar;

            if (orderType != OrderType.Market)
            {
                index = DataCalcContext.CurrentBar - 1;
            }

            if (State == State.Playback || State == State.RealTime)
            {
                index = DataCalcContext.CurrentBar;
            }

            //when placing market order on last bar of historical execution
            if (CurrentBar == DataCalcContext.Candles.Count && State == State.Historical)
            {
                index = DataCalcContext.CurrentBar - 1;
            }

            return index;
        }

        private int GetBar(OrderType orderType)
        {
            int bar = DataCalcContext.CurrentBar + 1;

            if (orderType != OrderType.Market)
            {
                bar = DataCalcContext.CurrentBar;
            }

            if (State == State.Playback || State == State.RealTime)
            {
                bar = DataCalcContext.CurrentBar + 1;
            }

            //when placing market order on last bar of historical execution
            if (CurrentBar == DataCalcContext.Candles.Count && State == State.Historical)
            {
                bar = DataCalcContext.CurrentBar;
            }

            return bar;
        }

        private void AddBarTradeToChart(Trade trade, int bar)
        {
            if (Chart == null)
                return;

            if (Chart.BarTrades.TryGetValue(bar, out var barTrades))
            {
                if (!barTrades.Contains(trade.GlobalTradeId))
                    barTrades.Add(trade.GlobalTradeId);
            }
            else
            {
                if (trade.GlobalTradeId == null)
                {
                    var t = 0;
                }

                Chart.BarTrades.Add(bar, new List<string>() { trade.GlobalTradeId });
            }
        }

        /*
        private double RoundToTickSize(double val, double tickSize)
        {
            try
            {
                if (tickSize == 0.25)
                {
                    val = Math.Round(val * 4, MidpointRounding.ToEven) / 4;
                    return val;
                }

                decimal argument = Convert.ToDecimal(TickSize);
                int count = BitConverter.GetBytes(decimal.GetBits(argument)[3])[2];

                return Math.Round(val, count);
            }
            catch (Exception e)
            {
                return val;
            }
        }*/

        public double RoundToTickSize(double value, double tickSize)
        {

            var diff = value % tickSize;

            if (diff == 0)
                return value;

            var val1 = value - diff;
            var val2 = val1 + tickSize;


            if (diff < val2 - value)
            {
                return val1;
            }


            return val2;
        }


        private async void AddTradeInfoToChart(Trade trade)
        {
      

            var tickSize = TickSize;


            Chart?.TradeDraws.Add(trade.GlobalTradeId,
                new TradeDraw()
                {
                    EntryTime = trade.EntryTime,
                    EntryBar = trade.EntryBar,
                    EntryPrice = RoundToTickSize(trade.EntryPrice, TickSize),
                    ExitBar = trade.ExitBar,
                    ExitPrice = RoundToTickSize(trade.ExitPrice, TickSize),
                    IsLong = trade.MarketPosition == MarketPosition.Long,
                    IsProfit = trade.Profit > 0,
                    EntryOrderName = trade.EntryName,
                    ExitOrderName = trade.ExitName,
                    EntryQuantity = trade.EntryQuantity,
                    ExitQuantity = trade.ExitQuantity,
                });
        }


        private async void ChangeTradeInfoToChart(Trade trade)
        {
            

            if (Chart == null)
                return;

            Chart.TradeDraws[trade.GlobalTradeId] =
                new TradeDraw()
                {
                    EntryTime = trade.EntryTime,
                    EntryBar = trade.EntryBar,
                    EntryPrice = trade.EntryPrice,
                    ExitBar = trade.ExitBar,
                    ExitPrice = trade.ExitPrice,
                    IsLong = trade.MarketPosition == MarketPosition.Long,
                    IsProfit = trade.Profit > 0,
                    EntryOrderName = trade.EntryName,
                    ExitOrderName = trade.ExitName,
                    EntryQuantity = trade.EntryQuantity,
                    ExitQuantity = trade.ExitQuantity
                };
        }

        #endregion

        private double GetMaeMfeVirtualExitPrice(Trade trade, bool isMae)
        {
            if (trade.MarketPosition == MarketPosition.Long)
            {
                if (isMae)
                {
                    return trade.MinLowPriceWhileOpen;
                }

                if (!isMae)
                {
                    return trade.MaxHighPriceWhileOpen;
                }
            }

            if (trade.MarketPosition == MarketPosition.Short)
            {
                if (isMae)
                {
                    return trade.MaxHighPriceWhileOpen;
                }

                if (!isMae)
                {
                    return trade.MinLowPriceWhileOpen;
                }
            }

            return 1;
        }

        /*private double CalcTradeProfit(double entryPrice, double exitPrice, MarketPosition mp, int qnt)
        {
            double profit = 0;

            if (mp == MarketPosition.Long)
            {
                profit = (exitPrice - entryPrice) * qnt;
            }

            if (mp == MarketPosition.Short)
            {
                profit = (entryPrice - exitPrice) * qnt;
            }

            return profit * Instrument.Multiplier ?? 1;
        }*/


        public void NotifyOptimizationParamsChanged()
        {
            OnPropertyChanged(nameof(Optimizer));
        }

        public void SetOptimizerParams(Dictionary<string, object> paramValues)
        {
            var paramsStr = "";
            foreach (var paramValue in paramValues)
            {
                var pi = TwmPropertyInfos[paramValue.Key];
                pi.SetValue(this, paramValue.Value);

                TwmPropertyValues[paramValue.Key] = paramValue.Value;

                paramsStr = paramsStr + paramValue.Key + "=" + paramValue.Value + ", ";
            }

            _optimizerParamsHashCode = paramsStr.GetHashCode();

            OnPropertyChanged("DisplayName");
        }


        public virtual void OnExecutionUpdate(Order order)
        {
        }


        public override void Print(object message)
        {
            if (!IsOptimization)
                base.Print(message);
        }


        public void Destroy()
        {
            Reset();
            base.Clear();
            Optimizer = null;
        }


        #region Obsolete

        //private void ActivePositionCheckOrders()
        //{
        //    var currentBarHigh = GetCurrentBarHigh();
        //    var currentBarLow = GetCurrentBarLow();

        //    if (LastPosition.MarketPosition == MarketPosition.Long)
        //    {
        //        var stopOrders = GetWorkingStopOrders();

        //        for (int i = 0; i < stopOrders.Count; i++)
        //        {
        //            if (stopOrders[i].OrderAction == OrderAction.Sell && currentBarLow <= stopOrders[i].StopPrice)
        //            {
        //                stopOrders[i].FillPrice = stopOrders[i].StopPrice;
        //                stopOrders[i].OrderState = OrderState.Filled;

        //                if (stopOrders[i].Quantity >= LastPosition.Quantity)
        //                {
        //                    CloseAll(stopOrders[i]);
        //                }
        //                else if (stopOrders[i].Quantity < LastPosition.Quantity)
        //                {
        //                    PartialClose(stopOrders[i]);
        //                }

        //                OnOrderUpdate(stopOrders[i]);
        //            }
        //        }

        //        var limitOrders = GetWorkingLimitOrders();

        //        for (int i = 0; i < limitOrders.Count; i++)
        //        {
        //            if (limitOrders[i].OrderAction == OrderAction.Sell && currentBarHigh >= limitOrders[i].LimitPrice)
        //            {
        //                limitOrders[i].FillPrice = limitOrders[i].LimitPrice;
        //                limitOrders[i].OrderState = OrderState.Filled;

        //                if (limitOrders[i].Quantity >= LastPosition.Quantity)
        //                {
        //                    CloseAll(limitOrders[i]);
        //                }
        //                else if (limitOrders[i].Quantity < LastPosition.Quantity)
        //                {
        //                    PartialClose(limitOrders[i]);
        //                }

        //                OnOrderUpdate(limitOrders[i]);
        //            }
        //        }
        //    }
        //    else if (LastPosition.MarketPosition == MarketPosition.Short)
        //    {
        //        var stopOrders = GetWorkingStopOrders();

        //        for (int i = 0; i < stopOrders.Count; i++)
        //        {
        //            if (stopOrders[i].OrderAction == OrderAction.BuyToCover &&
        //                currentBarHigh >= stopOrders[i].StopPrice)
        //            {
        //                stopOrders[i].FillPrice = stopOrders[i].StopPrice;
        //                stopOrders[i].OrderState = OrderState.Filled;

        //                if (stopOrders[i].Quantity >= LastPosition.Quantity)
        //                {
        //                    CloseAll(stopOrders[i]);
        //                }
        //                else if (stopOrders[i].Quantity < LastPosition.Quantity)
        //                {
        //                    PartialClose(stopOrders[i]);
        //                }

        //                OnOrderUpdate(stopOrders[i]);
        //            }
        //        }

        //        var limitOrders = GetWorkingLimitOrders();

        //        for (int i = 0; i < limitOrders.Count; i++)
        //        {
        //            if (limitOrders[i].OrderAction == OrderAction.BuyToCover &&
        //                currentBarLow <= limitOrders[i].LimitPrice)
        //            {
        //                limitOrders[i].FillPrice = limitOrders[i].LimitPrice;
        //                limitOrders[i].OrderState = OrderState.Filled;

        //                if (limitOrders[i].Quantity >= LastPosition.Quantity)
        //                {
        //                    CloseAll(limitOrders[i]);
        //                }
        //                else if (limitOrders[i].Quantity < LastPosition.Quantity)
        //                {
        //                    PartialClose(limitOrders[i]);
        //                }

        //                OnOrderUpdate(limitOrders[i]);
        //            }
        //        }
        //    }
        //}

        //private void PartialCloseInstant(Order order)
        //{
        //    LastPosition.Quantity -= order.Quantity;

        //    var totalClosed = 0;

        //    foreach (var trade in LastPosition.Trades)
        //    {
        //        if (trade.IsClosed)
        //            continue;

        //        if (trade.EntryQuantity + totalClosed <= order.Quantity)
        //        {
        //            trade.ExitTime = DataCalcContext.Candles[DataCalcContext.CurrentBar - 1].t;
        //            trade.ExitBar = DataCalcContext.CurrentBar;
        //            trade.ExitPrice = order.FillPrice;
        //            trade.Profit = CalcTradeProfit(trade.EntryPrice,
        //                order.FillPrice, trade.MarketPosition, trade.EntryQuantity);

        //            trade.ExitName = order.Name;
        //            trade.ExitOrder = order;
        //            trade.IsClosed = true;

        //            trade.ExitQuantity = order.Quantity;
        //            trade.EntryQuantity = order.Quantity;

        //            AddBarTradeToChart(trade, DataCalcContext.CurrentBar - 1);
        //            ChangeTradeInfoToChart(trade);
        //            totalClosed += trade.EntryQuantity;
        //        }
        //        else if (trade.EntryQuantity + totalClosed > order.Quantity)
        //        {
        //            var closeNowQnt = order.Quantity - totalClosed;

        //            var splitTrade = (Trade)trade.Clone();
        //            splitTrade.EntryQuantity = trade.EntryQuantity - closeNowQnt;
        //            splitTrade.TradeNumber = trade.TradeNumber + 0.1;
        //            splitTrade.GlobalTradeId = "P" + LastPosition.PositionNumber + "T" + splitTrade.TradeNumber;

        //            LastPosition.Trades.Add(splitTrade);

        //            trade.EntryQuantity = closeNowQnt;
        //            trade.ExitTime = DataCalcContext.Candles[DataCalcContext.CurrentBar - 1].t;
        //            trade.ExitBar = DataCalcContext.CurrentBar;
        //            trade.ExitPrice = order.FillPrice;
        //            trade.Profit = CalcTradeProfit(trade.EntryPrice,
        //                order.FillPrice, trade.MarketPosition, closeNowQnt);

        //            trade.ExitQuantity = order.Quantity;
        //            trade.EntryQuantity = order.Quantity;
        //            trade.ExitName = order.Name;
        //            trade.ExitOrder = order;
        //            trade.IsClosed = true;
        //            AddBarTradeToChart(trade, DataCalcContext.CurrentBar - 1);
        //            ChangeTradeInfoToChart(trade);
        //            totalClosed += closeNowQnt;
        //        }


        //        if (totalClosed == order.Quantity)
        //            break;
        //    }
        //}

        //private void PartialClose(Order order)
        //{
        //    LastPosition.Quantity -= order.Quantity;

        //    var totalClosed = 0;

        //    foreach (var trade in LastPosition.Trades)
        //    {
        //        if (trade.IsClosed)
        //            continue;

        //        if (trade.EntryQuantity + totalClosed <= order.Quantity)
        //        {
        //            trade.ExitTime = DataCalcContext.Candles[DataCalcContext.CurrentBar].t;
        //            trade.ExitBar = DataCalcContext.CurrentBar + 1;
        //            trade.ExitPrice = DataCalcContext.Candles[DataCalcContext.CurrentBar].O;
        //            trade.Profit = CalcTradeProfit(trade.EntryPrice,
        //                DataCalcContext.Candles[DataCalcContext.CurrentBar].O, trade.MarketPosition, trade.EntryQuantity);


        //            trade.ExitQuantity = order.Quantity;
        //            trade.EntryQuantity = order.Quantity;
        //            trade.ExitName = order.Name;
        //            trade.ExitOrder = order;
        //            trade.IsClosed = true;
        //            AddBarTradeToChart(trade, DataCalcContext.CurrentBar);
        //            ChangeTradeInfoToChart(trade);
        //            totalClosed += trade.EntryQuantity;
        //        }
        //        else if (trade.EntryQuantity + totalClosed > order.Quantity)
        //        {
        //            var closeNowQnt = order.Quantity - totalClosed;

        //            var splitTrade = (Trade)trade.Clone();
        //            splitTrade.EntryQuantity = trade.EntryQuantity - closeNowQnt;
        //            splitTrade.TradeNumber = trade.TradeNumber + 0.1;
        //            splitTrade.GlobalTradeId = "P" + LastPosition.PositionNumber + "T" + splitTrade.TradeNumber;

        //            LastPosition.Trades.Add(splitTrade);


        //            trade.ExitName = order.Name;
        //            trade.EntryQuantity = closeNowQnt;
        //            trade.ExitTime = DataCalcContext.Candles[DataCalcContext.CurrentBar].t;
        //            trade.ExitBar = DataCalcContext.CurrentBar + 1;
        //            trade.ExitPrice = DataCalcContext.Candles[DataCalcContext.CurrentBar].O;
        //            trade.Profit = CalcTradeProfit(trade.EntryPrice,
        //                DataCalcContext.Candles[DataCalcContext.CurrentBar].O, trade.MarketPosition, closeNowQnt);


        //            trade.ExitQuantity = order.Quantity;
        //            trade.EntryQuantity = order.Quantity;
        //            trade.ExitOrder = order;
        //            trade.IsClosed = true;
        //            AddBarTradeToChart(trade, DataCalcContext.CurrentBar);
        //            ChangeTradeInfoToChart(trade);
        //            totalClosed += closeNowQnt;
        //        }

        //        if (totalClosed == order.Quantity)
        //            break;
        //    }
        //}

        //private void CloseAll(Order order)
        //{
        //    LastPosition.MarketPosition = MarketPosition.Flat;
        //    LastPosition.Quantity = 0;
        //    LastPosition.ExitDate = DataCalcContext.Candles[DataCalcContext.CurrentBar].t;
        //    LastPosition.ExitBarNumber = DataCalcContext.CurrentBar + 1;

        //    foreach (var trade in LastPosition.Trades)
        //    {
        //        if (trade.IsClosed)
        //            continue;

        //        trade.ExitTime = DataCalcContext.Candles[DataCalcContext.CurrentBar].t;
        //        trade.ExitBar = DataCalcContext.CurrentBar + 1;
        //        trade.ExitPrice = DataCalcContext.Candles[DataCalcContext.CurrentBar].O;
        //        trade.Profit = CalcTradeProfit(trade.EntryPrice,
        //            order.FillPrice, trade.MarketPosition, trade.EntryQuantity);

        //        trade.ExitQuantity = order.Quantity;
        //        trade.ExitName = order.Name;
        //        trade.ExitOrder = order;
        //        trade.IsClosed = true;
        //        AddBarTradeToChart(trade, DataCalcContext.CurrentBar);
        //        ChangeTradeInfoToChart(trade);
        //    }

        //    foreach (var trade in LastPosition.Trades)
        //    {
        //        if (trade.IsClosed)
        //            continue;

        //        trade.ExitTime = DataCalcContext.Candles[DataCalcContext.CurrentBar - 1].t;
        //        trade.ExitBar = DataCalcContext.CurrentBar;
        //        trade.ExitPrice = order.FillPrice;
        //        trade.Profit = CalcTradeProfit(trade.EntryPrice,
        //            order.FillPrice, trade.MarketPosition, trade.EntryQuantity);

        //        trade.ExitQuantity = trade.EntryQuantity;
        //        trade.ExitName = order.Name;
        //        trade.ExitOrder = order;
        //        trade.IsClosed = true;
        //        AddBarTradeToChart(trade, DataCalcContext.CurrentBar - 1);
        //        ChangeTradeInfoToChart(trade);
        //    }
        //}

        //private void CloseAllInstant(Order order)
        //{
        //    LastPosition.MarketPosition = MarketPosition.Flat;
        //    LastPosition.Quantity = 0;
        //    LastPosition.ExitDate = DataCalcContext.Candles[DataCalcContext.CurrentBar - 1].t;
        //    LastPosition.ExitBarNumber = DataCalcContext.CurrentBar;

        //    foreach (var trade in LastPosition.Trades)
        //    {
        //        if (trade.IsClosed)
        //            continue;

        //        trade.ExitTime = DataCalcContext.Candles[DataCalcContext.CurrentBar - 1].t;
        //        trade.ExitBar = DataCalcContext.CurrentBar;
        //        trade.ExitPrice = order.FillPrice;
        //        trade.Profit = CalcTradeProfit(trade.EntryPrice,
        //            order.FillPrice, trade.MarketPosition, trade.EntryQuantity);

        //        trade.ExitQuantity = trade.EntryQuantity;
        //        trade.ExitName = order.Name;
        //        trade.ExitOrder = order;
        //        trade.IsClosed = true;
        //        AddBarTradeToChart(trade, DataCalcContext.CurrentBar - 1);
        //        ChangeTradeInfoToChart(trade);
        //    }
        //}

        //private void CreateNewPositionStopOrLimitOrder(Order order)
        //{
        //    var pos = new Position()
        //    {
        //        Instrument = order.Instrument,
        //        Trades = new List<Trade>()
        //    };

        //    if (order.OrderAction == OrderAction.Buy)
        //    {
        //        pos.MarketPosition = MarketPosition.Long;
        //    }

        //    if (order.OrderAction == OrderAction.SellShort)
        //    {
        //        pos.MarketPosition = MarketPosition.Short;
        //    }

        //    pos.Quantity = order.Quantity;
        //    pos.EntryDate = DataCalcContext.Candles[DataCalcContext.CurrentBar - 1].t;
        //    pos.EntryBarNumber = DataCalcContext.CurrentBar;
        //    pos.AverageEntryPrice = order.FillPrice;
        //    pos.PositionNumber = _internalPositionNumber++;

        //    var trade = new Trade()
        //    {
        //        Instrument = Instrument.Symbol,
        //        Strategy = this.DisplayName,
        //        StrategyId = LocalId,
        //        EntryBar = DataCalcContext.CurrentBar,
        //        EntryTime = DataCalcContext.Candles[DataCalcContext.CurrentBar - 1].t,
        //        EntryOrder = order,
        //        EntryPrice = order.FillPrice,
        //        EntryName = order.Name,
        //        EntryQuantity = order.Quantity,
        //        TradeNumber = _internalTradeNumber++,
        //        MarketPosition = pos.MarketPosition,
        //        GlobalTradeId = "P" + pos.PositionNumber + "T" + 1,
        //    };

        //    pos.Trades.Add(trade);

        //    Positions.Add(pos);
        //    AddBarTradeToChart(trade, DataCalcContext.CurrentBar - 1);
        //    AddTradeInfoToChart(trade);
        //}


        //private void CreateNewPositionUsingMarketOrder(Order order)
        //{
        //    var pos = new Position()
        //    {
        //        Instrument = order.Instrument,
        //        Trades = new List<Trade>()
        //    };

        //    if (order.OrderAction == OrderAction.Buy)
        //    {
        //        pos.MarketPosition = MarketPosition.Long;
        //    }

        //    if (order.OrderAction == OrderAction.SellShort)
        //    {
        //        pos.MarketPosition = MarketPosition.Short;
        //    }

        //    pos.Quantity = order.Quantity;
        //    pos.EntryDate = DataCalcContext.Candles[DataCalcContext.CurrentBar].t;
        //    pos.EntryBarNumber = DataCalcContext.CurrentBar + 1;
        //    pos.AverageEntryPrice = order.FillPrice;
        //    pos.PositionNumber = _internalPositionNumber++;

        //    var trade = new Trade()
        //    {
        //        StrategyId = LocalId,
        //        Instrument = Instrument.Symbol,
        //        Strategy = FullName,
        //        EntryBar = DataCalcContext.CurrentBar + 1,
        //        EntryTime = DataCalcContext.Candles[DataCalcContext.CurrentBar].t,
        //        EntryOrder = order,
        //        EntryPrice = order.FillPrice,
        //        EntryName = order.Name,
        //        EntryQuantity = order.Quantity,
        //        TradeNumber = _internalTradeNumber++,
        //        MarketPosition = pos.MarketPosition,
        //        GlobalTradeId = "P" + pos.PositionNumber + "T" + 1,
        //    };

        //    pos.Trades.Add(trade);

        //    Positions.Add(pos);
        //    AddBarTradeToChart(trade, DataCalcContext.CurrentBar);
        //    AddTradeInfoToChart(trade);
        //}

        #endregion


        public void BeforeChangeState(State state)
        {
            if (State == State.Historical && (state == State.Playback || state == State.RealTime))
            {
                if (Account.AccountType == AccountType.LocalPaper)
                {
                    Session.Instance.GetMockup(Connection).OnOrderStatusChanged += OnOrderUpdateMockUp;
                    TransferOrdersToRealTime();
                }
                else if (Account.AccountType == AccountType.ServerPaper || Account.AccountType == AccountType.Broker)
                {
                    Account.OnOrdersChanged += OnOrderUpdateServer;
                    Account.OnPositionsChanged += OnPositionsUpdateServer;
                }

                Draw.LineVertical("StartLive", CurrentBar, System.Windows.Media.Brushes.Red, 3, DashStyles.Dash);
            }
        }


        private void OnOrderUpdateServer(object sender, OrderChangeEventArgs e)
        {

            for (int i = 0; i < e.Orders.Count; i++)
            {
                Print("STRATEGYBASE OnOrderUpdate " + e.Orders[i].Name + " " + e.Orders[i].Guid);

                for (int j = 0; j < _orderBook.Count; j++)
                {
                    if (e.Orders[i].Guid == _orderBook[j].Guid)
                    {
                        Print("STRATEGYBASE OnOrderUpdate match found updating...");
                        _orderBook[j] = (Order)e.Orders[i].Clone();

                        HandlePositions(_orderBook[j]);
                        OnOrderUpdate(_orderBook[j]);

                    }
                }
            }


        }


        private void OnPositionsUpdateServer(object sender, PositionChangeEventArgs e)
        {

        }


        private void TransferOrdersToRealTime()
        {
            List<Order> workingOrders;
            lock (_lockObj)
            {
                workingOrders = _orderBook.Where(a =>
                    a.OrderState == OrderState.Working &&
                    (a.OrderType == OrderType.Limit ||
                     a.OrderType == OrderType.StopMarket)).ToList();
            }

            foreach (var workingOrder in workingOrders)
            {
                Task.Run(() => Session.Instance.GetMockup(Connection).SubmitOrder((Order)workingOrder.Clone()));
            }
        }
    }
}