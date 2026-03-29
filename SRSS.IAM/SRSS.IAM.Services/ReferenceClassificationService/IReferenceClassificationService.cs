using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Services.ReferenceMatchingService.DTOs;

namespace SRSS.IAM.Services.ReferenceClassificationService
{
    public interface IReferenceClassificationService
    {
        ReferenceType Classify(ExtractedReference reference);
    }
}
