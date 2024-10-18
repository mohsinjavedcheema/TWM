using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Twm.Core.Classes;
using Twm.Core.Enums;
using Twm.Core.Helpers;
using Twm.Core.Interfaces;
using Twm.Core.Market;

namespace Twm.Core.DataCalc.CalcStat
{
    public class CalcStat
    {
        private List<double> PercentArray { get; set; }

        private List<DrawDown> _drawDowns;

        //private readonly Dictionary<AnalyticalFeature, object> _defaultValues;

        private Dictionary<AnalyticalFeature, object> _currentValues;

        private List<ITrade> _trades;

        public string Name { get; set; }


        private double _cumProfit;
        private double _commission;
        private ITrade _prevTrade;

        private double _lastEquityHigh;
        private double _maxDrawDownStartEquity;
        private double _maxDrawDown;
        private double _maxMae;
        private DateTime _maxDrawDownDate;
        private int _tradeNo;

        private double _plus;
        private int _plusCount;
        private double  _minus;

        private int _maxConsWins;
        private int _maxConsLoss;
        private int _consWins;
        private int _consLoss;

        private double _lastTradeProfit;


        //private double _startCapital;
        private double _grossProfit;
        private double _grossLoss;


        private double _maxDDBelowZero;
        private int _countEquityHighs;

        public double StartingCapital { get; set; }

        public CalcStat(double startingCapital = 0.0, string name = "")
        {
            Name = Guid.NewGuid().ToString();
            StartingCapital = startingCapital;
            Reset();
           
        }

       



        private Dictionary<AnalyticalFeature, object> CreateValues()
        {
           return new Dictionary<AnalyticalFeature, object>
            {
                {AnalyticalFeature.StartDate, null},
                {AnalyticalFeature.EndDate, null},
                {AnalyticalFeature.NetProfitSum, StartingCapital},
                {AnalyticalFeature.Trades, 0},
                {AnalyticalFeature.TradesInProfit, 0.0},
                {AnalyticalFeature.AverageTradeProfit, 0.0},
                {AnalyticalFeature.AverageTradePercent, 0.0},
                {AnalyticalFeature.AverageWinningTrade, 0.0},
                {AnalyticalFeature.AverageWinningTradePercent, 0.0},
                {AnalyticalFeature.LargestWinningTrade, 0.0},
                {AnalyticalFeature.LargestWinningTradeNo, 0},
                {AnalyticalFeature.LargestWinningTradePercent, 0.0},
                {AnalyticalFeature.LargestWinningTradePercentNo, 0},
                {AnalyticalFeature.AverageLoosingTrade, 0.0},
                {AnalyticalFeature.AverageLoosingTradePercent, 0.0},
                {AnalyticalFeature.LargestLoosingTrade, 0.0},
                {AnalyticalFeature.LargestLoosingTradeNo, 0},
                {AnalyticalFeature.LargestLoosingTradePercent, 0.0},
                {AnalyticalFeature.LargestLoosingTradePercentNo, 0},
                {AnalyticalFeature.MaxConsWins, 0},
                {AnalyticalFeature.MaxConsLoss, 0},
                {AnalyticalFeature.AverageTradesInYear, 0},
                {AnalyticalFeature.AverageTradeDurationInDays, 0},
                {AnalyticalFeature.MaxDrawDown, 0.0},
                {AnalyticalFeature.MaxDrawDownPercent, 0.0},
                {AnalyticalFeature.MaxDrawDownDays, 0.0},
                {AnalyticalFeature.Calmar, 0.0},
                {AnalyticalFeature.Sharpe, 0.0},
                {AnalyticalFeature.Sortino, 0.0},
                {AnalyticalFeature.Expectancy, 0.0},
                {AnalyticalFeature.ProfitFactor, 0.0},
                {AnalyticalFeature.Commission, 0.0},
                {AnalyticalFeature.EquityHighs, 0},
                {AnalyticalFeature.MaxMae, 0.0}
            };
        }

