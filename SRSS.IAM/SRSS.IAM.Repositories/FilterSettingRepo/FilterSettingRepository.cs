using Shared.Repositories;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Repositories.FilterSettingRepo
{
    public class FilterSettingRepository : GenericRepository<FilterSetting, Guid, AppDbContext>, IFilterSettingRepository
    {
        public FilterSettingRepository(AppDbContext context) : base(context)
        {
        }
    }
}
