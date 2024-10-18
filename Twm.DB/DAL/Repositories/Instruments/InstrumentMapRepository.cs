using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twm.DB.DAL.Interfaces.Instruments;
using Twm.Model.Model;
using Microsoft.EntityFrameworkCore;

namespace Twm.DB.DAL.Repositories.Instruments
{
    public class InstrumentMapRepository : GenericRepository<InstrumentMap,  TwmContext>, IInstrumentMapRepository
    {
        public InstrumentMapRepository(TwmContext context) : base(context)
        {
        }

        public Task<IEnumerable<InstrumentMap>> GetAll()
        {
            var instrumentMaps = DbSet.OrderBy(x=>x.Id);

            return Task.FromResult(instrumentMaps.AsEnumerable());
        }


        public Task<IEnumerable<InstrumentMap>> GetPage(int page, int pageItems)
        {
            var instrumentMaps = DbSet.Include(x=>x.FirstInstrument).Include(x=>x.SecondInstrument).Skip(page * pageItems).Take(pageItems).OrderBy(x => x.Id);

            return Task.FromResult(instrumentMaps.AsEnumerable());
        }

        public Task<IEnumerable<InstrumentMap>> GetInstrumentMapByInstrument(int id)
        {
            var instrumentMaps = DbSet.Include(x => x.FirstInstrument).Include(x => x.SecondInstrument).
                Where(x=>x.FirstInstrumentId == id || x.SecondInstrumentId == id);
            return Task.FromResult(instrumentMaps.AsEnumerable());
        }

    }
}