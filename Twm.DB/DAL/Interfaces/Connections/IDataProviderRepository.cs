using System.Collections.Generic;
using System.Threading.Tasks;
using Twm.Model.Model;

namespace Twm.DB.DAL.Interfaces.Connections
{
    public interface IDataProviderRepository : IGenericRepository<DataProvider>
    {
        Task<IEnumerable<DataProvider>> GetAll();

        Task<DataProvider> GetById(int id);

        
    }
}