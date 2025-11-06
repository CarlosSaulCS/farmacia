using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Farmacia.Data.Contexts;
using Farmacia.Data.Seed;
using Farmacia.Data.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

static string ResolveDatabasePath()
{
	var dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FarmaciaApp");
	Directory.CreateDirectory(dataDir);
	return Path.Combine(dataDir, "farmacia.db");
}

static DbContextOptions<PharmacyDbContext> BuildOptions(string dbPath)
	=> new DbContextOptionsBuilder<PharmacyDbContext>()
		.UseSqlite($"Data Source={dbPath}")
		.Options;

static async Task EnsureDatabaseReadyAsync(string dbPath)
{
	var options = BuildOptions(dbPath);
	await using var context = new PharmacyDbContext(options);
	var seeder = new DatabaseSeeder(context, NullLogger<DatabaseSeeder>.Instance);
	await seeder.SeedAsync();
}

static async Task<int> InspectAsync(string dbPath)
{
	await EnsureDatabaseReadyAsync(dbPath);

	Console.WriteLine($"Database file: {dbPath}");
	Console.WriteLine($"Exists: {File.Exists(dbPath)}");

	if (!File.Exists(dbPath))
	{
		Console.WriteLine("No database found; launch the WPF app once to initialize it.");
		return 1;
	}

	var fileInfo = new FileInfo(dbPath);
	Console.WriteLine($"Size: {fileInfo.Length / 1024.0:F2} KB");
	Console.WriteLine($"Last Write: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");

	using var connection = new SqliteConnection($"Data Source={dbPath}");
	await connection.OpenAsync();

	var tables = new List<string>();
	await using (var command = connection.CreateCommand())
	{
		command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name";
		await using var reader = await command.ExecuteReaderAsync();
		while (await reader.ReadAsync())
		{
			tables.Add(reader.GetString(0));
		}
	}

	if (tables.Count == 0)
	{
		Console.WriteLine("No user tables detected.");
		return 0;
	}

	Console.WriteLine();
	Console.WriteLine("Table overview (row counts):");

	foreach (var table in tables)
	{
		await using var countCommand = connection.CreateCommand();
		countCommand.CommandText = $"SELECT COUNT(*) FROM \"{table}\"";
		var count = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
		Console.WriteLine($" - {table}: {count}");
	}

	await using (var integrityCommand = connection.CreateCommand())
	{
		integrityCommand.CommandText = "PRAGMA integrity_check";
		var result = Convert.ToString(await integrityCommand.ExecuteScalarAsync());
		Console.WriteLine();
		Console.WriteLine($"Integrity check: {result}");
	}

	return 0;
}

static async Task<int> BackupAsync(string dbPath, string? destination)
{
	await EnsureDatabaseReadyAsync(dbPath);

	destination ??= Path.Combine(Environment.CurrentDirectory, "backups");
	Directory.CreateDirectory(destination);

	var options = BuildOptions(dbPath);
	await using var context = new PharmacyDbContext(options);
	var maintenance = new DatabaseMaintenanceService(context, NullLogger<DatabaseMaintenanceService>.Instance);

	var backupPath = await maintenance.CreateBackupAsync(destination);
	Console.WriteLine($"Backup created at: {backupPath}");
	return 0;
}

static async Task<int> RestoreAsync(string dbPath, string source)
{
	if (!File.Exists(source))
	{
		Console.WriteLine($"Backup file not found: {source}");
		return 1;
	}

	var options = BuildOptions(dbPath);
	await using var context = new PharmacyDbContext(options);
	var maintenance = new DatabaseMaintenanceService(context, NullLogger<DatabaseMaintenanceService>.Instance);

	await maintenance.RestoreBackupAsync(source);
	Console.WriteLine("Database restored successfully.");
	return 0;
}

var dbPath = ResolveDatabasePath();
var command = args.Length > 0 ? args[0].ToLowerInvariant() : "inspect";

try
{
	switch (command)
	{
		case "inspect":
			return await InspectAsync(dbPath);
		case "backup":
			return await BackupAsync(dbPath, args.Length > 1 ? args[1] : null);
		case "restore":
			if (args.Length < 2)
			{
				Console.WriteLine("Usage: restore <backupPath>");
				return 1;
			}

			return await RestoreAsync(dbPath, args[1]);
		case "init":
			await EnsureDatabaseReadyAsync(dbPath);
			Console.WriteLine("Database initialized and seeded.");
			return 0;
		default:
			Console.WriteLine("Commands: inspect | backup [destination] | restore <backupPath>");
			return 1;
	}
}
catch (Exception ex)
{
	Console.WriteLine($"Error: {ex.Message}");
	return 1;
}
