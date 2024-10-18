using System.Collections.Generic;
using System.Threading.Tasks;
using Twm.Model.Model;

namespace Twm.DB.DAL.Interfaces.OptimizerResults
{
    public interface IOptimizerResultRepository : IGenericRepository<OptimizerResult>
    {
        Task<IEnumerable<OptimizerResult>> GetAll();

        Task<OptimizerResult> GetById(int id);

        Task<OptimizerResult> GetByGuid(string guid);
    }
}