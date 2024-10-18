using System.Runtime.Serialization;



namespace Twm.Core.DataProviders.Bybit.Models.Response
{
    /// <summary>
    /// Result Response following a call to the Create Order endpoint
    /// </summary>
    [DataContract]
    public partial class NewOrderResponse 
    {
        [DataMember(Order = 1, Name = "retCode")]
        public int RetCode { get; set; }

        [DataMember(Order = 2, Name = "retMsg")]
        public string RetMsg { get; set; }

        [DataMember(Order = 3, Name = "result")]
        public NewOrderResult Result { get; set; }

    }
}