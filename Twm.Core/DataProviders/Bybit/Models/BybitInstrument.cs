using Twm.Core.DataProviders.Bybit.Enums;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Twm.Core.DataProviders.Bybit.Models
{
    [Serializable]
    public class BybitInstrument : IBybitInstrument
    {
        #region Main fields
        [XmlIgnore]
        [Category("Main")]
        public MarketType Type { get; set; }

        [XmlIgnore]
        [Category("Main")]
        public string Symbol { get; set; }






        #endregion region

        [Category("Provider specific")]
        public double MinLotSize { get; set; }

        [Category("Provider specific")]
        public double Notional { get; set; }

        [Category("Provider specific")]
        public string Pair { get; set; }

        [Category("Provider specific")]
        public string ContractType { get; set; }

        [Category("Provider specific")]        
        public DateTime DeliveryDate { get; set; }

        [Category("Provider specific")]
        public DateTime OnboardDate { get; set; }


        [Category("Provider specific")]
        public double TickSize { get; set; }

        [Category("Provider specific")]
        public string Status { get; set; }

        [Category("Provider specific")]
        public double MaintMarginPercent { get; set; }

        [Category("Provider specific")]
        public double RequiredMarginPercent { get; set; }


        [Category("Provider specific")]
        public string BaseAsset { get; set; }

        [Category("Provider specific")]
        public int BaseAssetPrecision { get; set; }

        [Category("Provider specific")]
        public string QuoteAsset { get; set; }

        [Category("Provider specific")]
        public string MarginAsset { get; set; }

        [Category("Provider specific")]
        public double PricePrecision { get; set; }

        [Category("Provider specific")]
        public double QuantityPrecision { get; set; }


        [Category("Provider specific")]
        public int QuotePrecision { get; set; }


        [Category("Provider specific")]
        public string UnderlyingType { get; set; }

        [Category("Provider specific")]
        public string[] UnderlyingSubType { get; set; }

        [Category("Provider specific")]
        public double SettlePlan { get; set; }

        [Category("Provider specific")]
        public double TriggerProtect { get; set; }

        [Category("Provider specific")]
        public double LiquidationFee { get; set; }

        [Category("Provider specific")]
        public double MarketTakeBound { get; set; }

        [Category("Provider specific")]
        public double MaxMoveOrderLimit { get; set; }


        [Category("Provider specific")]
        public Filter[] Filters { get; set; }






        
        

  




    }
}
