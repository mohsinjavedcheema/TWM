using System.Collections.Generic;
using System.Threading.Tasks;
using AlgoDesk.Core.Interfaces;
using AlgoDesk.Model;

namespace AlgoDesk.Data.Interfaces.Options
{
    public interface ISystemOptionRepository : IGenericRepository<ISystemOption>
    {
        Task<IEnumerable<ISystemOption>> GetAll();

        Task<ISystemOption> GetById(int id);

        Task<ISystemOption> GetByCode(string code);
    }
}