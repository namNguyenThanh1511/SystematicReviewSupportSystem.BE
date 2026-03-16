using System.Threading.Tasks;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.MetadataMergeService
{
    public interface IMetadataMergeService
    {
        Task MergeAsync(Paper paper, PaperSourceMetadata sourceMetadata);
        Task MergeSelectedFieldsAsync(Paper paper, PaperSourceMetadata sourceMetadata, List<string> fields);
    }
}
