using Farmacia.Domain.Entities;

namespace Farmacia.Domain.Services;

public interface IAuthenticationService
{
    Task<User?> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<User> CreateUserAsync(User user, string plainPassword, CancellationToken cancellationToken = default);
    string ComputeHash(string plainPassword, string salt);
}
