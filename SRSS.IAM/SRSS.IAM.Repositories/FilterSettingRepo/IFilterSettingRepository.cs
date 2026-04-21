using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.FilterSettingRepo
{
    public interface IFilterSettingRepository : IGenericRepository<FilterSetting, Guid, AppDbContext>
    {
    }
}
