# Architecture Plan

## Solution Structure
- `Farmacia.Domain`: Entities, value objects, enums, service abstractions, and core logic rules.
- `Farmacia.Data`: Entity Framework Core SQLite DbContext, configurations, repositories, seeders, migrations support, data access utilities, and backup/restore helpers.
- `Farmacia.UI.Wpf`: MVVM-based WPF client containing views, view models, services, commands, value converters, and presentation logic, plus printing templates and configuration handling.
- `docs`: Documentation, installation guide, and database scripts.
- `install`: Inno Setup scripts, packaging assets, and runtime bootstrapper.

## Key Packages (planned)
- `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`
- `CommunityToolkit.Mvvm` for MVVM tooling (observable objects, relays)
- `Microsoft.Extensions.DependencyInjection` for DI in WPF app
- `Dapper` (optional) for reporting queries requiring performance
- `OxyPlot` (optional) for basic charts in reports (out of scope for initial pass)

## Core Modules
1. **Authentication & Security**: login window, role-based navigation, permissions matrix.
2. **POS**: product lookup, cart management, payment breakdown, ticket printing, cash drawer pulse (placeholder), cuts (X/Z) generation.
3. **Inventory**: product catalog CRUD, lot tracking, stock alerts, purchases workflow, inventory movements/kardex.
4. **Consultorio**: agenda, clinical records, prescription generation with PDF export, integration with POS cart.
5. **Patients & Clients**: quick registration, history retrieval.
6. **Reports**: sales, product performance, expirations, supplier purchases, cash audits.
7. **Configuration**: business data, tax rates, printing settings, backup/restore.

## Cross-Cutting Concerns
- Logging (Serilog local file sink)
- Settings stored in JSON alongside database path
- Localization: default es-MX, resource-ready for future internationalization
- Validation via FluentValidation
- Abstractions for future cloud sync/facturación connectors

## Data Model Highlights
- Enforce FIFO consumption per product/lote via domain service
- Soft delete flags for products/users with auditing
- Use `DateOnly` and `TimeOnly` where applicable (converted for EF Core)
- Database script in `docs/sql/schema.sql` generated alongside EF migrations

## Installer Strategy
- Inno Setup script referencing published self-contained build (`win-x64`)
- Prerequisite check for .NET 8 Desktop runtime, fallback to bundled installer
- Post-install custom step to initialize database and admin user

## Immediate Next Steps
1. Add required NuGet packages.
2. Model domain entities with validations.
3. Configure EF Core DbContext and migrations.
4. Create seed data service and SQL scripts.
5. Implement foundational MVVM infrastructure and navigation shell.
6. Build initial screens (Login, Dashboard, POS, Inventory, Agenda, Patients, Reports, Settings).
7. Implement printing templates and placeholders for hardware integrations.
8. Provide documentation (README, quick start guide) and licensing.
