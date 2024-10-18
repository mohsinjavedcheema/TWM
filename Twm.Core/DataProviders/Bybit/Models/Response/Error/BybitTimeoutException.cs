namespace Twm.Core.DataProviders.Bybit.Models.Response.Error
{
    /// <summary>
    /// This timeout exception means the request was valid, the server went to execute but then timed out. This does mean
    /// the request failed. The request should be treated as unknown.
    /// </summary>
    public class BybitTimeoutException : BybitException
    {
        public BybitTimeoutException(BybitError errorDetails) : base((string) " request was valid, the server went to execute but then timed out. This doesn't mean it failed, and should be treated as UNKNOWN.", errorDetails)
        {
        }
    }
}