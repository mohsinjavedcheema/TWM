using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twm.Core.Tests.Enums
{
    public enum IndicatorSpeedSettings
    {
        NotSet = 0,
        T01_BlankExecution = 1,
        T02_ValueAssignmentTest = 2,
        T03_EMACalculated = 3,
        T04_EMACalculatedUsingSMAInput = 4,
        T05_EMACalculatedUsingLocalSMA = 5,
        T06_CalculatedEMAIsInputToSMA = 6,
        T07_MacdDiff = 7,
        T08_WMASpeedTest = 8,
        T09_VWMASpeedTest = 9,
        T10_TEMASpeedTest = 10,
        T11_TMASpeedTest = 11,
        T12_HMASpeedTest = 12,
    }
}
