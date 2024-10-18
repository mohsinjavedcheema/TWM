using System;

namespace Twm.Core.DataProviders.Bybit.Models.Response.Error
{
    public class BybitException: Exception
    {
        public BybitError ErrorDetails { get; set; }

        public BybitException(string message, BybitError errorDetails):base(message)
        {
            ErrorDetails = errorDetails;
        }
    }
}