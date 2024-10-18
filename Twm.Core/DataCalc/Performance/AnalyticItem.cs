using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
//using System.Windows;
using Twm.Core.Enums;


namespace Twm.Core.DataCalc.Performance
{
    public class AnalyticItem : INotifyPropertyChanged
    {
        public AnalyticalFeature Feature { get; set; }

        private static Dictionary<AnalyticalFeature, string> _descriptions = new Dictionary<AnalyticalFeature, string>()
        {

            {AnalyticalFeature.None, "None"},
            {AnalyticalFeature.StartDate, "Date first trade"},
            {AnalyticalFeature.EndDate, "Date last trade"},
            {AnalyticalFeature.NetProfitSum, "Net profit $"},
            {AnalyticalFeature.MaxDrawDown, "Max draw down"},
            {AnalyticalFeature.MaxDrawDownPercent, "Max draw down %"},
            {AnalyticalFeature.MaxDrawDownDays, "Max draw down days"},
            {AnalyticalFeature.MaxConsLoss, "Max cons loss"},
            {AnalyticalFeature.MaxConsWins, "Max cons wins"},

            {AnalyticalFeature.Trades, "Total trades"},
            {AnalyticalFeature.TradesInProfit, "Wins %"},

            {AnalyticalFeature.AverageTradeProfit, "Average trade $"},
            {AnalyticalFeature.AverageTradePercent, "Average trade %"},
            {AnalyticalFeature.AverageWinningTrade, "Average winning trade $"},
            {AnalyticalFeature.AverageWinningTradePercent, "Average winning trade %"},
            {AnalyticalFeature.LargestWinningTrade, "Largest winning trade $"},
            {AnalyticalFeature.LargestWinningTradeNo, "Largest winning trade num"},
            {AnalyticalFeature.LargestWinningTradePercent, "Largest winning trade %"},
            {AnalyticalFeature.LargestWinningTradePercentNo, "Largest winning trade % num"},
            {AnalyticalFeature.AverageLoosingTrade, "Average loosing trade $"},
            {AnalyticalFeature.AverageLoosingTradePercent, "Average loosing trade %"},
            
            {AnalyticalFeature.LargestLoosingTrade, "Largest loosing trade $"},
            
            {AnalyticalFeature.LargestLoosingTradeNo, "Largest loosing trade num"},
            {AnalyticalFeature.LargestLoosingTradePercent, "Largest loosing trade %"},
            {AnalyticalFeature.LargestLoosingTradePercentNo, "Largest loosing trade % num"},

            {AnalyticalFeature.AverageTradesInYear, "Average trades in year"},
            {AnalyticalFeature.AverageTradeDurationInDays, "Average trade duration in days"},
            {AnalyticalFeature.Calmar, "Calmar"},
            {AnalyticalFeature.Sharpe, "Sharpe"},
            {AnalyticalFeature.Sortino, "Sortino"},
            {AnalyticalFeature.Expectancy, "Expectancy"},
            {AnalyticalFeature.ProfitFactor, "Profit Factor"},
            {AnalyticalFeature.Commission, "Commission"},
            {AnalyticalFeature.EquityHighs, "Equity Highs"},

        };
    



        private object _displayName;
        private object _hint;

        public object DisplayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value;
                OnPropertyChanged();
            }
        }

        public object Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public AnalyticItem(AnalyticalFeature feature)
        {
            Feature = feature;
            DisplayName = GetNameByType(feature);
            Hint = GetHintByFeature(feature);

        }

        private string GetNameByType(AnalyticalFeature feature)
        {
            if (_descriptions.ContainsKey(feature))
                return _descriptions[feature];

            return Enum.GetName(typeof(AnalyticalFeature), feature);
        }

        private string GetHintByFeature(AnalyticalFeature feature)
        {/*
            if (feature == AnalyticalFeature.NetProfitSum)
                return Application.Current.Resources["np"].ToString();

            if (feature == AnalyticalFeature.Sharpe)
                return Application.Current.Resources["sharpe"].ToString();

            if (feature == AnalyticalFeature.ProfitFactor)
                return Application.Current.Resources["pf"].ToString();*/


            return null;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}