using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Twm.Model.Model.Interfaces;

namespace Twm.Model.Model
{
    [Table("Instruments")]
    public class Instrument :IInstrument, ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Browsable(false)]
        public int Id { get; set; }

        [Description("Unique identifier in the provider's system")]
        [Category("Main")]
        public string DpId { get; set; }

        [Description("Instrument type")]
        [Category("Main")]
        public string Type { get; set; }

        [DataMember]
        [Category("Main")]
        public string Symbol { get; set; }//

        [DataMember]
        [Category("Main")]
        public string Base { get; set; }//

        [DataMember]
        [Category("Main")]
        public string Quote { get; set; }//

        [Category("Main")]
        public string Description { get; set; }


        [Category("Main")]
        public double? MinLotSize { get; set; }//       


        [Category("Main")]
        public double? Notional { get; set; }//       




        [Category("Main")]
        public double? Multiplier { get; set; }//       

        [Category("Main")]
        public string PriceIncrements { get; set; }//

        [Category("Main")]
        public string TradingHours { get; set; }

       
        public string ProviderData { get; set; }


        [Browsable(false)]
        public int ConnectionId { get; set; }
        

        [Browsable(false)]
        public List<InstrumentInstrumentList> InstrumentInstrumentLists { get; set; }


        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}