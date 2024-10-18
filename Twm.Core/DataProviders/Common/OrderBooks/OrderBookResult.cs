using System;
using System.Collections.Generic;
using Twm.Core.DataProviders.Enums;

namespace Twm.Core.DataProviders.Common.OrderBooks
{
    public class OrderBookResult
    {
        public DateTime Time;

        public OrderBook Snapshot { get; set; }

        public Queue<Dictionary<PriceLevelOperation, OrderBook>> PriceLevelsQueue { get; set; }

        public Dictionary<PriceLevelOperation, OrderBook> PriceLevelBooks { get; set; }

        public bool IsNewBook { get; set; }     

        public bool IsIncrement { get; set; }


        public OrderBookResult()
        {
            PriceLevelBooks = new Dictionary<PriceLevelOperation, OrderBook>();
            PriceLevelsQueue = new Queue<Dictionary<PriceLevelOperation, OrderBook>>();
        }
    }
}
