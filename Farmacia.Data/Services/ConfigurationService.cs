using System.Collections.Generic;
using System.Linq;
using Farmacia.Data.Contexts;
using Farmacia.Domain.Entities;
using Farmacia.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.Data.Services;

public class ConfigurationService : IConfigurationService
{
    private const string StoreNameKey = "General.StoreName";
    private const string StoreAddressKey = "General.StoreAddress";
    private const string StorePhoneKey = "General.StorePhone";
    private const string TicketFooterKey = "General.TicketFooter";

    private readonly PharmacyDbContext _context;

    public ConfigurationService(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task<GeneralSettings> GetGeneralSettingsAsync(CancellationToken cancellationToken = default)
    {
        var configurations = await _context.AppConfigurations
            .AsNoTracking()
            .Where(c => Keys.Contains(c.Key))
            .ToListAsync(cancellationToken);

        string GetValue(string key, string fallback) =>
            configurations.FirstOrDefault(c => c.Key == key)?.Value ?? fallback;

        return new GeneralSettings(
            GetValue(StoreNameKey, "Farmacia Local"),
            GetValue(StoreAddressKey, ""),
            GetValue(StorePhoneKey, ""),
            GetValue(TicketFooterKey, "Gracias por su compra")
        );
    }

    public async Task SaveGeneralSettingsAsync(GeneralSettings settings, CancellationToken cancellationToken = default)
    {
        await UpsertAsync(StoreNameKey, settings.StoreName, cancellationToken);
        await UpsertAsync(StoreAddressKey, settings.StoreAddress, cancellationToken);
        await UpsertAsync(StorePhoneKey, settings.PhoneNumber, cancellationToken);
        await UpsertAsync(TicketFooterKey, settings.TicketFooter, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertAsync(string key, string value, CancellationToken cancellationToken)
    {
        var entry = await _context.AppConfigurations.FirstOrDefaultAsync(c => c.Key == key, cancellationToken);
        if (entry is null)
        {
            _context.AppConfigurations.Add(new AppConfiguration { Key = key, Value = value });
        }
        else
        {
            entry.Value = value;
        }
    }

    private static readonly HashSet<string> Keys = new()
    {
        StoreNameKey,
        StoreAddressKey,
        StorePhoneKey,
        TicketFooterKey
    };
}
