using System.Collections.Generic;
using System.Threading.Tasks;
using Twm.Model.Model;

namespace Twm.DB.DAL.Interfaces.Instruments
{
    public interface IInstrumentMapRepository : IGenericRepository<InstrumentMap>
    {
        Task<IEnumerable<InstrumentMap>> GetAll();

        Task<IEnumerable<InstrumentMap>> GetPage(int page, int pageItems);

        Task<IEnumerable<InstrumentMap>> GetInstrumentMapByInstrument(int id);
    }
}