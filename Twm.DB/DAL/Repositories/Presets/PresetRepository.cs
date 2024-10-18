using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twm.DB.DAL.Interfaces.Presets;
using Twm.Model.Model;

namespace Twm.DB.DAL.Repositories.Presets
{
    public class PresetRepository : GenericRepository<Preset,  TwmContext>, IPresetRepository
    {
        public PresetRepository(TwmContext context) : base(context)
        {
        }

        public Task<IEnumerable<Preset>> GetAll()
        {
            var presets = DbSet.OrderByDescending(x => x.Name).ToList();

            return Task.FromResult(presets.Cast<Preset>().AsEnumerable());
        }

        public Task<Preset> GetById(int id)
        {
            var preset = DbSet.FirstOrDefault(x => x.Id == id);
            return Task.FromResult(preset);
        }

        public Task<Preset> GetByGuid(string guid)
        {
            var preset = DbSet.FirstOrDefault(x => x.Guid == guid);
            return Task.FromResult(preset);
        }
    }
}