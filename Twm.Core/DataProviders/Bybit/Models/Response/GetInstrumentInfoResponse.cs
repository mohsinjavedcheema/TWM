
using System.Runtime.Serialization;
using Twm.Core.DataProviders.Bybit.Models.Classes;
using Twm.Core.DataProviders.Interfaces;


namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    [DataContract]    
    public class GetInstrumentInfoResponse : IResponse
    {
        [DataMember(Order = 1, Name = "retCode")]
        public int RetCode { get; set; }

        [DataMember(Order = 2, Name = "retMsg")]
        public string RetMsg { get; set; }

        [DataMember(Order = 3, Name = "result")]
        public InstrumentInfoList Result { get; set; }
    }
}