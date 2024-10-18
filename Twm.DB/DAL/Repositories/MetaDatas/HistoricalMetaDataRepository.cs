using Twm.DB.DAL.Interfaces.MetaDatas;
using Twm.Model.Model;

namespace Twm.DB.DAL.Repositories.MetaDatas
{
    public class HistoricalMetaDataRepository : GenericRepository<HistoricalMetaData,  TwmContext>, IHistoricalMetaDataRepository
    {
        public HistoricalMetaDataRepository(TwmContext context) : base(context)
        {
        }


    }
}