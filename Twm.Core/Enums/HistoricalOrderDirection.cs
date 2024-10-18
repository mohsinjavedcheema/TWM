using System.ComponentModel;

namespace Twm.Core.Enums
{
    public enum HistoricalOrderDirection
    {
        BreakOut = 0,
        PullBack = 1,
        Equal = 2,
        [Browsable(false)] Unknown = 99
    }

}