using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twm.DB.DAL.Interfaces.Instruments;
using Twm.Model.Model;
using Microsoft.EntityFrameworkCore;

namespace Twm.DB.DAL.Repositories.Instruments
{
    public class InstrumentListRepository : GenericRepository<InstrumentList,  TwmContext>, IInstrumentListRepository
    {
        public InstrumentListRepository(TwmContext context) : base(context)
        {
        }

        public Task<IEnumerable<InstrumentList>> GetAll()
        {
            var connections = DbSet.Include(x=>x.InstrumentInstrumentLists).ThenInclude(x=>x.Instrument).Include(x=>x.Connection).OrderByDescending(x => x.Name);

            return Task.FromResult(connections.AsEnumerable());
        }

    }
}