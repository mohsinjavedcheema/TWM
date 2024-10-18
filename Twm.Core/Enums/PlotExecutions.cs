using System.ComponentModel;

namespace Twm.Core.Enums
{
    public enum PlotExecutions
    {
        [Description("Do not plot")]
        DoNotPlot = 0,

        [Description("Text and markers")]
        TextAndMarkers = 1,

        [Description("Markers only")]
        MarkersOnly = 2
    }
}