namespace ClothingERP.Domain.Entities;

public class DashboardLayout : BaseEntity
{
    public int UserId { get; set; }
    public string LayoutJson { get; set; } = string.Empty; 

    public User User { get; set; } = null!;
}