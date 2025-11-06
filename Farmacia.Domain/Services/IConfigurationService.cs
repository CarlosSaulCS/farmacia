namespace Farmacia.Domain.Services;

public interface IConfigurationService
{
    Task<GeneralSettings> GetGeneralSettingsAsync(CancellationToken cancellationToken = default);
    Task SaveGeneralSettingsAsync(GeneralSettings settings, CancellationToken cancellationToken = default);
}

public record GeneralSettings(string StoreName, string StoreAddress, string PhoneNumber, string TicketFooter);
