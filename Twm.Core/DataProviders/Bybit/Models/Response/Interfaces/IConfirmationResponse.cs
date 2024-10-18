
using Twm.Core.DataProviders.Interfaces;

namespace Twm.Core.DataProviders.Bybit.Models.Response.Interfaces
{
    public interface IConfirmationResponse : IResponse
    {
        bool Success { get; set; }
    }
}