        public void Reset()
        {
           // Debug.WriteLine(Name + " *** Reset");
            _trades = new List<ITrade>();
            _currentValues = CreateValues();
            PercentArray = new List<double>();
            _cumProfit = 0.0;
            _commission = 0.0;
            _prevTrade = null;
            _lastEquityHigh = 0.0;
            _maxDrawDownStartEquity = 0.0;
            _maxDrawDown = 0.0;
            _maxDrawDownDate = DateTime.MinValue;
            _tradeNo = 1;
            _plus = 0.0;
            _plusCount = 0;
            _minus = 0.0;
            _drawDowns = new List<DrawDown>();
            _lastTradeProfit = 0.0;
            _grossLoss = 0.0;
            _grossProfit = 0.0;
            _maxMae = 0.0;
            _maxDDBelowZero = 0.0;
            _countEquityHighs = 0;
            _maxConsWins = 0;
            _maxConsLoss =0;
            _consWins = 0;
            _consLoss = 0;
    }



        private int CalcBase(IEnumerable<ITrade> tradeList, IEnumerable<IRiskLevel> painLevels = null)
        {
            Dictionary<AnalyticalFeature, IRiskLevel> dictRiskLevels = new Dictionary<AnalyticalFeature, IRiskLevel>();
            var checkRiskLevels = false;
            if (painLevels != null)
            {
                var painLevelList = painLevels.ToList();

                var maxDrawDownRiskLevel = painLevelList.FirstOrDefault(x => x.Feature == AnalyticalFeature.MaxDrawDown);
                dictRiskLevels.Add(AnalyticalFeature.MaxDrawDown, maxDrawDownRiskLevel);

                 var maxDrawDownDaysRiskLevel = painLevelList.FirstOrDefault(x => x.Feature == AnalyticalFeature.MaxDrawDownDays);
                 dictRiskLevels.Add(AnalyticalFeature.MaxDrawDownDays, maxDrawDownDaysRiskLevel);

                var maxConsLossRiskLevel = painLevelList.FirstOrDefault(x => x.Feature == AnalyticalFeature.MaxConsLoss);
                dictRiskLevels.Add(AnalyticalFeature.MaxConsLoss, maxConsLossRiskLevel);

                var largestLoosingTradeRiskLevel = painLevelList.FirstOrDefault(x => x.Feature == AnalyticalFeature.LargestLoosingTrade);
                dictRiskLevels.Add(AnalyticalFeature.LargestLoosingTrade, largestLoosingTradeRiskLevel);

                var maxMaeRiskLevel = painLevelList.FirstOrDefault(x => x.Feature == AnalyticalFeature.MaxMae);
                dictRiskLevels.Add(AnalyticalFeature.MaxMae, maxMaeRiskLevel);

                checkRiskLevels = dictRiskLevels.Any(x => x.Value != null);
            }

            int painTradeIndex = -1;
            foreach (var trade in tradeList)
            {
                if (_cumProfit.Equals(0))
                {
                    trade.TradePercent = 0;
                }
                else
                {
                    trade.TradePercent = (trade.Profit * 100) / _cumProfit;
                }

                PercentArray.Add(trade.TradePercent);

                trade.CumProfitBefore = _cumProfit;

                trade.Profit -= trade.Commission;
                _commission += trade.Commission;

                _cumProfit += trade.Profit;

                if (_tradeNo == 1)
                    _cumProfit += StartingCapital;

                trade.CumProfit = _cumProfit;


                CalcDrawDowns(_tradeNo - 1, trade, _prevTrade, ref _lastEquityHigh, ref _maxDrawDown, ref _maxDrawDownDate,
                    ref _maxDrawDownStartEquity);

                

                _tradeNo++;


                if (trade.Profit > 0)
                {
                    _plus += trade.Profit;
                    _plusCount++;
                    _grossProfit += trade.Profit;

                    _consWins++;
                    if (_lastTradeProfit > 0)
                    {
                        if (_consWins > _maxConsWins)
                            _maxConsWins = _consWins;
                    }

                    _consLoss = 0;
                }

                if (trade.Profit < 0)
                {
                    _minus += trade.Profit;
                    _grossLoss += trade.Profit;
                    _consLoss++;
                    if (_lastTradeProfit < 0)
                    {
                        if (_consLoss > _maxConsLoss)
                            _maxConsLoss = _consLoss;
                    }

                    _consWins = 0;
                }

                if (trade.Mae < _maxMae)
                {
                    _maxMae = trade.Mae;
                }

                _lastTradeProfit = trade.Profit;

                _prevTrade = trade;


                if (checkRiskLevels)
                {
                    if (dictRiskLevels[AnalyticalFeature.MaxDrawDown] != null)
                    {
                        var painLevel = dictRiskLevels[AnalyticalFeature.MaxDrawDown].PainValue;
                        dictRiskLevels[AnalyticalFeature.MaxDrawDown].OSValue = _maxDrawDown;
                        if (_maxDrawDown < painLevel * -1)
                        {
                            dictRiskLevels[AnalyticalFeature.MaxDrawDown].IsFalse = true;
                            
                            painTradeIndex = _tradeNo - 2;
                            break;
                        }
                    }

                    if (dictRiskLevels[AnalyticalFeature.MaxDrawDownDays] != null)
                    {
                        var painLevel = dictRiskLevels[AnalyticalFeature.MaxDrawDownDays].PainValue;

                        dictRiskLevels[AnalyticalFeature.MaxDrawDownDays].OSValue = trade.CurrentDaysNoEquityHigh;
                        if (trade.CurrentDaysNoEquityHigh > painLevel)
                        {
                            dictRiskLevels[AnalyticalFeature.MaxDrawDownDays].IsFalse = true;
                            painTradeIndex = _tradeNo - 2;
                            break;
                        }
                    }

                    if (dictRiskLevels[AnalyticalFeature.LargestLoosingTrade] != null)
                    {
                        var painLevel = dictRiskLevels[AnalyticalFeature.LargestLoosingTrade].PainValue;
                        dictRiskLevels[AnalyticalFeature.LargestLoosingTrade].OSValue = trade.Profit;
                        if (trade.Profit < painLevel * -1)
                        {
                            dictRiskLevels[AnalyticalFeature.LargestLoosingTrade].IsFalse = true;
                            painTradeIndex = _tradeNo - 2;
                            break;
                        }
                    }


                    if (dictRiskLevels[AnalyticalFeature.MaxMae] != null)
                    {
                        var painLevel = dictRiskLevels[AnalyticalFeature.MaxMae].PainValue;

                        dictRiskLevels[AnalyticalFeature.MaxMae].OSValue = trade.Mae;
                        if (trade.Mae < painLevel * -1)
                        {
                            dictRiskLevels[AnalyticalFeature.MaxMae].IsFalse = true;
                            painTradeIndex = _tradeNo - 2;
                            break;
                        }
                    }
                }


            }

            return painTradeIndex;
        }


