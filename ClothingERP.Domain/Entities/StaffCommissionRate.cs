namespace ClothingERP.Domain.Entities;

public class StaffCommissionRate : BaseEntity
{
    public int UserId { get; set; }
    public decimal CommissionPercent { get; set; }
    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;
}