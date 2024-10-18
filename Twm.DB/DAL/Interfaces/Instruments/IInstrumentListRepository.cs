using System.Collections.Generic;
using System.Threading.Tasks;
using Twm.Model.Model;

namespace Twm.DB.DAL.Interfaces.Instruments
{
    public interface IInstrumentListRepository : IGenericRepository<InstrumentList>
    {
        Task<IEnumerable<InstrumentList>> GetAll();

    }
}