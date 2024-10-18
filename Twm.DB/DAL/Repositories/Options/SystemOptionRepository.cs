using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twm.DB.DAL.Interfaces.Options;
using Twm.Model.Model;

namespace Twm.DB.DAL.Repositories.Options
{
    public class SystemOptionRepository : GenericRepository<SystemOption,  TwmContext>, ISystemOptionRepository
    {
        public SystemOptionRepository(TwmContext context) : base(context)
        {
        }

        public Task<IEnumerable<SystemOption>> GetAll()
        {
            var systemOptions = DbSet.OrderByDescending(x => x.Code).ToList();

            return Task.FromResult(systemOptions.Cast<SystemOption>().AsEnumerable());
        }

        

        public Task<SystemOption> GetById(int optionId)
        {
            var systemOption = DbSet.FirstOrDefault(x => x.Id == optionId);
            return Task.FromResult(systemOption);
        }


        public Task<SystemOption> GetByCode(string code)
        {
            var systemOption = DbSet.FirstOrDefault(x => x.Code == code);
            return Task.FromResult(systemOption);
        }


    }
}