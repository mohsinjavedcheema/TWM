namespace Twm.Core.DataProviders.Common
{
    /// <summary>
    /// This exception is used when malformed requests are sent to the server. Please review the request object
    /// </summary>
    public class TwmBadRequestException : TwmException
    {
        public TwmBadRequestException(TwmError errorDetails) : base((string)"Malformed requests are sent to the server. Please review the request object/string", errorDetails)
        {
        }
    }
}