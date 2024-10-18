
using Twm.Core.DataProviders.Interfaces;

namespace Twm.Core.DataProviders.Binance.Models.Response.Interfaces
{
    public interface IConfirmationResponse : IResponse
    {
        bool Success { get; set; }
    }
}