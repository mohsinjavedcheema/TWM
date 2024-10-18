using System;

namespace Twm.Core.DataProviders.Common
{
   
    public class TwmException : Exception
    {
        public TwmError ErrorDetails { get; set; }

        public TwmException(string message, TwmError errorDetails) : base(message)
        {
            ErrorDetails = errorDetails;
        }
    }
}