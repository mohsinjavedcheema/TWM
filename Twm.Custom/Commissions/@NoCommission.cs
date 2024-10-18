using Twm.Core.DataCalc.Commissions;
using Twm.Core.Market;

namespace Twm.Custom.Commissions
{
    public class NoCommission: Commission
    {
        private const string CommissionName = "No commission";


        public override double GetCommission(Trade trade)
        {
            return 0;
        }
    }
}