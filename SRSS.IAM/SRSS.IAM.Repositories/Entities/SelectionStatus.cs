namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Selection status for papers in the screening phase
    /// This is NOT stored in Paper - it belongs to ScreeningResolution
    /// </summary>
    public enum SelectionStatus
    {
        Pending = 0,
        Included = 1,
        Excluded = 2,
        Duplicate = 3
    }
}
