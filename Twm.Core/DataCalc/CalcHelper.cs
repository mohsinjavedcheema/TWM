using Twm.Core.Enums;
using Twm.Model.Model;

namespace Twm.Core.DataCalc
{
    public static  class CalcHelper
    {
        public static double CalcTradeProfit(double entryPrice, double exitPrice, MarketPosition mp, double qnt, double? multiplier)
        {
            double profit = 0;

            if (mp == MarketPosition.Long)
            {
                profit = (exitPrice - entryPrice) * qnt;
            }

            if (mp == MarketPosition.Short)
            {
                profit = (entryPrice - exitPrice) * qnt;
            }

            return profit * multiplier ?? 1.0;
        }
    }
}