using System;

namespace Twm.Core.Helpers
{
    public static class MathHelper
    {
        
        public static double RoundToTickSize(this double value, double tickSize)
        {

            var diff = value % tickSize;

            if (diff == 0)
                return value;

            var val1 = value - diff;
            var val2 = val1 + tickSize;


            if (diff < val2 - value)
            {
                return val1;
            }
            

            return val2;
        }
    }
}