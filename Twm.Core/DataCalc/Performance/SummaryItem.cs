using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Twm.Core.Enums;
using Twm.Core.Helpers;

namespace Twm.Core.DataCalc.Performance
{
    public class SummaryItem : INotifyPropertyChanged
    {
        private bool _isLastItem;

        public bool IsLastItem
        {
            get { return _isLastItem; }
            set
            {
                _isLastItem = value;
                OnPropertyChanged();
            }
        }

        private AnalyticItem _analyticItem;

        public AnalyticItem AnalyticItem
        {
            get { return _analyticItem; }

            set
            {
                _analyticItem = value;
                OnPropertyChanged();
            }
        }


        private string _allTradesStringFormat;

        public string AllTradesStringFormat
        {
            get { return _allTradesStringFormat; }
            set
            {
                _allTradesStringFormat = value;
                OnPropertyChanged();
            }
        }


        private string _longTradesStringFormat;

        public string LongTradesStringFormat
        {
            get { return _longTradesStringFormat; }
            set
            {
                _longTradesStringFormat = value;
                OnPropertyChanged();
            }
        }


        private string _shortTradesStringFormat;

        public string ShortTradesStringFormat
        {
            get { return _shortTradesStringFormat; }
            set
            {
                _shortTradesStringFormat = value;
                OnPropertyChanged();
            }
        }



        private object _allTrades;

        public object AllTrades
        {
            get { return _allTrades; }
            set
            {
                _allTrades = value;
                OnPropertyChanged();

            }
        }


        public string AllTradesFormat
        {
            get
            {
                return StringHelper.Format(AllTrades, GetStringFormat(AnalyticItem.Feature, false));
            }
        }


        private object _longTrades;

        public object LongTrades
        {
            get { return _longTrades; }
            set
            {
                _longTrades = value;
                OnPropertyChanged();
            }
        }


        public string LongTradesFormat
        {
            get
            {
                return StringHelper.Format(LongTrades, GetStringFormat(AnalyticItem.Feature, false));
            }
        }


        private object _shortTrades;

        public object ShortTrades
        {
            get { return _shortTrades; }
            set
            {
                _shortTrades = value;
                OnPropertyChanged();
            }
        }


        public string ShortTradesFormat
        {
            get
            {
                return StringHelper.Format(ShortTrades, GetStringFormat(AnalyticItem.Feature, false));
            }
        }


        private bool _isRiskLevelSupport;

        public bool IsRiskLevelSupport
        {
            get { return _isRiskLevelSupport; }
            set
            {
                _isRiskLevelSupport = value;
                OnPropertyChanged();
            }
        }

        public void SetStringFormats()
        {
            AllTradesStringFormat = GetStringFormat(AnalyticItem.Feature);
            LongTradesStringFormat = GetStringFormat(AnalyticItem.Feature);
            ShortTradesStringFormat = GetStringFormat(AnalyticItem.Feature);
        }


        public SummaryItem(AnalyticalFeature type)
        {
            AnalyticItem = new AnalyticItem(type);
            SetStringFormats();
            IsLastItem = false;
        }

        private string GetStringFormat(AnalyticalFeature analyticType, bool isWpf = true)
        {
            switch (analyticType)
            {
                case AnalyticalFeature.StartDate: 
                case AnalyticalFeature.EndDate: return "dd.MM.yyyy";


                case AnalyticalFeature.AverageTradeProfit:
                case AnalyticalFeature.AverageLoosingTrade:
                case AnalyticalFeature.AverageWinningTrade:
                case AnalyticalFeature.LargestLoosingTrade:
                case AnalyticalFeature.LargestWinningTrade:
                case AnalyticalFeature.NetProfitSum:
                /*case AnalyticalFeature.MaxDrawDown:
                case AnalyticalFeature.EquityHighs:
                case AnalyticalFeature.MaxDrawDownDays:*/
                    if (isWpf)
                        return "{0:##}";
                    else
                    {
                        return "0.##";
                    }

                case AnalyticalFeature.Trades:
                case AnalyticalFeature.LargestLoosingTradeNo:
                case AnalyticalFeature.LargestLoosingTradePercentNo:
                case AnalyticalFeature.LargestWinningTradeNo:
                case AnalyticalFeature.LargestWinningTradePercentNo:
                case AnalyticalFeature.AverageTradesInYear:
                case AnalyticalFeature.AverageTradeDurationInDays:
                    if (isWpf)
                        return "{0:0.##}";
                    else
                    {
                        return "0.##";
                    }

                case AnalyticalFeature.MaxConsLoss:
                case AnalyticalFeature.MaxConsWins:
                    if (isWpf)
                    {
                        return "{0}";
                    }
                    else
                    {
                        return "0";
                    }

                default:
                    if (isWpf)
                    {
                        return "{0:0.00}";
                    }
                    else
                    {
                        return "0.00";
                    }
                    
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}