using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twm.Core.DataProviders.Binance.Models
{
    public class Filter
    {
        public string FilterType { get; set; }

        public double MinPrice { get; set; }
        public double MaxPrice { get; set; }
        public double TickSize { get; set; }
        public double MultiplierUp { get; set; }
        public double MultiplierDown { get; set; }
        public double AvgPriceMins { get; set; }

        public double MinQty { get; set; }

        public double MaxQty { get; set; }

        public double StepSize { get; set; }

        public double MinNotional { get; set; }

        public bool ApplyToMarket { get; set; }

        public int Limit { get; set; }
        public int MaxNumAlgoOrders { get; set; }
        public int MaxNumOrders { get; set; }

    }
}
