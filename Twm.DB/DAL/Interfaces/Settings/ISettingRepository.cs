using System.Collections.Generic;
using System.Threading.Tasks;
using Twm.Model.Model;

namespace Twm.DB.DAL.Interfaces.Settings
{
    public interface ISettingRepository : IGenericRepository<Setting>
    {
        Task<IEnumerable<Setting>> GetAll();

        Task<Setting> GetById(int id);

        Task<Setting> GetByCode(string code);
    }
}