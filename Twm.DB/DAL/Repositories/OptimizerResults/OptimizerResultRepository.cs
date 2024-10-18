using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twm.DB.DAL.Interfaces.OptimizerResults;
using Twm.Model.Model;

namespace Twm.DB.DAL.Repositories.OptimizerResults
{
    public class OptimizerResultRepository : GenericRepository<OptimizerResult,  TwmContext>, IOptimizerResultRepository
    {
        public OptimizerResultRepository(TwmContext context) : base(context)
        {
        }

        public Task<IEnumerable<OptimizerResult>> GetAll()
        {
            var optimizerResult = DbSet.OrderByDescending(x => x.Name).ToList();

            return Task.FromResult(optimizerResult.Cast<OptimizerResult>().AsEnumerable());
        }

        public Task<OptimizerResult> GetById(int id)
        {
            var optimizerResult = DbSet.FirstOrDefault(x => x.Id == id);
            return Task.FromResult(optimizerResult);
        }

        public Task<OptimizerResult> GetByGuid(string guid)
        {
            var optimizerResult = DbSet.FirstOrDefault(x => x.Guid == guid);
            return Task.FromResult(optimizerResult);
        }
    }
}