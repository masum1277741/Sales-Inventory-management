namespace ClothingERP.Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfileImage { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
public class UserListDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? BranchNames { get; set; }  
    public string? PhoneNumber { get; set; }
}
public class CreateUserDto
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    public int RoleId { get; set; }
}

public class UpdateUserDto
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    public int RoleId { get; set; }

    public bool IsActive { get; set; } = true;

    public string? ProfileImagePath { get; set; }
}