using Farmacia.Data.Contexts;
using Farmacia.Domain.Entities;
using Farmacia.Domain.Enums;
using Farmacia.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.Data.Services;

public class InventoryService : IInventoryService
{
    private readonly PharmacyDbContext _context;

    public InventoryService(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task ApplyPurchaseAsync(Purchase purchase, CancellationToken cancellationToken = default)
    {
        foreach (var line in purchase.Lines)
        {
            await ApplyPurchaseLineAsync(purchase, line, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ApplySaleAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        foreach (var line in sale.Lines)
        {
            await ApplySaleLineAsync(sale, line, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AdjustInventoryAsync(InventoryMovement movement, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.Include(p => p.Lots).FirstAsync(p => p.Id == movement.ProductId, cancellationToken);
        ProductLot? lot = null;

        if (movement.ProductLotId.HasValue)
        {
            lot = await _context.ProductLots.FirstOrDefaultAsync(l => l.Id == movement.ProductLotId.Value, cancellationToken);
        }

        if (lot is null && product.UsesBatches)
        {
            lot = await _context.ProductLots
                .Where(l => l.ProductId == product.Id)
                .OrderBy(l => l.ExpirationDate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var signedQuantity = movement.MovementType switch
        {
            InventoryMovementType.Entrada or InventoryMovementType.AjustePositivo => movement.Quantity,
            InventoryMovementType.Salida or InventoryMovementType.AjusteNegativo => -movement.Quantity,
            _ => 0
        };

        if (lot is null)
        {
            if (product.UsesBatches)
            {
                throw new InvalidOperationException("Se requiere lote para productos con control de lotes.");
            }
        }
        else
        {
            lot.RemainingQuantity += signedQuantity;
            lot.Quantity += signedQuantity;
            if (lot.RemainingQuantity < 0)
            {
                throw new InvalidOperationException($"El lote {lot.LotCode} no cuenta con stock suficiente.");
            }
        }

        await _context.InventoryMovements.AddAsync(movement, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductLot>> GetLotsForProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _context.ProductLots
            .Where(l => l.ProductId == productId)
            .OrderBy(l => l.ExpirationDate)
            .ToListAsync(cancellationToken);
    }

    private async Task ApplyPurchaseLineAsync(Purchase purchase, PurchaseLine line, CancellationToken cancellationToken)
    {
        ProductLot? lot = null;
        if (!string.IsNullOrWhiteSpace(line.LotCode))
        {
            lot = await _context.ProductLots
                .FirstOrDefaultAsync(l => l.ProductId == line.ProductId && l.LotCode == line.LotCode, cancellationToken);
        }

        if (lot is null)
        {
            lot = new ProductLot
            {
                ProductId = line.ProductId,
                LotCode = line.LotCode ?? $"{line.ProductId}-{DateTime.Now:yyyyMMddHHmmss}",
                ExpirationDate = line.ExpirationDate,
                Quantity = 0,
                RemainingQuantity = 0
            };
            _context.ProductLots.Add(lot);
        }

        lot.Quantity += line.Quantity;
        lot.RemainingQuantity += line.Quantity;

        await _context.InventoryMovements.AddAsync(new InventoryMovement
        {
            ProductId = line.ProductId,
            ProductLot = lot,
            MovementType = InventoryMovementType.Entrada,
            Quantity = line.Quantity,
            Reason = $"Compra {purchase.Folio}",
            UserId = purchase.UserId
        }, cancellationToken);
    }

    private async Task ApplySaleLineAsync(Sale sale, SaleLine line, CancellationToken cancellationToken)
    {
        var product = await _context.Products.Include(p => p.Lots).FirstAsync(p => p.Id == line.ProductId, cancellationToken);

        var lots = product.UsesBatches
            ? await _context.ProductLots
                .Where(l => l.ProductId == product.Id && l.RemainingQuantity > 0)
                .OrderBy(l => l.ExpirationDate)
                .ToListAsync(cancellationToken)
            : new List<ProductLot>();

        var remaining = line.Quantity;
        foreach (var lot in lots)
        {
            if (remaining <= 0)
            {
                break;
            }

            var consume = Math.Min(lot.RemainingQuantity, remaining);
            lot.RemainingQuantity -= consume;
            remaining -= consume;

            await _context.InventoryMovements.AddAsync(new InventoryMovement
            {
                ProductId = product.Id,
                ProductLotId = lot.Id,
                MovementType = InventoryMovementType.Salida,
                Quantity = consume,
                Reason = $"Venta {sale.Folio}",
                UserId = sale.UserId
            }, cancellationToken);
        }

        if (product.UsesBatches && remaining > 0)
        {
            throw new InvalidOperationException($"Stock insuficiente para el producto {product.Name}.");
        }

        if (!product.UsesBatches)
        {
            await _context.InventoryMovements.AddAsync(new InventoryMovement
            {
                ProductId = product.Id,
                MovementType = InventoryMovementType.Salida,
                Quantity = line.Quantity,
                Reason = $"Venta {sale.Folio}",
                UserId = sale.UserId
            }, cancellationToken);
        }
    }
}
