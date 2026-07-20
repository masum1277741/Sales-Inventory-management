namespace ClothingERP.Domain.Entities;

public class UserBranch : BaseEntity
{
    public int UserId { get; set; }
    public int BranchId { get; set; }
    public bool IsDefault { get; set; } = false;  

    public User User { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}