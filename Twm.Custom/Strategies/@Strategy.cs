using Twm.Core.DataCalc;
using Twm.Custom.Indicators;

namespace Twm.Custom.Strategies
{
    public partial class Strategy : StrategyBase
    {
		private Indicator indicator;

        public Strategy()
        {
            Init();
        }

        public sealed override void Init()
        {
            indicator = new Indicator { Parent = this };
        }

	}
}