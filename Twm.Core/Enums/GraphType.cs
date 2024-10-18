using System.ComponentModel;

namespace Twm.Core.Enums
{
    public enum GraphType
    {
        [Description("Equity Trades")]
        Equity = 1,

        [Description("Portfolio equity")]
        PortfolioEquity = 2,

        [Description("Portfolio equity (compare)")]
        PortfolioEquityCompare = 3,

        [Description("Cumulative Draw Downs")]
        DrawDowns = 4,

        [Description("Cumulative Draw Downs (compare)")] 
        DrawDownsCompare = 5

    }
}