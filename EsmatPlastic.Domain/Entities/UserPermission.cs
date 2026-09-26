namespace EsmatPlastic.Domain.Entities;

public class UserPermission : ISyncTimestamped
{
    public int UserId { get; set; }

    public int PermissionId { get; set; }

    public Guid SyncId { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;

    public Permission Permission { get; set; } = null!;
}
