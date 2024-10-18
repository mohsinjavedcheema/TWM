using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Twm.Model.Model
{
    [Table("ViewOptions")]
    public class ViewOption : ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Code { get; set; }

        public string Data { get; set; }


        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}