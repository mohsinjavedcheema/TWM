using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twm.Chart
{
    public static class DoubleHelper
    {
        public static int GetDecimalCount(this double val)
        {
            int i = 0;
            while (Math.Round(val, i) != val)
                i++;
            return i;
        }
    }
}
