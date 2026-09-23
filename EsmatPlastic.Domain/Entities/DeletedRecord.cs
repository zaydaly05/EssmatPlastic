namespace EsmatPlastic.Domain.Entities;

public class DeletedRecord
{
    public string EntityType { get; set; } = string.Empty;

    public string RecordKey { get; set; } = string.Empty;

    public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
}
