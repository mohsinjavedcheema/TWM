using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Twm.Core.Converters;
using Twm.Core.Market;
using Newtonsoft.Json;

namespace Twm.Core.DataCalc.Commissions
{
    [DataContract]
    [JsonConverter(typeof(CommissionJsonConverter))]
    public abstract class Commission : DataCalcObject, ICloneable
    {

        [DataMember]
        [Browsable(false)]
        public string TypeName
        {
            get { return this.GetType().Name; }
        }

        public abstract double GetCommission(Trade trade);

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override string ToString()
        {
            return "";
        }
    }
}