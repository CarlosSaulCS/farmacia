using Farmacia.Domain.Enums;

namespace Farmacia.Domain.Entities;

public class User : EntityBase
{
    public required string Username { get; set; }
    public required string FullName { get; set; }
    public UserRole Role { get; set; }
    public required string PasswordHash { get; set; }
    public bool MustChangePassword { get; set; }
}
