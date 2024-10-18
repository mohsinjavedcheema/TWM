using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Twm.Core.DataCalc.Commissions;
using Twm.Core.Market;

namespace Twm.Custom.Commissions
{
    public class PerTradePercentSpot : Commission
    {

        private const string CommissionName = "Per Trade % Spot";

        [Browsable(true)]
        [Display(Name = "Commission Taker", Order = 0)]
        [DataMember]
        public double CommissionTaker { get; set; }

        [Browsable(true)]
        [Display(Name = "Commission Maker", Order = 1)]
        [DataMember]
        public double CommissionMaker { get; set; }


        public PerTradePercentSpot()
        {
            CommissionTaker = 0.1;
            CommissionMaker = 0.1;
        }

        public override double GetCommission(Trade trade)
        {
            var entry = 0d;
            var exit = 0d;

            if (IsTaker(trade.EntryOrder))
            {
                entry = (trade.ExitQuantity * trade.EntryPrice)*CommissionMaker/100;
            }
            if (trade.EntryOrder.OrderType == Core.Enums.OrderType.Market)
            {
                entry = (trade.ExitQuantity * trade.ExitPrice)*CommissionTaker/100;
            }

            return entry + exit;
        }

        private bool IsMaker(Order order)
        {
            return  order.OrderType == Core.Enums.OrderType.Limit ||
                    order.OrderType == Core.Enums.OrderType.StopLimit;
        }

        private bool IsTaker(Order order)
        {
            return  order.OrderType == Core.Enums.OrderType.Market ||
                    order.OrderType == Core.Enums.OrderType.StopMarket;
        }
    }
}