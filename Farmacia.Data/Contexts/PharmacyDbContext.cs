using System.Reflection;
using Farmacia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.Data.Contexts;

public class PharmacyDbContext : DbContext
{
    public PharmacyDbContext(DbContextOptions<PharmacyDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductLot> ProductLots => Set<ProductLot>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleLine> SaleLines => Set<SaleLine>();
    public DbSet<ReturnOperation> ReturnOperations => Set<ReturnOperation>();
    public DbSet<ReturnLine> ReturnLines => Set<ReturnLine>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseLine> PurchaseLines => Set<PurchaseLine>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Consultation> Consultations => Set<Consultation>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<AppConfiguration> AppConfigurations => Set<AppConfiguration>();
    public DbSet<CashSession> CashSessions => Set<CashSession>();
    public DbSet<CashMovement> CashMovements => Set<CashMovement>();
    public DbSet<Sequence> Sequences => Set<Sequence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties().Where(p => p.ClrType == typeof(DateOnly)))
            {
                property.SetValueConverter(new DateOnlyConverters.DateOnlyConverter());
                property.SetValueComparer(DateOnlyConverters.DateOnlyComparer.Instance);
            }

            foreach (var property in entityType.GetProperties().Where(p => p.ClrType == typeof(TimeOnly)))
            {
                property.SetValueConverter(new DateOnlyConverters.TimeOnlyConverter());
                property.SetValueComparer(DateOnlyConverters.TimeOnlyComparer.Instance);
            }
        }
    }
}
