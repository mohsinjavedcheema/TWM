using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Converter;


namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    public class SpotResult
    {
        
        [JsonProperty("memberId")]
        public string MemberId { get; set; }

        
        [JsonProperty("accountType")]
        public string AccountType { get; set; }

        [JsonProperty("balance")]
        
        public List<AssetResult> Assets { get; set; }




    }


   
}
