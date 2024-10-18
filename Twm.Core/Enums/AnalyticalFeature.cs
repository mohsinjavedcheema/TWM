using System.ComponentModel;

namespace Twm.Core.Enums
{
    public enum AnalyticalFeature
    {
        None,
        [Description("Date first trade")]
        StartDate,
        [Description("Date last trade")]
        EndDate,
        [Description("Net profit $")]
        NetProfitSum,

        [Description("Max draw down")]
        MaxDrawDown,
        [Description("Max draw down %")]
        MaxDrawDownPercent,
        [Description("Max draw down days")]
        MaxDrawDownDays,

        [Description("Max cons loss")]
        MaxConsLoss,
        [Description("Max cons wins")]
        MaxConsWins,

        [Description("Total trades")]
        Trades,
        [Description("Wins %")]
        TradesInProfit,
        [Description("Average trade $")]
        AverageTradeProfit,
        [Description("Average trade %")]
        AverageTradePercent,
        [Description("Average winning trade $")]
        AverageWinningTrade,
        [Description("Average winning trade %")]
        AverageWinningTradePercent,
        [Description("Largest winning trade $")]
        LargestWinningTrade,
        [Description("Largest winning trade num")]
        LargestWinningTradeNo,
        [Description("Largest winning trade %")]
        LargestWinningTradePercent,
        [Description("Largest winning trade % num")]
        LargestWinningTradePercentNo,
        [Description("Average loosing trade $")]
        AverageLoosingTrade,
        [Description("Average loosing trade %")]
        AverageLoosingTradePercent,
        [Description("Largest loosing trade $")]
        LargestLoosingTrade,
        [Description("Largest loosing trade num")]
        LargestLoosingTradeNo,
        [Description("Largest loosing trade %")]
        LargestLoosingTradePercent,
        [Description("Largest loosing trade % num")]
        LargestLoosingTradePercentNo,
        [Description("Average trades in year")]
        AverageTradesInYear,
        [Description("Average trade duration in days")]
        AverageTradeDurationInDays,
        [Description("Calmar")]
        Calmar,
        [Description("Sharpe")]
        Sharpe,
        [Description("Sortino")]
        Sortino,
        [Description("Expectancy")]
        Expectancy,
        
        [Description("Profit Factor")]
        ProfitFactor,

        [Description("Commission")]
        Commission,

        [Description("Equity Highs")]
        EquityHighs,

        [Description("MAX MAE")]
        MaxMae


    }
}