namespace Shared.Entities.BaseEntity
{
    public abstract class BaseEntity<T> : IBaseEntity<T>
    {
        public T Id { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
    }
}