        public Dictionary<AnalyticalFeature, object> Calc(IEnumerable<ITrade> tradeList, out int painTradeIndex, IEnumerable<IRiskLevel> painLevels = null)
        {
            painTradeIndex = -1;

             var orderedTrades = tradeList.Where(x=>x.IsClosed).OrderBy(x => x.ExitTime).ToList();

            if (!orderedTrades.Any())
                return _currentValues;

            
            _trades.AddRange(orderedTrades);
            

            var tradeCount = _trades.Count();
            var startDate = _trades.Min(x => x.EntryTime);
            var endDate = _trades.Max(x => x.EntryTime);

            _currentValues[AnalyticalFeature.StartDate] =  startDate;
            _currentValues[AnalyticalFeature.EndDate] = endDate;

            painTradeIndex = CalcBase(orderedTrades, painLevels);

            _currentValues[AnalyticalFeature.EquityHighs] = _countEquityHighs;

            _currentValues[AnalyticalFeature.NetProfitSum] = Math.Round(_cumProfit, 0);
            _currentValues[AnalyticalFeature.ProfitFactor] = Math.Round(Math.Abs(_grossProfit/_grossLoss), 2);
            _currentValues[AnalyticalFeature.Commission] = Math.Round(_commission, 2);

            if (painTradeIndex != -1)
            {
                tradeCount = painTradeIndex + 1;
                _trades = _trades.Take(tradeCount).ToList();
                _currentValues[AnalyticalFeature.EndDate] = _trades.Max(x => x.EntryTime); 
            }

            _currentValues[AnalyticalFeature.Trades] = tradeCount;
            _currentValues[AnalyticalFeature.TradesInProfit] = Math.Round((((double)_plusCount / tradeCount) * 100), 2);
            _currentValues[AnalyticalFeature.AverageTradeProfit] = Math.Round((_cumProfit) / tradeCount, 0);
            _currentValues[AnalyticalFeature.AverageTradePercent] = Math.Round(PercentArray.Sum() / PercentArray.Count, 2);
            

            #region winning Trades

            var winningTrades = _trades.Where(x => x.Profit > 0).Select(x => x.Profit).ToList();
            var winningTradesPercent = PercentArray.Where(x => x > 0).ToList();
            var winningTradesMax = winningTrades.DefaultIfEmpty(0.0).Max();
            var winningTradesPercentMax = winningTradesPercent.DefaultIfEmpty(0.0).Max();

            _currentValues[AnalyticalFeature.AverageWinningTrade] = Math.Round(winningTrades.Sum() / winningTrades.Count, 0);
            _currentValues[AnalyticalFeature.AverageWinningTradePercent] = Math.Round(winningTradesPercent.Sum() / winningTradesPercent.Count(), 2);
            _currentValues[AnalyticalFeature.LargestWinningTrade] = Math.Round(winningTradesMax, 0);
            _currentValues[AnalyticalFeature.LargestWinningTradeNo] = _trades.FindIndex(x => x.Profit.Equals(winningTradesMax)) + 1;
            _currentValues[AnalyticalFeature.LargestWinningTradePercent] = Math.Round(winningTradesPercentMax, 2);
            _currentValues[AnalyticalFeature.LargestWinningTradePercentNo] = PercentArray.FindIndex(x => x.Equals(winningTradesPercentMax)) + 1;

            #endregion

            #region loosing Trades

            var loosingTrades = _trades.Where(x => x.Profit < 0).Select(x => x.Profit).ToList();
            var loosingTradesPercent = PercentArray.Where(x => x < 0).ToList();
            var loosingTradesMax = loosingTrades.DefaultIfEmpty(0.0).Min();
            var loosingTradesPercentMax = loosingTradesPercent.DefaultIfEmpty(0.0).Min();

            _currentValues[AnalyticalFeature.AverageLoosingTrade] = Math.Round(loosingTrades.Sum() / loosingTrades.Count, 0);
            _currentValues[AnalyticalFeature.AverageLoosingTradePercent] = Math.Round(loosingTradesPercent.Sum() / loosingTradesPercent.Count, 2);
            _currentValues[AnalyticalFeature.LargestLoosingTrade] = Math.Round(loosingTradesMax, 0);
            _currentValues[AnalyticalFeature.LargestLoosingTradeNo] = _trades.FindIndex(x => x.Profit.Equals(loosingTradesMax)) + 1;
            _currentValues[AnalyticalFeature.LargestLoosingTradePercent] = Math.Round(loosingTradesPercentMax, 2);
            _currentValues[AnalyticalFeature.LargestLoosingTradePercentNo] = PercentArray.FindIndex(x => x.Equals(loosingTradesPercentMax)) + 1;

            #endregion

            _currentValues[AnalyticalFeature.MaxConsWins] = _maxConsWins;
            _currentValues[AnalyticalFeature.MaxConsLoss] = _maxConsLoss;

            var dateBegin = startDate;
            var dateEnd = _trades.Max(x => x.ExitTime);

            _currentValues[AnalyticalFeature.AverageTradesInYear] = (int)((tradeCount * 365) / (dateEnd - dateBegin).TotalDays);
            _currentValues[AnalyticalFeature.AverageTradeDurationInDays] = (int)(_trades.Sum(x => (x.ExitTime - x.EntryTime).TotalDays) / tradeCount);

            #region DrawDowns

            _currentValues[AnalyticalFeature.MaxDrawDown] = Math.Round(_maxDrawDown, 0);
            _currentValues[AnalyticalFeature.MaxDrawDownPercent] = Math.Round((_maxDrawDown / _maxDrawDownStartEquity) * 100, 2);

            var maxDrawDownDays = 0.0;

            if (_drawDowns.Count > 0)
            {
                var dd = _drawDowns.Where(x => x.EndDate != DateTime.MinValue);
                if (dd.Any())
                    maxDrawDownDays = dd.Max(x => (x.EndDate - x.StartDate).TotalDays);
                if (_trades[tradeCount - 1].CurrentDaysNoEquityHigh > maxDrawDownDays)
                    maxDrawDownDays = _trades[tradeCount - 1].CurrentDaysNoEquityHigh;
            }

            _currentValues[AnalyticalFeature.MaxDrawDownDays] = maxDrawDownDays;

            #endregion

            #region Calmar
            _currentValues[AnalyticalFeature.Calmar] = Math.Abs(Math.Round((_cumProfit) / _maxDrawDown, 2));
            #endregion

            #region Sharp
            _currentValues[AnalyticalFeature.Sharpe] = Math.Round(CalcSharpe(_trades, dateBegin, dateEnd, _cumProfit), 2);
            #endregion

            #region Sortino
            _currentValues[AnalyticalFeature.Sortino] = Math.Round(CalcSortino(_trades, dateBegin, dateEnd, _cumProfit), 2);
            #endregion

            #region Expectancy
            double winPercentRation = (double) winningTrades.Count / tradeCount;
            double lossPercentRation = (double) loosingTrades.Count / tradeCount;
            double riskReward = 0;

            if (loosingTrades.Count > 0 && winningTrades.Count > 0)
            {
                riskReward = winningTrades.Average() / Math.Abs(loosingTrades.Average());
            }

            _currentValues[AnalyticalFeature.Expectancy] = riskReward * winPercentRation - lossPercentRation;

            #endregion

            _currentValues[AnalyticalFeature.MaxMae] = _maxMae; 

            return _currentValues;
        }


