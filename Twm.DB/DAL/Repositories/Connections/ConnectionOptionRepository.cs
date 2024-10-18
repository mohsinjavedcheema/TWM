using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twm.DB.DAL.Interfaces.Connections;
using Twm.Model.Model;

namespace Twm.DB.DAL.Repositories.Connections
{
    public class ConnectionOptionRepository : GenericRepository<ConnectionOption, TwmContext>, IConnectionOptionRepository
    {
        public ConnectionOptionRepository(TwmContext context) : base(context)
        {
        }

        public Task<IEnumerable<ConnectionOption>> GetAll()
        {
            var connectionOptions = DbSet.OrderByDescending(x => x.Name);

            return Task.FromResult(connectionOptions.AsEnumerable());
        }

        

        public Task<ConnectionOption> GetById(int id)
        {
            var connectionOption = DbSet.FirstOrDefault(x => x.Id == id);
            return Task.FromResult(connectionOption);
        }




    }
}