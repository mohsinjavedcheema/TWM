using Twm.DB.DAL.Interfaces.Instruments;
using Twm.Model.Model;

namespace Twm.DB.DAL.Repositories.Instruments
{
    public class InstrumentInstrumentListRepository : GenericRepository<InstrumentInstrumentList,  TwmContext>, IInstrumentInstrumentListRepository
    {
        public InstrumentInstrumentListRepository(TwmContext context) : base(context)
        {
        }

      
    }
}