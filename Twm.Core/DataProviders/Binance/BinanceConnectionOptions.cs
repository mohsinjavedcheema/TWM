using Twm.Core.Attributes;
using Twm.Core.DataProviders.Common;
using Twm.Core.DataProviders.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Twm.Core.DataProviders.Binance
{
    public sealed class BinanceConnectionOptions : ConnectionOptionsBase
    {
        [CustomConnectionOption]
        [Display(Name = "API Key", GroupName = "Credential", Order = 0)]
        public string APIKey { get; set; }

        [CustomConnectionOption]
        [Display(Name = "API Secret", GroupName = "Credential", Order = 1)]
        public string APISecret { get; set; }

        [CustomConnectionOption]       
        [Display(Name = "Connection type", GroupName = "Connection", Order = 0)]
        public ConnectionType ConnectionType { get;  private set; }


        public void SetConnectionType(ConnectionType connectionType)
        {
            ConnectionType = connectionType;
        }

    }
}
