namespace Twm.Core.DataProviders.Binance.Models.WebSocket
{
    public class KlineCandleStick
    {
        public decimal Open { get; set; }

        public decimal High { get; set; }

        public decimal Low { get; set; }

        public decimal Close { get; set; }

        public decimal Volume { get; set; }
    }
}