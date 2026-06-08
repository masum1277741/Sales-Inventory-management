namespace ClothingERP.Domain.Entities;

public class RolePermission : BaseEntity
{
    public int RoleId { get; set; }
    public int ModuleId { get; set; }
    public bool CanView { get; set; }
    public bool CanInsert { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public bool CanPrint { get; set; }
    public bool CanExport { get; set; }

    public virtual Role Role { get; set; } = null!;
    public virtual AppModule Module { get; set; } = null!;
}