        private double CalcSharpe(IEnumerable<ITrade> trades, DateTime dateBegin, DateTime dateEnd, double cumProfit, double startCapital = 0.0)
        {
            var days = (dateEnd - dateBegin).Days;
            var ppm = (cumProfit - startCapital) / days * 30.5;

            var monthly = trades.GroupBy(x => new { x.ExitTime.Month, x.ExitTime.Year }).Select(
                g => new
                {
                    Date = g.Key,
                    Value = Math.Round(g.Sum(s => s.Profit), 2)
                }
            ).ToDictionary(x => (x.Date), y => y.Value);

            var profitsMonthly = monthly.OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month).Select(x => x.Value)
                .ToArray();

            if (profitsMonthly.Length <= 1)
                return 0;

            var mpStat = new StatDescriptive.Descriptive(profitsMonthly);
            mpStat.Analyze();
            var stdDevMonthlyProfits = mpStat.Result.StdDev;
            var sharpe = (ppm - 0) / stdDevMonthlyProfits;
            return sharpe;
        }

        private double CalcSortino(IEnumerable<ITrade> orderedTrades, DateTime dateBegin, DateTime dateEnd, double cumProfit, double startCapital = 0.0)
        {
            var days = (dateEnd - dateBegin).Days;
            var ppm = (cumProfit - startCapital) / days * 30.5;

            var monthlyTradesList = orderedTrades.GroupBy(x => new { x.ExitTime.Month, x.ExitTime.Year }).Select(
                g => new
                {
                    Date = g.Key,
                    Value = g.ToList()
                }
            ).ToDictionary(x => (x.Date), y => y.Value);

            var maxDrawDownList = new List<double>();

            foreach (var monthlyTrades in monthlyTradesList)
            {
                var trades = monthlyTrades.Value.Select(x => x.CloneTo(new Trade()));
                ITrade prevTrade = null;

                var lastEquityHigh = 0.0;

                var i = 1;

                var maxDrawDownSortino = 0.0;
                var maxDrawDownStartEquity = 0.0;
                var maxDrawDownDate = DateTime.MinValue;

                cumProfit = 0.0;

                foreach (var trade in trades)
                {
                    cumProfit += trade.Profit;
                    trade.CumProfit = cumProfit;

                    trade.CurrentDrawDown = 0;
                    CalcDrawDowns(i - 1, trade, prevTrade, ref lastEquityHigh, ref maxDrawDownSortino,
                        ref maxDrawDownDate, ref maxDrawDownStartEquity);


                    i++;
                    prevTrade = trade;
                }

                maxDrawDownList.Add(maxDrawDownSortino);
            }

            if (maxDrawDownList.Count <= 1)
                return 0;

            var mpStat = new StatDescriptive.Descriptive(maxDrawDownList.ToArray());
            mpStat.Analyze();
            var stdDevMonthlyProfits = mpStat.Result.StdDev;
            var sortino = (ppm - 0) / stdDevMonthlyProfits;

            return sortino;
        }


        

