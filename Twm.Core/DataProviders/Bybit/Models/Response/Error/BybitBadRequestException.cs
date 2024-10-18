namespace Twm.Core.DataProviders.Bybit.Models.Response.Error
{
    /// <summary>
    /// This exception is used when malformed requests are sent to the server. Please review the request object
    /// </summary>
    public class BybitBadRequestException : BybitException {
        public BybitBadRequestException(BybitError errorDetails) : base((string) "Malformed requests are sent to the server. Please review the request object/string", errorDetails)
        {
        }
    }
}