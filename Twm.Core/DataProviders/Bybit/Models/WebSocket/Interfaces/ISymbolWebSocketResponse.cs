namespace Twm.Core.DataProviders.Bybit.Models.WebSocket.Interfaces
{
    public interface ISymbolWebSocketResponse: IWebSocketResponse
    {
        string Symbol { get; set; }
    }
}