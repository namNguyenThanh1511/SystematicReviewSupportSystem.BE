namespace Shared.Entities.BaseEntity
{
    public interface IBaseEntity
    {
        DateTimeOffset CreatedAt { get; set; }
        DateTimeOffset ModifiedAt { get; set; }
    }

    public interface IBaseEntity<T> : IBaseEntity
    {
        T Id { get; set; }
    }
}
