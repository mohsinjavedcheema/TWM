using Twm.Core.DataCalc.Performance;
using Twm.Core.Enums;

namespace Twm.Core.Interfaces
{
    public interface IRiskLevel
    {
       AnalyticalFeature Feature { get; set; }
       double? ISValue { get; set; }
       double? OSValue { get; set; }

       double? SimValue { get; set; }
       double Value { get; set; }
       bool IsChecked { get; set; }
       bool IsFalse { get; set; }
       bool IsPercent { get; set;}

       double PainValue { get; set; }

        void SetPainValue(Summary summary);

        void SetGlobalPainValue(double value);

        object Clone();
    }
}