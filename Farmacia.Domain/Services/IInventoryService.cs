using Farmacia.Domain.Entities;

namespace Farmacia.Domain.Services;

public interface IInventoryService
{
    Task ApplyPurchaseAsync(Purchase purchase, CancellationToken cancellationToken = default);
    Task ApplySaleAsync(Sale sale, CancellationToken cancellationToken = default);
    Task AdjustInventoryAsync(InventoryMovement movement, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductLot>> GetLotsForProductAsync(int productId, CancellationToken cancellationToken = default);
}
