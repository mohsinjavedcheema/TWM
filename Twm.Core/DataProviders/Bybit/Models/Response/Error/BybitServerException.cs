namespace Twm.Core.DataProviders.Bybit.Models.Response.Error
{
    /// <summary>
    /// This exception is used when the request is valid but there was an error on the server side
    /// </summary>
    public class BybitServerException : BybitException
    {
        public BybitServerException(BybitError errorDetails) : base((string) "Request to BybitAPI is valid but there was an error on the server side", errorDetails)
        {
        }
    }
}