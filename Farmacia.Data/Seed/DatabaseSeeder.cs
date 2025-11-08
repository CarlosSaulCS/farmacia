using Farmacia.Data.Contexts;
using Farmacia.Domain.Entities;
using Farmacia.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Farmacia.Data.Seed;

public class DatabaseSeeder
{
    private readonly PharmacyDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    private static readonly IReadOnlyList<ServiceSeed> ServiceSeeds = new List<ServiceSeed>
    {
        new("SERV-CONSULTA", "Consulta médica general", 150m),
        new("SERV-CURACION", "Curación y limpieza de heridas", 90m),
        new("SERV-SIGNOS", "Toma de signos vitales", 40m),
        new("SERV-CERTIFICADO", "Certificado médico", 60m)
    };

    public DatabaseSeeder(PharmacyDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.EnsureCreatedAsync(cancellationToken);

        if (!await _context.Users.AnyAsync(cancellationToken))
        {
            await SeedUsersAsync(cancellationToken);
        }

        if (!await _context.Suppliers.AnyAsync(cancellationToken))
        {
            await SeedSuppliersAsync(cancellationToken);
        }

        if (!await _context.Products.AnyAsync(cancellationToken))
        {
            await SeedProductsAsync(cancellationToken);
        }

        if (!await _context.Patients.AnyAsync(cancellationToken))
        {
            await SeedPatientsAsync(cancellationToken);
        }

        if (!await _context.Sequences.AnyAsync(cancellationToken))
        {
            await SeedSequencesAsync(cancellationToken);
        }

        if (!await _context.AppConfigurations.AnyAsync(cancellationToken))
        {
            await SeedConfigurationsAsync(cancellationToken);
        }

        await EnsureServiceCatalogAsync(cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        const string defaultPassword = "Admin123!";

        var users = new[]
        {
            new User
            {
                Username = "admin",
                FullName = "Administrador",
                Role = UserRole.Administrador,
                PasswordHash = string.Empty,
                MustChangePassword = true
            },
            new User
            {
                Username = "cajero",
                FullName = "Cajero Principal",
                Role = UserRole.Cajero,
                PasswordHash = string.Empty
            },
            new User
            {
                Username = "encargado",
                FullName = "Encargado de Inventario",
                Role = UserRole.Encargado,
                PasswordHash = string.Empty
            },
            new User
            {
                Username = "medico",
                FullName = "Doctora Ana Pérez",
                Role = UserRole.Medico,
                PasswordHash = string.Empty
            }
        };

        foreach (var user in users)
        {
            user.PasswordHash = ComputeHash(defaultPassword, user.Username);
        }

        _context.Users.AddRange(users);

        _logger.LogInformation("Usuarios de ejemplo generados (contraseña Admin123!).");
        await Task.CompletedTask;
    }

    private async Task SeedSuppliersAsync(CancellationToken cancellationToken)
    {
        _context.Suppliers.AddRange(
            new Supplier { Name = "Distribuidora Salud Ideal", Phone = "555-0101" },
            new Supplier { Name = "Medicamentos del Centro", Phone = "555-0202" });

        await Task.CompletedTask;
    }

    private async Task SeedProductsAsync(CancellationToken cancellationToken)
    {
        var products = new List<Product>
        {
            new()
            {
                Name = "Paracetamol 500mg 10 tabletas",
                Presentation = "Caja",
                Barcode = "7501001234567",
                Cost = 15m,
                Price = 28m,
                TaxRate = 0.16m,
                StockMinimum = 10,
                UsesBatches = true
            },
            new()
            {
                Name = "Ibuprofeno 400mg 20 tabletas",
                Presentation = "Blister",
                Barcode = "7502009876543",
                Cost = 30m,
                Price = 55m,
                TaxRate = 0.16m,
                StockMinimum = 15,
                UsesBatches = true
            },
            new()
            {
                Name = "Vitamina C 500mg",
                Presentation = "Frasco",
                Barcode = "7503005432198",
                Cost = 25m,
                Price = 45m,
                TaxRate = 0.16m,
                StockMinimum = 8,
                UsesBatches = false
            },
            new()
            {
                Name = "Consulta médica general",
                Presentation = "Servicio",
                InternalCode = "SERV-CONSULTA",
                Cost = 0m,
                Price = 150m,
                TaxRate = 0m,
                StockMinimum = 0,
                UsesBatches = false
            },
            new()
            {
                Name = "Curación y limpieza de heridas",
                Presentation = "Servicio",
                InternalCode = "SERV-CURACION",
                Cost = 0m,
                Price = 90m,
                TaxRate = 0m,
                StockMinimum = 0,
                UsesBatches = false
            },
            new()
            {
                Name = "Toma de signos vitales",
                Presentation = "Servicio",
                InternalCode = "SERV-SIGNOS",
                Cost = 0m,
                Price = 40m,
                TaxRate = 0m,
                StockMinimum = 0,
                UsesBatches = false
            }
        };

        _context.Products.AddRange(products);

        _context.ProductLots.AddRange(
            new ProductLot
            {
                Product = products[0],
                LotCode = "PARA-001",
                ExpirationDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(12)),
                Quantity = 100,
                RemainingQuantity = 100
            },
            new ProductLot
            {
                Product = products[1],
                LotCode = "IBUP-001",
                ExpirationDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(8)),
                Quantity = 80,
                RemainingQuantity = 80
            });

