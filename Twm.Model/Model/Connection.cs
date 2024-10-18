using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Twm.Model.Model
{


    [Table("Connections")]
    public class Connection
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int DataProviderId { get; set; }
        public bool IsSystem { get; set; }
        public DataProvider DataProvider { get; set; }
        public List<ConnectionOption> Options { get; set; }
        public List<Instrument> Instruments { get; set; }
    }

}