using System;
using System.Runtime.Serialization;
using Twm.Core.Classes;
using Twm.Core.Enums;
using Twm.Core.Interfaces;

namespace Twm.ViewModels.Presets
{
    [DataContract]
    public class InstrumentPreset
    {
        [DataMember]
        public string Symbol { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string ConnectionCode { get; set; }

        [DataMember]
        public int DataSeriesValue { get; set; }
        [DataMember]
        public DataSeriesType DataSeriesType { get; set; }

        [DataMember] 
        public DataSeriesValue DataSeriesFormat { get; set; }

        [DataMember]
        public DateTime PeriodStart { get; set; }

        [DataMember]
        public DateTime PeriodEnd { get; set; }

        [DataMember]
        public TimeFrameBase TimeFrameBase { get; set; }

        [DataMember]
        public int DaysToLoad { get; set; }

        [DataMember]
        public ValidatorStrategyPreset Strategy { get; set; }

    }
}