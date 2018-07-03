using System.Collections.Generic;
using System.Threading.Tasks;

namespace HangfireService.Plugins.Repository
{
    interface IRepository<TRecord, TId>
    {
        Task<TRecord> FindById(TId id);
        Task Add(TRecord record);
        Task Delete(TRecord record);
        Task Update(TRecord record);
        Task<IEnumerable<TRecord>> FindAll();

    }
}
