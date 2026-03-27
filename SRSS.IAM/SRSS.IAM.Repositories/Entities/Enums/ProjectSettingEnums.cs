namespace SRSS.IAM.Repositories.Entities
{
    public enum DeduplicationStrictness
    {
        Strict,
        Moderate,
        Lenient,
        ExactMatch // You can keep exact match if you want
    }
}
