using System.Collections.Generic;
using System.Threading.Tasks;
using Twm.Model.Model;

namespace Twm.DB.DAL.Interfaces.Connections
{
    public interface IConnectionRepository : IGenericRepository<Connection>
    {
        Task<IEnumerable<Connection>> GetAll();

        Task<Connection> GetById(int id);

        
    }
}