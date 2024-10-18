using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twm.Core.Tests.Enums
{
    public enum IndicatorValueCheckSettings
    {
        NotSet = 0,
        T01_SimpleEMAValuesCheck = 1,
        T02_EMACalculatedUsingSMAValuesCheck = 2,
        T03_SMAValuesCheck = 3,
        T04_EMACalculatedUsingLocalSMAValuesCheck = 4,
        T05_CalculatedEMAIsInputToSMAValuesCheck = 5,
        T06_MacdDiffValuesCheck = 6,
        T07_WMAValuesCheck = 7,
        T08_VWMAValuesCheck = 8,
        T09_TEMAValuesCheck = 9,
        T10_TMAValuesCheck = 10,
        T11_HMAValuesCheck = 11,

    }
}
