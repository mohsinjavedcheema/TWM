using System;

namespace Twm.Chart.DrawObjects
{
    public class TradeDraw
    {
        public DateTime EntryTime { get; set; }
        public double EntryPrice { get; set; }
        
        public double? ExitPrice { get; set; }
        public int EntryBar { get; set; }
        public int? ExitBar { get; set; }
        public bool IsLong { get; set; }
        public bool IsProfit { get; set; }

        public string EntryOrderName { get; set; }

        public string ExitOrderName { get; set; }

        public double EntryQuantity { get; set; }

        public double ExitQuantity { get; set; }


    }
}