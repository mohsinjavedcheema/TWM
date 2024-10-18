using System.Collections.Generic;
using System.Threading.Tasks;
using Twm.Model.Model;

namespace Twm.DB.DAL.Interfaces.Options
{
    public interface IViewOptionRepository : IGenericRepository<ViewOption>
    {
        Task<IEnumerable<ViewOption>> GetAll();

        Task<ViewOption> GetById(int id);

        Task<ViewOption> GetByCode(string code);
    }
}