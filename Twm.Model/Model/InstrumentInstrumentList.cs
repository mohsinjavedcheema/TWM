using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Twm.Model.Model
{

    [Table("InstrumentInstrumentLists")]
    public class InstrumentInstrumentList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int InstrumentId { get; set; }
        public Instrument Instrument { get; set; }

        public int InstrumentListId { get; set; }
        public InstrumentList InstrumentList { get; set; }

    }
}