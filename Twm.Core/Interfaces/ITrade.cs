using System;
using Twm.Core.Enums;
using Twm.Core.Market;
using Newtonsoft.Json;

namespace Twm.Core.Interfaces
{
    public interface ITrade
    {
        int Id { get; set; }
        string Instrument { get; set; }

        string Strategy { get; set; }

        string StrategyId { get; set; }

        int EntryBar { get; set; }
        int ExitBar { get; set; }

        MarketPosition MarketPosition { get; set; }

        DateTime EntryTime { get; set; }
        DateTime ExitTime { get; set; }

        double EntryPrice { get; set; }
        double ExitPrice { get; set; }


        string EntryName { get; set; }
        string ExitName { get; set; }


        double EntryQuantity { get; set; }

        double ExitQuantity { get; set; }

        bool IsClosed { get; set; }

        bool IsProcessed { get; set; }
        double TradeNumber { get; set; }
        string GlobalTradeId { get; set; }

        double Profit { get; set; }

        double Commission { get; set; }

        double CumProfit { get; set; }

        double TradePercent { get; set; }

        double CumProfitBefore { get; set; }

        double CurrentDaysNoEquityHigh { get; set; }

        double CurrentDrawDown { get; set; }

        double Mae { get; set; }

        double Mfe { get; set; }

        double MaxHighPriceWhileOpen { get; set; }

        double MinLowPriceWhileOpen { get; set; }

        int PeriodNum { get; set; }

        string Guid { get; set; }
    }
}