using System.Collections.Generic;
using System.Linq;
using AlgoDesk.Core.DataProviders.DxFeed;
using AlgoDesk.Core.DataProviders.DxFeed.Models;
using AlgoDesk.Model.Model.Interfaces;
using CsvHelper.Configuration;

namespace AlgoDesk.Core.DataProviders.DataReaders.Csv.Map.DxFeed.MapHeaders
{
    public sealed class InstrumentMapHeader<T> : ClassMap<T>
    {
        private readonly Dictionary<string, int> _dictionary;

        public InstrumentMapHeader(Dictionary<string, int> dictionary)
        {
            _dictionary = dictionary;

            var type = typeof(IDxFeedInstrument);
            var members = type.GetMembers();
            Map(type, members.FirstOrDefault(x => x.Name == "Type")).Index(GetIndex("TYPE"));
            Map(type, members.FirstOrDefault(x => x.Name == "Symbol")).Index(GetIndex("SYMBOL"));
            Map(type, members.FirstOrDefault(x => x.Name == "Description")).Index(GetIndex("DESCRIPTION"));
            Map(type, members.FirstOrDefault(x => x.Name == "LocalSymbol")).Index(GetIndex("LOCAL_SYMBOL"));
            Map(type, members.FirstOrDefault(x => x.Name == "LocalDescription")).Index(GetIndex("LOCAL_DESCRIPTION"));
            Map(type, members.FirstOrDefault(x => x.Name == "Country")).Index(GetIndex("COUNTRY"));
            Map(type, members.FirstOrDefault(x => x.Name == "Opol")).Index(GetIndex("OPOL"));
            Map(type, members.FirstOrDefault(x => x.Name == "ExchangeData")).Index(GetIndex("EXCHANGE_DATA"));
            Map(type, members.FirstOrDefault(x => x.Name == "Exchanges")).Index(GetIndex("EXCHANGES"));
            Map(type, members.FirstOrDefault(x => x.Name == "Currency")).Index(GetIndex("CURRENCY"));
            Map(type, members.FirstOrDefault(x => x.Name == "BaseCurrency")).Index(GetIndex("BASE_CURRENCY"));
            Map(type, members.FirstOrDefault(x => x.Name == "Cfi")).Index(GetIndex("CFI"));
            Map(type, members.FirstOrDefault(x => x.Name == "Isin")).Index(GetIndex("ISIN"));
            Map(type, members.FirstOrDefault(x => x.Name == "Sedol")).Index(GetIndex("SEDOL"));
            Map(type, members.FirstOrDefault(x => x.Name == "Cusip")).Index(GetIndex("CUSIP"));
            Map(type, members.FirstOrDefault(x => x.Name == "Icb")).Index(GetIndex("ICB"));
            Map(type, members.FirstOrDefault(x => x.Name == "Sic")).Index(GetIndex("SIC"));
            Map(type, members.FirstOrDefault(x => x.Name == "Multiplier")).Index(GetIndex("MULTIPLIER"));
            Map(type, members.FirstOrDefault(x => x.Name == "Product")).Index(GetIndex("PRODUCT"));
            Map(type, members.FirstOrDefault(x => x.Name == "Underlying")).Index(GetIndex("UNDERLYING"));
            Map(type, members.FirstOrDefault(x => x.Name == "Spc")).Index(GetIndex("SPC"));
            Map(type, members.FirstOrDefault(x => x.Name == "AdditionalUnderlying")).Index(GetIndex("ADDITIONAL_UNDERLYINGS"));
            Map(type, members.FirstOrDefault(x => x.Name == "Mmy")).Index(GetIndex("MMY"));
            Map(type, members.FirstOrDefault(x => x.Name == "Expiration")).Index(GetIndex("EXPIRATION")).TypeConverterOption.Format("yyyy-MM-dd"); 
            Map(type, members.FirstOrDefault(x => x.Name == "LastTrade")).Index(GetIndex("LAST_TRADE")).TypeConverterOption.Format("yyyy-MM-dd");
            Map(type, members.FirstOrDefault(x => x.Name == "Strike")).Index(GetIndex("STRIKE"));
            Map(type, members.FirstOrDefault(x => x.Name == "ExpirationStyle")).Index(GetIndex("EXPIRATION_STYLE"));
            Map(type, members.FirstOrDefault(x => x.Name == "SettlementStyle")).Index(GetIndex("SETTLEMENT_STYLE"));
            Map(type, members.FirstOrDefault(x => x.Name == "PriceIncrements")).Index(GetIndex("PRICE_INCREMENTS"));
            Map(type, members.FirstOrDefault(x => x.Name == "TradingHours")).Index(GetIndex("TRADING_HOURS"));
            Map(type, members.FirstOrDefault(x => x.Name == "FirstInterestDate")).Index(GetIndex("FIRST_INTEREST_DATE")).TypeConverterOption.Format("yyyy-MM-dd");
            Map(type, members.FirstOrDefault(x => x.Name == "InterestRate")).Index(GetIndex("INTEREST_RATE"));
            Map(type, members.FirstOrDefault(x => x.Name == "IssueDate")).Index(GetIndex("ISSUE_DATE")).TypeConverterOption.Format("yyyy-MM-dd");
        }


        private int GetIndex(string name)
        {
            if (!_dictionary.TryGetValue(name, out var index))
            {
                index = -1;
            }

            return index;
        }
    }
}