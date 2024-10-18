using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twm.Core.Tests.Enums
{
    public enum StrategyValueCheckSettings
    {
        NotSet = 0,
        T01_SimpleMACrossOverStrategy = 1,
        T02_MACrossOverLimitOrderEntry = 2,
        T03_SMACrossOverSLTPTest = 3
    }
}
