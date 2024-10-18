namespace Twm.Core.DataProviders.Binance.Models.Response.Error
{
    /// <summary>
    /// This timeout exception means the request was valid, the server went to execute but then timed out. This does mean
    /// the request failed. The request should be treated as unknown.
    /// </summary>
    public class BinanceUnauthorizedException : BinanceException
    {
        public BinanceUnauthorizedException(BinanceError errorDetails) : base((string) " Unauthorized.", errorDetails)
        {
        }
    }
}