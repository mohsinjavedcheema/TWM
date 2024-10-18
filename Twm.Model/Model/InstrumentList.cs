using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Twm.Model.Model
{

    [Table("InstrumentLists")]
    public class InstrumentList: ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Browsable(false)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public bool IsDefault { get; set; }

        public int ConnectionId { get; set; }

        public Connection Connection { get; set; }

        public List<InstrumentInstrumentList> InstrumentInstrumentLists { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}