using System;
using System.ComponentModel;
using AlgoDesk.Core.DataProviders.DxFeed;
using AlgoDesk.Model.Model.Interfaces;

namespace AlgoDesk.Core.DataProviders.DataReaders.Csv.Map.DxFeed.Classes
{
    public class ProductIns : IDxFeedInstrument
    {

        public string DpId { get; set; }
        public string Type { get; set; }

        public string Symbol { get; set; }

        public string Description { get; set; }

        public string LocalSymbol { get; set; }

        public string LocalDescription { get; set; }

        public string Country { get; set; }

        [Description("Official Place Of Listing")]
        public string Opol { get; set; }

        public string ExchangeData { get; set; }

        public string Exchanges { get; set; }

        public string Currency { get; set; }

        public string BaseCurrency { get; set; }

        [Description("Classification of Financial Instruments")]
        public string Cfi { get; set; }

        [Description("International Securities Identifying Number")]
        public string Isin { get; set; }

        [Description("Stock Exchange Daily Official List")]
        public string Sedol { get; set; }

        [Description("Committee on Uniform Security Identification Procedures")]
        public string Cusip { get; set; }

        [Description("Industry Classification Benchmark")]
        public double? Icb { get; set; }

        [Description("Standard Industrial Classification")]
        public double? Sic { get; set; }

        public double? Multiplier { get; set; }

        public string Product { get; set; }

        public string Underlying { get; set; }

        public double? Spc { get; set; }

        public string AdditionalUnderlying { get; set; }

        [Description("Maturity month-year ")]
        public string Mmy { get; set; }

        public DateTime? Expiration { get; set; }

        public DateTime? LastTrade { get; set; }

        public double? Strike { get; set; }

        public string ExpirationStyle { get; set; }

        public string SettlementStyle { get; set; }

        public string PriceIncrements { get; set; }

        public string TradingHours { get; set; }

        public DateTime? FirstInterestDate { get; set; }

        public double? InterestRate { get; set; }

        public DateTime? IssueDate { get; set; }
    }
}