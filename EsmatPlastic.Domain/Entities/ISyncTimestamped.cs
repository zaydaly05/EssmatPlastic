namespace EsmatPlastic.Domain.Entities;

public interface ISyncTimestamped
{
    Guid SyncId { get; set; }
    DateTime UpdatedAt { get; set; }
}