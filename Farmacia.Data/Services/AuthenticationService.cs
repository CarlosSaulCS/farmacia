using System.Security.Cryptography;
using System.Text;
using Farmacia.Data.Contexts;
using Farmacia.Domain.Entities;
using Farmacia.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.Data.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly PharmacyDbContext _context;

    public AuthenticationService(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task<User?> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var normalized = username.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == normalized && u.IsActive, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var hash = ComputeHash(password, user.Username);
        return hash == user.PasswordHash ? user : null;
    }

    public async Task<User> CreateUserAsync(User user, string plainPassword, CancellationToken cancellationToken = default)
    {
        user.PasswordHash = ComputeHash(plainPassword, user.Username);
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public string ComputeHash(string plainPassword, string salt)
    {
        var data = Encoding.UTF8.GetBytes(plainPassword + salt);
        var hash = SHA256.HashData(data);
        return Convert.ToHexString(hash);
    }
}
