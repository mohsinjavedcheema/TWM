namespace Twm.Core.DataProviders.Bybit.Models.Response.Interfaces
{
    public interface IBalanceResponse
    {
        string Asset { get; set; }
        decimal Free { get; set; }
        decimal Locked { get; set; }
    }
}