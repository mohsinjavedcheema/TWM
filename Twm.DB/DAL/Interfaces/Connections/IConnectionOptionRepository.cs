using System.Collections.Generic;
using System.Threading.Tasks;
using Twm.Model.Model;

namespace Twm.DB.DAL.Interfaces.Connections
{
    public interface IConnectionOptionRepository : IGenericRepository<ConnectionOption>
    {
        Task<IEnumerable<ConnectionOption>> GetAll();

        Task<ConnectionOption> GetById(int id);

        
    }
}