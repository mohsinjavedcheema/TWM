using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using Twm.DB.DAL.Interfaces.Instruments;
using Twm.Model.Model;

namespace Twm.DB.DAL.Repositories.Instruments
{
    public class InstrumentRepository : GenericRepository<Instrument,  TwmContext>, IInstrumentRepository
    {
        public InstrumentRepository(TwmContext context) : base(context)
        {
        }

        public Task<IEnumerable<Instrument>> GetAll()
        {
            var connections = DbSet.OrderByDescending(x => x.Symbol);

            return Task.FromResult(connections.AsEnumerable());
        }

        public void RemoveAll()
        {
            var records = DbSet.ToList();
            DbSet.RemoveRange(records);
        }

        public void RemoveAll(int connectionId, List<string> excludeList)
        {
            var records = DbSet.Where(x=>x.ConnectionId == connectionId && !excludeList.Contains(x.Symbol)).ToList();
            DbSet.RemoveRange(records);
        }
    }
}