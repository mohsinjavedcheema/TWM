using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Twm.Model.Model
{
    [Table("SystemOptions")]
    [JsonObject(MemberSerialization.OptIn)]
    public class SystemOption
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string Code { get; set; }
        public string Category { get; set; }
        public string Group { get; set; }
        public string ValueType { get; set; }

        [JsonProperty]
        public string Value { get; set; }
        public DateTime ValueDate { get; set; }
        public double ValueDouble { get; set; }
        public int ValueInt { get; set; }
        public bool ValueBool { get; set; }

    }
}