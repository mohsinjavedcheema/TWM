using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twm.DB.DAL.Interfaces.Options;
using Twm.Model.Model;

namespace Twm.DB.DAL.Repositories.Options
{
    public class ViewOptionRepository : GenericRepository<ViewOption,  TwmContext>, IViewOptionRepository
    {
        public ViewOptionRepository(TwmContext context) : base(context)
        {
        }

        public Task<IEnumerable<ViewOption>> GetAll()
        {
            var viewOptions = DbSet.OrderByDescending(x => x.Code).ToList();

            return Task.FromResult(viewOptions.Cast<ViewOption>().AsEnumerable());
        }

        

        public Task<ViewOption> GetById(int optionId)
        {
            var viewOption = DbSet.FirstOrDefault(x => x.Id == optionId);
            return Task.FromResult(viewOption);
        }


        public Task<ViewOption> GetByCode(string code)
        {
            var viewOption = DbSet.FirstOrDefault(x => x.Code == code);
            return Task.FromResult(viewOption);
        }


    }
}