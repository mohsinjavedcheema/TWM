using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlgoDesk.Core.Interfaces;
using AlgoDesk.Data.Interfaces.Options;
using AlgoDesk.Model;
using AlgoDesk.Model.DAL;
using Microsoft.EntityFrameworkCore;

namespace AlgoDesk.Data.Repositories.Options
{
    public class SystemOptionRepository : GenericRepository<ISystemOption, AlgoDeskContext>, ISystemOptionRepository
    {
        public SystemOptionRepository(AlgoDeskContext context) : base(context)
        {
        }

        public Task<IEnumerable<ISystemOption>> GetAll()
        {
            var posts = DbSet.OrderByDescending(x => x.Code).ToList();

            return System.Threading.Tasks.Task.FromResult(posts.AsEnumerable());
        }

        

        public Task<ISystemOption> GetById(int optionId)
        {
            var post = DbSet.FirstOrDefault(x => x.Id == optionId);
            return System.Threading.Tasks.Task.FromResult(post);
        }


        public Task<ISystemOption> GetByCode(string code)
        {
            var option = DbSet.FirstOrDefault(x => x.Code == code);
            return System.Threading.Tasks.Task.FromResult(option); 
        }


    }
}