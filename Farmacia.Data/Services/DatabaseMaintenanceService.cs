using Farmacia.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Farmacia.Data.Services;

public class DatabaseMaintenanceService
{
    private readonly PharmacyDbContext _context;
    private readonly ILogger<DatabaseMaintenanceService> _logger;

    public DatabaseMaintenanceService(PharmacyDbContext context, ILogger<DatabaseMaintenanceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> CreateBackupAsync(string destinationFolder, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        var sqlitePath = connection.DataSource;
        if (string.IsNullOrWhiteSpace(sqlitePath) || !File.Exists(sqlitePath))
        {
            throw new InvalidOperationException("No se encontró la base de datos.");
        }

        var fileName = $"farmacia-backup-{DateTime.Now:yyyyMMddHHmmss}.db";
        var destinationPath = Path.Combine(destinationFolder, fileName);
        await using var sourceStream = File.OpenRead(sqlitePath);
        await using var destinationStream = File.Create(destinationPath);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken);

        _logger.LogInformation("Respaldo creado en {Destination}", destinationPath);
        return destinationPath;
    }

    public async Task RestoreBackupAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.CloseAsync();

        var sqlitePath = connection.DataSource;
        if (string.IsNullOrWhiteSpace(sqlitePath))
        {
            throw new InvalidOperationException("No se encontró la base de datos.");
        }

        await using var sourceStream = File.OpenRead(backupPath);
        await using var destinationStream = File.Create(sqlitePath);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken);

        _logger.LogInformation("Base de datos restaurada desde {Source}", backupPath);
    }
}
