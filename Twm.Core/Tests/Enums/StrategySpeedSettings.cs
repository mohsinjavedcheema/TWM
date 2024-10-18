using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twm.Core.Tests.Enums
{
    public enum StrategySpeedSettings
    {
        NotSet = 0,
        T01_BlankExecution = 1,
        T02_SimpleMACrossOverStrategy = 2,
        T03_MACrossOverLimitOrderEntry = 3,
        T04_SMACrossOverSLTPTest = 4
    }
}
