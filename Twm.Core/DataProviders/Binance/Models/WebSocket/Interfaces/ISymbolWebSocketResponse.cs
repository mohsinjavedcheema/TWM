namespace Twm.Core.DataProviders.Binance.Models.WebSocket.Interfaces
{
    public interface ISymbolWebSocketResponse: IWebSocketResponse
    {
        string Symbol { get; set; }
    }
}