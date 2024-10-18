using Twm.Core.Attributes;
using Twm.Core.DataProviders.Common;
using Twm.Core.DataProviders.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;

namespace Twm.Core.DataProviders.Bybit
{

    
    public sealed class BybitConnectionOptions : ConnectionOptionsBase
    {
        [CustomConnectionOption]
        [Display(Name = "API Key", GroupName = "Credential", Order = 0)]
        public string APIKey { get; set; }


        private string _apiSecret;        
        [CustomConnectionOption]
        [Display(Name = "API Secret", GroupName = "Credential", Order = 1)]
      
        public string APISecret 
        {
            get{
                return _apiSecret;
            }
            set
            {
                _apiSecret = value;
      
                
            }
        }

        [CustomConnectionOption]
        [Display(Name = "Connection type", GroupName = "Connection", Order = 0)]
        public ConnectionType ConnectionType { get; private set; }

        public void SetConnectionType(ConnectionType connectionType)
        {
            ConnectionType = connectionType;
        }
    }
}
