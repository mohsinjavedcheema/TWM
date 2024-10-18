using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Twm.Model.Model
{
    [Table("Presets")]
    public class Preset : ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int Type { get; set; }

        public string Name { get; set; }

        public string Guid { get; set; }

        public string Data { get; set; }


        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}