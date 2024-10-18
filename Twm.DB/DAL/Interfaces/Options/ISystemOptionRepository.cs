using System.Collections.Generic;
using System.Threading.Tasks;
using Twm.Model.Model;

namespace Twm.DB.DAL.Interfaces.Options
{
    public interface ISystemOptionRepository : IGenericRepository<SystemOption>
    {
        Task<IEnumerable<SystemOption>> GetAll();

        Task<SystemOption> GetById(int id);

        Task<SystemOption> GetByCode(string code);
    }
}