        await Task.CompletedTask;
    }

    private async Task SeedPatientsAsync(CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            Name = "Juan Torres",
            Phone = "555-3030",
            BirthDate = DateOnly.FromDateTime(new DateTime(1990, 5, 12))
        };

        _context.Customers.Add(customer);

        _context.Patients.AddRange(
            new Patient
            {
                Name = "Juan Torres",
                Phone = "555-3030",
                BirthDate = DateOnly.FromDateTime(new DateTime(1990, 5, 12)),
                Notes = "Paciente frecuente",
                Customer = customer
            },
            new Patient
            {
                Name = "María López",
                Phone = "555-5050",
                BirthDate = DateOnly.FromDateTime(new DateTime(1985, 11, 3)),
                Notes = "Hipertensión controlada"
            });

        await Task.CompletedTask;
    }

    private async Task SeedSequencesAsync(CancellationToken cancellationToken)
    {
        _context.Sequences.AddRange(
            new Sequence { Name = "VENTA", Prefix = "V-", CurrentValue = 1000 },
            new Sequence { Name = "COMPRA", Prefix = "C-", CurrentValue = 500 },
            new Sequence { Name = "RECETA", Prefix = "R-", CurrentValue = 200 },
            new Sequence { Name = "DEVOLUCION", Prefix = "D-", CurrentValue = 50 },
            new Sequence { Name = "CORTE", Prefix = "CZ-", CurrentValue = 10 });

        await Task.CompletedTask;
    }

    private async Task SeedConfigurationsAsync(CancellationToken cancellationToken)
    {
        _context.AppConfigurations.AddRange(
            new AppConfiguration { Key = "General.StoreName", Value = "Farmacia Esperanza" },
            new AppConfiguration { Key = "General.StoreAddress", Value = "Av. Salud 123, Col. Centro" },
            new AppConfiguration { Key = "General.StorePhone", Value = "555-9090" },
            new AppConfiguration { Key = "General.TicketFooter", Value = "¡Gracias por elegirnos!" }
        );

        await Task.CompletedTask;
    }

    private async Task EnsureServiceCatalogAsync(CancellationToken cancellationToken)
    {
        foreach (var service in ServiceSeeds)
        {
            var existing = await _context.Products.FirstOrDefaultAsync(p => p.InternalCode == service.InternalCode, cancellationToken);
            if (existing is null)
            {
                _context.Products.Add(new Product
                {
                    Name = service.Name,
                    Presentation = "Servicio",
                    InternalCode = service.InternalCode,
                    Cost = 0m,
                    Price = service.DefaultPrice,
                    TaxRate = 0m,
                    StockMinimum = 0m,
                    UsesBatches = false
                });
                continue;
            }

            var updated = false;

            if (existing.UsesBatches)
            {
                existing.UsesBatches = false;
                updated = true;
            }

            if (existing.TaxRate != 0m)
            {
                existing.TaxRate = 0m;
                updated = true;
            }

            if (existing.StockMinimum != 0m)
            {
                existing.StockMinimum = 0m;
                updated = true;
            }

            if (string.IsNullOrWhiteSpace(existing.Presentation) || !existing.Presentation.Equals("Servicio", StringComparison.OrdinalIgnoreCase))
            {
                existing.Presentation = "Servicio";
                updated = true;
            }

            if (existing.Price <= 0m)
            {
                existing.Price = service.DefaultPrice;
                updated = true;
            }

            if (updated)
            {
                _context.Products.Update(existing);
            }
        }
    }

    private sealed record ServiceSeed(string InternalCode, string Name, decimal DefaultPrice);

    private static string ComputeHash(string value, string salt)
    {
        var bytes = Encoding.UTF8.GetBytes(value + salt);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
