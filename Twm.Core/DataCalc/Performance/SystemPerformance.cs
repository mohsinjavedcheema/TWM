using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
//using System.Windows;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Core.Interfaces;
using Twm.Core.Market;

namespace Twm.Core.DataCalc.Performance
{
    [DataContract]
    public class SystemPerformance
    {
        protected StrategyBase[] Strategies;

        protected ITrade[] AllTrades;


        [DataMember]
        public List<ITrade> Trades
        {
            get; set; 

        }

        public List<Order> Orders { get; set; }

        public int StartTradeCount { get; set; }

        [DataMember]
        public Summary Summary { get; set; }


        public bool IsPortfolio { get; set; }


        private readonly CalcStat.CalcStat _allCalcStat;
        private readonly CalcStat.CalcStat _shortCalcStat;
        private readonly CalcStat.CalcStat _longCalcStat;

        private  StrategyBase _strategy;

        private bool _isOptimization;


        public SystemPerformance()
        {
            
            Trades = new List<ITrade>();
            Orders = new List<Order>();
            Summary = new Summary();

        }

        public SystemPerformance(StrategyBase[] strategies, ITrade[] trades = null, bool isPortfolio = false,
            bool isOptimization = false):this()
        {
            Strategies = strategies;
            AllTrades = trades;
            IsPortfolio = isPortfolio;

            if (strategies != null && strategies.Any())
            {
                _strategy = strategies.FirstOrDefault();
            }

            _isOptimization = isOptimization;


            var startingCapital = 0d;

           

            _allCalcStat = new CalcStat.CalcStat(startingCapital);

            if (!_isOptimization)
            {
                _shortCalcStat = new CalcStat.CalcStat(startingCapital);
                _longCalcStat = new CalcStat.CalcStat(startingCapital);
            }
        }


        public virtual void Clear()
        {
            Trades.Clear();
            Orders.Clear();
            Summary.Clear();
        }

        public void Reset()
        {
            _allCalcStat.Reset();
            _shortCalcStat?.Reset();
            _longCalcStat?.Reset();
        }

        public virtual void Destroy()
        {
            Clear();
            Reset();
            _strategy = null;
            Strategies = null;
        }

        public virtual async Task Calculate(CancellationToken cancellationToken,
            IEnumerable<IRiskLevel> painLevels = null, TradeSourceType? tradeSourceType = TradeSourceType.Total)
        {
            if ((Strategies == null || !Strategies.Any()) && (AllTrades == null /*|| !AllTrades.Any()*/))
            {
                return;
            }

            var trades = new List<ITrade>();

            if (Strategies != null && Strategies.Any())
            {
                foreach (var strategy in Strategies)
                {
                    trades.AddRange(strategy.GetAllTrades(tradeSourceType).Select(x => (ITrade) x.Clone()));
                }
            }
            else
            {
                trades.AddRange(AllTrades.Select(x => new Trade().CopyFrom(x)));
            }

            if (tradeSourceType == TradeSourceType.RealTime)
                trades = trades.OrderByDescending(x => x.ExitTime).ToList();
            else
                trades = trades.OrderBy(x => x.ExitTime).ToList();

            if (painLevels != null)
            {
                StartTradeCount = trades.Count(x => x.IsClosed);
            }

            var paintTradeIndex = await CalcSummary(trades, cancellationToken, painLevels);

            if (paintTradeIndex != -1)
                trades = trades.Take(paintTradeIndex + 1).ToList();



            AddTrades(trades);

            #region Orders

            if (!IsPortfolio && _strategy != null)
            {
                IOrderedEnumerable<Order> orders;

                if (tradeSourceType == TradeSourceType.RealTime)
                    orders = _strategy.GetOrders(tradeSourceType).OrderByDescending(x => x.OrderInitDate);
                else
                    orders = _strategy.GetOrders(tradeSourceType).OrderBy(x => x.Id);

               // Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var order in orders)
                    {
                        
                        Orders.Add((Order) order.Clone());
                    }
                }
                //);
            }
            Debug.WriteLine("Calculation end");

            #endregion
            
        }


      



        public void SetAllTrades(ITrade[] trades)
        {
            AllTrades = trades;
        }

        public void CalcByStep(CancellationToken cancellationToken)
        {
            var trades = _strategy.GetLastTrades().Select(x => (Trade) x.Clone()).ToList();
            CalcSummary(trades, cancellationToken).GetAwaiter().GetResult();
        }


        private async Task<int> CalcSummary(IEnumerable<ITrade> trades, CancellationToken cancellationToken,
            IEnumerable<IRiskLevel> painLevels = null)
        {
            List<Task> tasks = new List<Task>();
            List<IRiskLevel> shortRiskLevels = null;
            List<IRiskLevel> longRiskLevels = null;
            List<IRiskLevel> painLevelList = null;
            if (painLevels != null)
            {
                painLevelList = painLevels.ToList();
                shortRiskLevels = new List<IRiskLevel>();
                longRiskLevels = new List<IRiskLevel>();

                foreach (var painLevel in painLevelList)
                {
                    shortRiskLevels.Add((IRiskLevel) painLevel.Clone());
                    longRiskLevels.Add((IRiskLevel) painLevel.Clone());
                }
            }

            #region Summary

            int painTradeIndex = -1;
            var allTask = Task.Run(() => _allCalcStat.Calc(trades, out painTradeIndex, painLevelList),
                cancellationToken);

            if (painLevels != null)
            {
                await Task.WhenAll(allTask);
                trades = trades.Take(painTradeIndex + 1);
            }
            else
            {
                tasks.Add(allTask);
            }

            if (!_isOptimization)
            {
                var longTask = Task.Run(() =>
                {
                    return _longCalcStat.Calc(trades.Where(x => x.MarketPosition == MarketPosition.Long )
                        .Select(x => new Trade().CopyFrom(x)).ToArray(), out var painTradeIndexLong, shortRiskLevels);
                }, cancellationToken);
                tasks.Add(longTask);

                var shortTask = Task.Run(() =>
                {
                    return _shortCalcStat.Calc(trades
                        .Where(x => x.MarketPosition == MarketPosition.Short)
                        .Select(x => new Trade().CopyFrom(x)).ToArray(), out var painTradeIndexShort, longRiskLevels);
                }, cancellationToken);
                tasks.Add(shortTask);


                await Task.WhenAll(tasks);

                Summary.FetchData(await allTask, await longTask, await shortTask);
            }
            else
            {
                await Task.WhenAll(tasks);

                Summary.FetchData(await allTask);
            }

            return painTradeIndex;

            #endregion
        }


        public double GetValue(AnalyticalFeature analyticalFeature, MarketPosition? marketPosition = null)
        {
            CalcByStep(new CancellationToken());

            return Summary.GetValue(analyticalFeature, marketPosition);
        }


        public T GetValue<T>(AnalyticalFeature analyticalFeature, MarketPosition? marketPosition = null)
        {
            CalcByStep(new CancellationToken());
            return Summary.GetValue<T>(analyticalFeature, marketPosition);
        }


        private void AddTrades(IEnumerable<ITrade> trades)
        {
            #region Trades

            //Application.Current.Dispatcher.Invoke(() =>
            {
               // foreach (var trade in trades)
                {
                    Trades.AddRange(trades);
                }
            }
            //);

            #endregion
        }
    }
}