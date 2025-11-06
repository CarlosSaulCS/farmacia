namespace Farmacia.Domain.Services;

public interface ISequenceService
{
    Task<string> GetNextFolioAsync(string sequenceName, CancellationToken cancellationToken = default);
}
