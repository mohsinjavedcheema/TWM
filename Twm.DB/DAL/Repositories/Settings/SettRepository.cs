using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twm.DB.DAL.Interfaces.Options;
using Twm.DB.DAL.Interfaces.Settings;
using Twm.Model.Model;

namespace Twm.DB.DAL.Repositories.Settings
{
    public class SettingRepository : GenericRepository<Setting,  TwmContext>, ISettingRepository
    {
        public SettingRepository(TwmContext context) : base(context)
        {
        }

        public Task<IEnumerable<Setting>> GetAll()
        {
            var viewOptions = DbSet.OrderByDescending(x => x.Code).ToList();

            return Task.FromResult(viewOptions.Cast<Setting>().AsEnumerable());
        }

        

        public Task<Setting> GetById(int optionId)
        {
            var viewOption = DbSet.FirstOrDefault(x => x.Id == optionId);
            return Task.FromResult(viewOption);
        }


        public Task<Setting> GetByCode(string code)
        {
            var viewOption = DbSet.FirstOrDefault(x => x.Code == code);
            return Task.FromResult(viewOption);
        }


    }
}