using System.Runtime.Serialization;



namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    /// <summary>
    /// Result Response following a call to the Modify Order endpoint
    /// </summary>
    [DataContract]
    public partial class ModifyOrderResponse 
    {
        [DataMember(Order = 1, Name = "retCode")]
        public int RetCode { get; set; }

        [DataMember(Order = 2, Name = "retMsg")]
        public string RetMsg { get; set; }

        [DataMember(Order = 3, Name = "result")]
        public ModifyOrderResult Result { get; set; }

    }
}