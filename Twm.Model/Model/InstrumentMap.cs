
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Twm.Model.Model
{
    [Table("InstrumentMaps")]
    public class InstrumentMap
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Browsable(false)]
        public int Id { get; set; }

        public int FirstInstrumentId { get; set; }
        public Instrument FirstInstrument { get; set; }

        public int SecondInstrumentId { get; set; }
        public Instrument SecondInstrument { get; set; }

    }
}
