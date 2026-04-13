using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Services.DTOs.StuSeExclusionCode
{
    public class StuSeExclusionCodeResponse
    {
        public Guid Id { get; set; }
        public Guid StudySelectionProcessId { get; set; }
        public Guid? LibraryReasonId { get; set; }
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public ExclusionReasonSource Source { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateExclusionReasonRequest
    {
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class AddExclusionReasonsRequest
    {
        public List<Guid>? LibraryReasonIds { get; set; }
        public List<CustomExclusionReasonRequest>? CustomReasons { get; set; }
    }

    public class CustomExclusionReasonRequest
    {
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
