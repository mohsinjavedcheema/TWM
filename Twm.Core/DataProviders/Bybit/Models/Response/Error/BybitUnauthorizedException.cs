namespace Twm.Core.DataProviders.Bybit.Models.Response.Error
{
    /// <summary>
    /// This timeout exception means the request was valid, the server went to execute but then timed out. This does mean
    /// the request failed. The request should be treated as unknown.
    /// </summary>
    public class BybitUnauthorizedException : BybitException
    {
        public BybitUnauthorizedException(BybitError errorDetails) : base((string) " Unauthorized.", errorDetails)
        {
        }
    }
}