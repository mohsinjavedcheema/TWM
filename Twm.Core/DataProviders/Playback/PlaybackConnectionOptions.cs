using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Twm.Core.DataProviders.Common;

namespace Twm.Core.DataProviders.Playback
{
    public sealed class PlaybackConnectionOptions : ConnectionOptionsBase
    {
        [Display(Name = "Name", GroupName = "Main", Order = 1)]
        [ReadOnly(true)]
        public override string Name { get; set; }


        [Browsable(false)]
        public override string DataProvider { get; set; }
    }
}
