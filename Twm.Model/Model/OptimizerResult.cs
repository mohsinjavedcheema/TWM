using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Twm.Model.Model
{
    [Table("OptimizerResults")]
    public class OptimizerResult : ICloneable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Guid { get; set; }

        public string StrategyGuid { get; set; }

        public string StrategyVersion { get; set; }

        public string Symbol { get; set; }
        public string DataSeriesType { get; set; }
        public int DataSeriesValue { get; set; }

        public DateTime DateCreated { get; set; }


        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}