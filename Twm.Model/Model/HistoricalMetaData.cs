using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Twm.Model.Model
{

    [Table("HistoricalMetaDatas")]
    public class HistoricalMetaData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Symbol { get; set; }
        public string DataType { get; set; }
        public string InstrumentType { get; set; }
        public int DataSeriesValue { get; set; }
        public string DataSeriesType { get; set; }
        public bool IsTest { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int DataProviderId { get; set; }
        
        public DataProvider DataProvider { get; set; }
    }
}