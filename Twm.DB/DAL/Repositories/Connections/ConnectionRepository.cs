using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twm.DB.DAL.Interfaces.Connections;
using Twm.Model.Model;
using Microsoft.EntityFrameworkCore;

namespace Twm.DB.DAL.Repositories.Connections
{
    public class ConnectionRepository : GenericRepository<Connection,  TwmContext>, IConnectionRepository
    {
        public ConnectionRepository(TwmContext context) : base(context)
        {
        }

        public Task<IEnumerable<Connection>> GetAll()
        {
            var connections = DbSet.Include(x=>x.Options).
                Include(x=>x.DataProvider).
                OrderByDescending(x => x.Name);

            return Task.FromResult(connections.AsEnumerable());
        }

        

        public Task<Connection> GetById(int id)
        {
            var connection = DbSet.Include(x => x.Options).
                Include(x => x.DataProvider).FirstOrDefault(x => x.Id == id);
            return Task.FromResult(connection);
        }




    }
}