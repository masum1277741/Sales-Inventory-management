namespace ClothingERP.Domain.Entities;

public class Branch : BaseEntity
{
    public string Code { get; set; } = string.Empty;   // "MAIN", "GULSHAN", "MALE-01"
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Country { get; set; } = "Bangladesh";    // Multi-currency context (BDT branch vs MVR branch)
    public bool IsMainBranch { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
}