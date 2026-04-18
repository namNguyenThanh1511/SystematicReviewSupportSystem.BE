using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class MasterSearchSources : BaseEntity<Guid>
    {
        public string SourceName {get; set;}
        public string? BaseUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
        
        