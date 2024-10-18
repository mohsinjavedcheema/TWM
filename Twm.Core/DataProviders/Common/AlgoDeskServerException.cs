namespace Twm.Core.DataProviders.Common
{
    /// <summary>
    /// This exception is used when the request is valid but there was an error on the server side
    /// </summary>
    public class TwmServerException : TwmException
    {
        public TwmServerException(TwmError errorDetails) : base((string)"Request to server is valid but there was an error on the server side", errorDetails)
        {
        }
    }
}