using System.Collections.Generic;
using System.Threading.Tasks;
using Twm.Model.Model;

namespace Twm.DB.DAL.Interfaces.Presets
{
    public interface IPresetRepository : IGenericRepository<Preset>
    {
        Task<IEnumerable<Preset>> GetAll();

        Task<Preset> GetById(int id);

        Task<Preset> GetByGuid(string guid);
    }
}