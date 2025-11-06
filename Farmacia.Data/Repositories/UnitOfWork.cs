using Farmacia.Data.Contexts;
using Microsoft.EntityFrameworkCore.Storage;

namespace Farmacia.Data.Repositories;

public class UnitOfWork : IAsyncDisposable
{
    private readonly PharmacyDbContext _context;

    public UnitOfWork(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _context.DisposeAsync();
    }
}
