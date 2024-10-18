using System;
using Twm.Core.Enums;
using Twm.Core.Interfaces;
using Newtonsoft.Json;

namespace Twm.Core.Market
{
    public class Trade : ITrade, ICloneable
    {
        [JsonIgnore]
        public int Id { get; set; }

        public string Instrument { get; set; }

        public string Strategy { get; set; }

        public string StrategyId { get; set; }

        public int EntryBar { get; set; }
        public int ExitBar { get; set; }

        public MarketPosition MarketPosition { get; set; }

        public DateTime EntryTime { get; set; }
        public DateTime ExitTime { get; set; }

        public double EntryPrice { get; set; }
        public double ExitPrice { get; set; }

        [JsonIgnore]
        public Order EntryOrder { get; set; }
        [JsonIgnore]
        public Order ExitOrder { get; set; }


        public string EntryName { get; set; }
        public string ExitName { get; set; }


        public double EntryQuantity { get; set; }

        public double ExitQuantity { get; set; }

        public bool IsClosed { get; set; }

        public bool IsProcessed { get; set; }
        public double TradeNumber { get; set; }
        public string GlobalTradeId { get; set; }

        public double Profit { get; set; }

        public double Commission { get; set; }

        public double CumProfit { get; set; }

        public double TradePercent { get; set; }

        public double CumProfitBefore { get; set; }

        public double CurrentDaysNoEquityHigh { get; set; }

        public double CurrentDrawDown { get; set; }

        public double Mae { get; set; }

        public double Mfe { get; set; }

        public double MaxHighPriceWhileOpen { get; set; }

        public double MinLowPriceWhileOpen { get; set; }

        public int PeriodNum { get; set; }

        public string Guid { get; set; }

        public Trade()
        {
            ExitTime = DateTime.MaxValue;
            Guid = System.Guid.NewGuid().ToString();
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public Trade CopyFrom(ITrade trade)
        {
            Id = trade.Id;
            Instrument = trade.Instrument;
            Strategy = trade.Strategy;
            StrategyId = trade.StrategyId;

            EntryBar = trade.EntryBar;
            ExitBar = trade.ExitBar;
            MarketPosition = trade.MarketPosition;
            EntryTime = trade.EntryTime;
            ExitTime = trade.ExitTime;
            EntryPrice = trade.EntryPrice;
            ExitPrice = trade.ExitPrice;
            EntryName = trade.EntryName;
            ExitName = trade.ExitName;
            EntryQuantity = trade.EntryQuantity;
            ExitQuantity = trade.ExitQuantity;

            IsClosed = trade.IsClosed;
            IsProcessed = trade.IsProcessed;
            TradeNumber = trade.TradeNumber;
            GlobalTradeId = trade.GlobalTradeId;
            Profit = trade.Profit;

            Commission = trade.Commission;

            CumProfit = trade.CumProfit;
            TradePercent = trade.TradePercent;

            CumProfitBefore = trade.CumProfitBefore;
            CurrentDaysNoEquityHigh = trade.CurrentDaysNoEquityHigh;
            CurrentDrawDown = trade.CurrentDrawDown;
            Mae = trade.Mae;
            Mfe = trade.Mfe;
            MaxHighPriceWhileOpen = trade.MaxHighPriceWhileOpen;
            MinLowPriceWhileOpen = trade.MinLowPriceWhileOpen;
            PeriodNum = trade.PeriodNum;
            Guid = trade.Guid;

            return this;

        }
    }
}