        private void CalcDrawDowns(int index, ITrade trade, ITrade prevTrade, ref double lastEquityHigh,
            ref double maxDrawDown, ref DateTime maxDrawDownDate, ref double startEquity)
        {
            if (index == 0)
            {
                if (trade.Profit > 0)
                {
                    lastEquityHigh = trade.CumProfit;
                    trade.CurrentDaysNoEquityHigh = 0;
                    _countEquityHighs++;
                }

                else if (trade.Profit < 0)
                {
                    //lastEquityHigh = 0;
                    trade.CurrentDrawDown = trade.Profit;
                    trade.CurrentDaysNoEquityHigh = (trade.ExitTime - trade.EntryTime).TotalDays;
                }

                if (trade.CumProfit < _maxDDBelowZero)
                {
                    _maxDDBelowZero = trade.CumProfit;
                }

                if (trade.CumProfit < lastEquityHigh)
                {
                    var dd = new DrawDown()
                    {
                        StartDate = trade.ExitTime,
                        StartEquity = lastEquityHigh,
                        Values = new List<double>()
                        {
                            trade.CurrentDrawDown
                        }
                    };

                    _drawDowns.Add(dd);
                }

                if (trade.CurrentDrawDown < maxDrawDown)
                {
                    maxDrawDown = trade.CurrentDrawDown;
                    maxDrawDownDate = trade.ExitTime;
                    //startEquity = _drawDowns[_drawDowns.Count - 1].StartEquity;
                }
            }

            if (index > 0)
            {
                if (prevTrade != null)
                {
                    if (trade.CumProfit > lastEquityHigh)
                    {
                        _countEquityHighs++;
                        trade.CurrentDaysNoEquityHigh = 0;
                        //add draw down period end date and calculate period Max Draw Down
                        if (prevTrade.CumProfit < lastEquityHigh)
                        {
                            _drawDowns[_drawDowns.Count - 1].EndDate = trade.ExitTime;

                            _drawDowns[_drawDowns.Count - 1].PeriodMaxDrawDown =
                                _drawDowns[_drawDowns.Count - 1].Values.Min();

                            var period = _drawDowns[_drawDowns.Count - 1].EndDate -
                                         _drawDowns[_drawDowns.Count - 1].StartDate;

                            _drawDowns[_drawDowns.Count - 1].DrawDownPeriodDays = period.TotalHours / 24;
                        }

                        lastEquityHigh = trade.CumProfit;
                        trade.CurrentDrawDown = 0;
                    }
                    else if (trade.CumProfit < lastEquityHigh)
                    {
                        trade.CurrentDrawDown = trade.CumProfit - lastEquityHigh;

                        //draw down started
                        if (prevTrade.CumProfit == lastEquityHigh)
                        {
                            var dd = new DrawDown()
                            {
                                StartDate = trade.ExitTime,
                                StartEquity = prevTrade.CumProfit,
                                Values = new List<double>()
                                {
                                    trade.CurrentDrawDown
                                }

                            };

                            _drawDowns.Add(dd);
                        }
                        else
                        {
                            //adding current draw down values to draw down object list of values
                            _drawDowns[_drawDowns.Count - 1].Values.Add(trade.CurrentDrawDown);
                        }

                        trade.CurrentDaysNoEquityHigh =
                            (trade.ExitTime - _drawDowns[_drawDowns.Count - 1].StartDate).TotalDays;


                        //evaluating maximum draw down and max draw down date
                        if (trade.CurrentDrawDown < maxDrawDown)
                        {
                            maxDrawDown = trade.CurrentDrawDown;
                            maxDrawDownDate = trade.ExitTime;
                            startEquity = _drawDowns[_drawDowns.Count - 1].StartEquity;
                        }
                    }
                }

                if (trade.CumProfit < _maxDDBelowZero)
                {
                    _maxDDBelowZero = trade.CumProfit;
                }
            }
        }
        
    }
}