using System.ComponentModel;

namespace Twm.Core.Enums
{
    public enum StrategyPerformanceSection
    {
        [Description("Summary")]
        Summary,
        [Description("Chart")]
        Chart,
        [Description("Analysis")]
        Analysis,
        /*[Description("Executions")]
        Executions,*/
        [Description("Trades ($)")]
        Trades,
        [Description("Orders ($)")]
        Orders
       
    }
}