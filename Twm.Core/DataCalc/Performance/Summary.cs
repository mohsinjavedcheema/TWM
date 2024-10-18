using System;
using System.Collections.Generic;
using Twm.Core.Enums;

namespace Twm.Core.DataCalc.Performance
{
    public class Summary
    {
        public List<SummaryItem> SummaryItems { get; set; }



        private Dictionary<AnalyticalFeature, object> _allTradesDict;
        private Dictionary<AnalyticalFeature, object> _longTradesDict;
        private Dictionary<AnalyticalFeature, object> _shortTradesDict;

        private readonly object _observableCollectionLock = new object();

        public Summary()
        {
            SummaryItems = new List<SummaryItem>();            
        }


        public void Clear()
        {
            SummaryItems.Clear();
        }

        public double GetValue(AnalyticalFeature analyticalFeature, MarketPosition? marketPosition = null)
        {
            var dict = _allTradesDict;
            if (marketPosition != null)
            {
                if (marketPosition == MarketPosition.Long)
                {
                    dict = _longTradesDict;
                }
                else if (marketPosition == MarketPosition.Short)
                {
                    dict = _shortTradesDict;
                }
                else
                {
                    return Double.NaN;
                }

            }

            if (dict == null)
            {
                return Double.NaN;
            }

            if (dict.TryGetValue(analyticalFeature, out var value))
            {
                return Convert.ToDouble(value);
            }
            return Double.NaN;

        }


        public T GetValue<T>(AnalyticalFeature analyticalFeature, MarketPosition? marketPosition = null)
        {
            var dict = _allTradesDict;
            if (marketPosition != null)
            {
                if (marketPosition == MarketPosition.Long)
                {
                    dict = _longTradesDict;
                }
                else if (marketPosition == MarketPosition.Short)
                {
                    dict = _shortTradesDict;
                }
                else
                {
                    return default(T);
                }

            }

            if (dict == null)
            {
                return default(T);
            }

            if (dict.TryGetValue(analyticalFeature, out var value))
            {
                if (value == null)
                    return default(T);
                else
                {
                    if (value is T)
                    {
                        return (T)value;
                    }
                    try
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                    catch (InvalidCastException)
                    {
                        return default(T);
                    }
                }
                  
            }
            return default(T);

        }


        public void FetchData(Dictionary<AnalyticalFeature, object> allTrades,
            Dictionary<AnalyticalFeature, object> longTrades,
            Dictionary<AnalyticalFeature, object> shortTrades)
        {
            _allTradesDict = allTrades;
            _longTradesDict = longTrades;
            _shortTradesDict = shortTrades;


            CreateSummaryItem(AnalyticalFeature.NetProfitSum);
            CreateSummaryItem(AnalyticalFeature.Commission);
            CreateSummaryItem(AnalyticalFeature.ProfitFactor);
            CreateSummaryItem(AnalyticalFeature.Sharpe);
            CreateSummaryItem(AnalyticalFeature.Sortino, true);
            CreateSummaryItem(AnalyticalFeature.EquityHighs);
            CreateSummaryItem(AnalyticalFeature.MaxDrawDown);
            CreateSummaryItem(AnalyticalFeature.MaxDrawDownDays);
            CreateSummaryItem(AnalyticalFeature.MaxMae, true);

            CreateSummaryItem(AnalyticalFeature.StartDate);
            CreateSummaryItem(AnalyticalFeature.EndDate, true);

            CreateSummaryItem(AnalyticalFeature.MaxConsLoss);
            CreateSummaryItem(AnalyticalFeature.MaxConsWins, true);

            CreateSummaryItem(AnalyticalFeature.Trades);
            CreateSummaryItem(AnalyticalFeature.TradesInProfit);
            CreateSummaryItem(AnalyticalFeature.AverageTradesInYear);
            CreateSummaryItem(AnalyticalFeature.AverageTradeDurationInDays);
            CreateSummaryItem(AnalyticalFeature.AverageTradeProfit, true);

            CreateSummaryItem(AnalyticalFeature.AverageWinningTrade);
            CreateSummaryItem(AnalyticalFeature.LargestWinningTrade, true);


            CreateSummaryItem(AnalyticalFeature.AverageLoosingTrade);
            CreateSummaryItem(AnalyticalFeature.LargestLoosingTrade, true);
            
        }

        private void CreateSummaryItem(AnalyticalFeature analyticalFeature, bool isLastItem = false, bool isFull = true)
        {
            lock (_observableCollectionLock)
            {
                
              //  Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var summaryItem =  new SummaryItem(analyticalFeature)
                    {
                        AllTrades = _allTradesDict[analyticalFeature],
                        IsLastItem = isLastItem
                    };

                    if (isFull)
                    {
                        summaryItem.LongTrades = _longTradesDict[analyticalFeature];
                        summaryItem.ShortTrades = _shortTradesDict[analyticalFeature];
                    }

                    SummaryItems.Add(summaryItem);
                }
                //);
            }
        }


        public void FetchData(Dictionary<AnalyticalFeature, object> allTrades)
        {
            _allTradesDict = allTrades;

            CreateSummaryItem(AnalyticalFeature.NetProfitSum, false, false);
            CreateSummaryItem(AnalyticalFeature.Commission, false, false);
            CreateSummaryItem(AnalyticalFeature.ProfitFactor, false, false);
            CreateSummaryItem(AnalyticalFeature.Sharpe, false, false);
            CreateSummaryItem(AnalyticalFeature.Sortino, true, false);

            CreateSummaryItem(AnalyticalFeature.EquityHighs, false, false);
            CreateSummaryItem(AnalyticalFeature.MaxDrawDown, false, false);
            CreateSummaryItem(AnalyticalFeature.MaxDrawDownDays, false, false);
            CreateSummaryItem(AnalyticalFeature.MaxMae, true, false);

            CreateSummaryItem(AnalyticalFeature.StartDate, false, false);
            CreateSummaryItem(AnalyticalFeature.EndDate, true, false);

            CreateSummaryItem(AnalyticalFeature.MaxConsLoss, false, false);
            CreateSummaryItem(AnalyticalFeature.MaxConsWins, true, false);

            CreateSummaryItem(AnalyticalFeature.Trades, false, false);
            CreateSummaryItem(AnalyticalFeature.TradesInProfit, false, false);
            CreateSummaryItem(AnalyticalFeature.AverageTradesInYear, false, false);
            CreateSummaryItem(AnalyticalFeature.AverageTradeDurationInDays, false, false);
            CreateSummaryItem(AnalyticalFeature.AverageTradeProfit, true, false);
            
            CreateSummaryItem(AnalyticalFeature.AverageWinningTrade, false, false);
            CreateSummaryItem(AnalyticalFeature.LargestWinningTrade, true, false);

            CreateSummaryItem(AnalyticalFeature.AverageLoosingTrade, false, false);
            CreateSummaryItem(AnalyticalFeature.LargestLoosingTrade, true, false);

        }
    }
}