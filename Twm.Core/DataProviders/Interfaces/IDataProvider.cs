using System.Collections.Generic;
using System.Threading.Tasks;
using Twm.Core.Interfaces;


namespace Twm.Core.DataProviders.Interfaces
{
    public interface IDataProvider
    {
        Task<IEnumerable<object>> GetInstruments();

        Task<IEnumerable<object>> GetTickers();

        Task<IEnumerable<object>> FindInstruments(IRequest request);

        Task<IEnumerable<IHistoricalCandle>> GetHistoricalData(IRequest request);

    }
}