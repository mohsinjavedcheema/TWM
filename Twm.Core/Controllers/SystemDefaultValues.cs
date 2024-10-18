using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Twm.Core.Classes;
using Twm.Core.Enums;

namespace Twm.Core.Controllers
{
    public static class SystemDefaultValues
    {
        public static string ThemeExtension = "thm";
        public static string PathToProject = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Twm\\bin\\Custom";
        public static string PathToProjectDll = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Twm\\bin\\Custom\\bin\\Debug";

        public static string ApiServerUrl = "https://algo-desk.com";

        public static Color TradeBuyColor = Colors.Blue;
        public static Color MarkerTextColor = Colors.Black;

        public static Color TradeSellColor = Colors.Magenta;
        public static Color DownBarColor = Colors.Red;
        public static Color IndicatorSeparatorColor = Colors.LightGray;

        public static int IndicatorSeparatorWidth = 1;
        public static Color UpBarColor = Colors.LimeGreen;
        public static Color CandleOutlineColor = Colors.Black;
        public static Color CandleWickColor = Colors.Black;
        public static Color TextColor = Colors.Black;

        public static Color ChartBackgroundColor = Colors.White;
        public static Color ChartVGridColor = Colors.LightGray;
        public static Color ChartHGridColor = Colors.LightGray;

        public static PlotExecutions PlotExecutions = PlotExecutions.TextAndMarkers;


        public static Dictionary<string, List<GridColumnInfo>> ViewOptions = new Dictionary<string, List<GridColumnInfo>>()
        {
            { "OptimizerPeriodStrategies",
                new List<GridColumnInfo>
                {
                    new GridColumnInfo("Parameters", "Parameters"),
                    new GridColumnInfo("PerformanceValue", "Performance value"),
                    new GridColumnInfo("Trades", "Trades"),
                    new GridColumnInfo("NetProfit", "Net Profit"),
                    new GridColumnInfo("CommissionValue", "Commission value", Visibility.Collapsed),
                    new GridColumnInfo("ProfitFactor", "Profit Factor", Visibility.Collapsed),
                    new GridColumnInfo("Sharpe", "Sharpe", Visibility.Collapsed),
                    new GridColumnInfo("EquityHighs", "Equity highs", Visibility.Collapsed),
                    new GridColumnInfo("MaxDrawDown", "Max DrawDown"),
                    new GridColumnInfo("MaxDrawDownDays", "Max DrawDown Days", Visibility.Collapsed),
                    new GridColumnInfo("StartDate", "Start Date", Visibility.Collapsed),
                    new GridColumnInfo("EndDate", "End Date", Visibility.Collapsed),

                    new GridColumnInfo("MaxConsLoss", "Max Cons Loss"),
                    new GridColumnInfo("MaxConsWins", "Max Cons Wins", Visibility.Collapsed),

                    new GridColumnInfo("WinPercent", "Win%"),

                    new GridColumnInfo("AverageTradesInYear", "Average trades in year", Visibility.Collapsed),
                    new GridColumnInfo("AverageTradeDurationInDays", "Average trade duration in days", Visibility.Collapsed),

                    new GridColumnInfo("AverageTradeProfit", "Average trade profit", Visibility.Collapsed),

                    new GridColumnInfo("AverageWinningTrade", "Average winning trade", Visibility.Collapsed),
                    new GridColumnInfo("LargestWinningTrade", "Largest winning trade", Visibility.Collapsed),
                    new GridColumnInfo("AverageLoosingTrade", "Average loosing trade", Visibility.Collapsed),
                    new GridColumnInfo("LargestLoosingTrade", "Largest loosing trade", Visibility.Collapsed),


                    new GridColumnInfo("CustomValue", "Custom value")
                } },
            {
                "BinanceInstruments",
                new List<GridColumnInfo>
                {
                    new GridColumnInfo("Symbol", "Symbol"),
                    new GridColumnInfo("Base", "Base"),
                    new GridColumnInfo("Quote", "Quote"),
                    new GridColumnInfo("Vol24", "Vol24"),
                    new GridColumnInfo("LastPrice", "LastPrice"),
                }
            },
             {
                "BybitInstruments",
                new List<GridColumnInfo>
                {
                    new GridColumnInfo("Symbol", "Symbol"),
                    new GridColumnInfo("Base", "Base"),
                    new GridColumnInfo("Quote", "Quote"),
                    new GridColumnInfo("Vol24", "Vol24"),
                    new GridColumnInfo("LastPrice", "LastPrice"),
                }
            }
        };

    }
}