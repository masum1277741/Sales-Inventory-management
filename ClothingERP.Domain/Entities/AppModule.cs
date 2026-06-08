namespace ClothingERP.Domain.Entities;

public class AppModule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = "Index";
    public int? ParentModuleId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual AppModule? ParentModule { get; set; }
    public virtual ICollection<AppModule> ChildModules { get; set; } = new List<AppModule>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}