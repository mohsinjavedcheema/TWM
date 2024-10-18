using System.Collections.Generic;
using System.Threading.Tasks;
using Twm.Model.Model;

namespace Twm.DB.DAL.Interfaces.Instruments
{
    public interface IInstrumentRepository : IGenericRepository<Instrument>
    {
        Task<IEnumerable<Instrument>> GetAll();

        void RemoveAll();

        void RemoveAll(int connectionId, List<string> excludeList);


    }
}