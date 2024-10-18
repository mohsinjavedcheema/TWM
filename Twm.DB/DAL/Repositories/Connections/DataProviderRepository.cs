using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twm.DB.DAL.Interfaces.Connections;
using Twm.Model.Model;


namespace Twm.DB.DAL.Repositories.Connections
{
    public class DataProviderRepository : GenericRepository<DataProvider,  TwmContext>, IDataProviderRepository
    {
        public DataProviderRepository(TwmContext context) : base(context)
        {
        }

        public Task<IEnumerable<DataProvider>> GetAll()
        {
            var dataProviders = DbSet.Include(x=>x.Connections).ThenInclude(x=>x.Options).OrderByDescending(x => x.Name);

            return Task.FromResult(dataProviders.AsEnumerable());
        }

        

        public Task<DataProvider> GetById(int id)
        {
            var dataProvider = DbSet.FirstOrDefault(x => x.Id == id);
            return Task.FromResult(dataProvider);
        }




    }
}