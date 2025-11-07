using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacia.Data.Contexts;
using Farmacia.Domain.Entities;
using Farmacia.UI.Wpf.Models;
using Farmacia.UI.Wpf.Services;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.UI.Wpf.ViewModels;

public partial class InventoryViewModel : ViewModelBase
{
    private readonly PharmacyDbContext _context;
    private readonly UserSessionService _sessionService;

    public ObservableCollection<InventoryItemModel> Items { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private int _totalSkus;

    [ObservableProperty]
    private int _lowStockSkus;

    [ObservableProperty]
    private int _expiringSoonSkus;

    [ObservableProperty]
    private decimal _totalUnits;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDetailVisible))]
    private InventoryItemModel? _selectedItem;

    [ObservableProperty]
    private decimal _selectedProductCost;

    [ObservableProperty]
    private decimal _selectedProductPrice;

    [ObservableProperty]
    private decimal _selectedProductMinimum;

    [ObservableProperty]
    private bool _selectedProductUsesBatches;

    [ObservableProperty]
    private string _newProductName = string.Empty;

    [ObservableProperty]
    private string? _newProductBarcode;

    [ObservableProperty]
    private string? _newProductDescription;

    [ObservableProperty]
    private decimal _newProductCost;

    [ObservableProperty]
    private decimal _newProductPrice;

    [ObservableProperty]
    private decimal _newProductMinimum;

    [ObservableProperty]
    private bool _newProductUsesBatches;

    [ObservableProperty]
    private DateTime? _newProductExpirationDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDetailVisible))]
    private bool _detailActivated;

    public InventoryViewModel(PharmacyDbContext context, UserSessionService sessionService)
    {
        _context = context;
        _sessionService = sessionService;
    }

    public bool IsDetailVisible => DetailActivated && SelectedItem is not null;

    partial void OnIsBusyChanged(bool value)
    {
        RegisterProductCommand?.NotifyCanExecuteChanged();
        UpdateSelectedProductCommand?.NotifyCanExecuteChanged();
        DeleteProductCommand?.NotifyCanExecuteChanged();
    }

    partial void OnSelectedItemChanged(InventoryItemModel? value)
    {
        if (value is null)
        {
            SelectedProductCost = 0m;
            SelectedProductPrice = 0m;
            SelectedProductMinimum = 0m;
            SelectedProductUsesBatches = false;
        }
        else
        {
            SelectedProductCost = value.Cost;
            SelectedProductPrice = value.Price;
            SelectedProductMinimum = value.Minimum;
            SelectedProductUsesBatches = value.UsesBatches;

            if (!DetailActivated)
            {
                DetailActivated = true;
            }
        }

        UpdateSelectedProductCommand?.NotifyCanExecuteChanged();
        DeleteProductCommand?.NotifyCanExecuteChanged();
    }

    partial void OnDetailActivatedChanged(bool value)
    {
        if (!value)
        {
            SelectedItem = null;
        }
    }

    partial void OnNewProductNameChanged(string value) => RegisterProductCommand?.NotifyCanExecuteChanged();

    partial void OnNewProductCostChanged(decimal value) => RegisterProductCommand?.NotifyCanExecuteChanged();

    partial void OnNewProductPriceChanged(decimal value) => RegisterProductCommand?.NotifyCanExecuteChanged();

    partial void OnNewProductExpirationDateChanged(DateTime? value) => RegisterProductCommand?.NotifyCanExecuteChanged();

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = null;
            await PopulateAsync(SearchText);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            IsBusy = true;
            if (!DetailActivated)
            {
                DetailActivated = true;
            }
            await PopulateAsync(SearchText);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PopulateAsync(string? filter, int? selectProductId = null)
    {
        StatusMessage = null;
        var previousSelectionId = selectProductId ?? (DetailActivated ? SelectedItem?.ProductId : null);

        Items.Clear();
        SelectedItem = null;

        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Lots)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var term = filter.Trim();
            query = query.Where(p => p.Name.Contains(term) || (p.Barcode != null && p.Barcode.Contains(term)) || (p.InternalCode != null && p.InternalCode.Contains(term)));
        }

        var products = await query.OrderBy(p => p.Name).Take(200).ToListAsync();
        foreach (var product in products)
        {
            var stock = product.Lots.Sum(l => l.RemainingQuantity);

            var lotModels = product.Lots
                .OrderBy(l => l.ExpirationDate ?? DateOnly.MaxValue)
                .Select(l => new InventoryLotModel
                {
                    LotCode = l.LotCode,
                    RemainingQuantity = l.RemainingQuantity,
                    Expiration = l.ExpirationDate
                })
                .ToList();

            var closestExpiration = lotModels
                .Where(l => l.Expiration.HasValue)
                .Select(l => l.Expiration)
                .FirstOrDefault();

            var isExpiringSoon = lotModels.Any(l => l.Expiration.HasValue && l.Expiration.Value.ToDateTime(TimeOnly.MinValue) <= DateTime.Today.AddDays(30));

            var item = new InventoryItemModel
            {
                ProductId = product.Id,
                Name = product.Name,
                Barcode = product.Barcode,
                Supplier = product.Supplier?.Name,
                Cost = product.Cost,
                Price = product.Price,
                TaxRate = product.TaxRate,
                UsesBatches = product.UsesBatches,
                Stock = stock,
                Minimum = product.StockMinimum,
                ClosestExpiration = closestExpiration,
                IsExpiringSoon = isExpiringSoon,
                Lots = lotModels
            };

            Items.Add(item);
        }

        UpdateMetrics();

        var shouldSelect = DetailActivated || selectProductId.HasValue;

        if (shouldSelect && previousSelectionId.HasValue)
        {
            SelectedItem = Items.FirstOrDefault(i => i.ProductId == previousSelectionId.Value);
        }

        if (shouldSelect && SelectedItem is null)
        {
            SelectedItem = Items.FirstOrDefault();
        }

        if (!Items.Any())
        {
            StatusMessage = "No se encontraron productos.";
            DetailActivated = false;
        }
    }

    private bool CanUpdateSelectedProduct() => !IsBusy && SelectedItem is not null && SelectedProductPrice > 0m;

    [RelayCommand(CanExecute = nameof(CanUpdateSelectedProduct))]
    private async Task UpdateSelectedProductAsync()
    {
        if (SelectedItem is null)
        {
            StatusMessage = "Selecciona un producto para actualizar.";
            return;
        }

        if (SelectedProductCost < 0m || SelectedProductMinimum < 0m)
        {
            StatusMessage = "Los valores no pueden ser negativos.";
            return;
        }

        try
        {
            IsBusy = true;
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == SelectedItem.ProductId);
            if (product is null)
            {
                StatusMessage = "No se encontró el producto seleccionado.";
                return;
            }

            product.Cost = SelectedProductCost;
            product.Price = SelectedProductPrice;
            product.StockMinimum = SelectedProductMinimum;
            product.UsesBatches = SelectedProductUsesBatches;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            StatusMessage = $"Producto \"{product.Name}\" actualizado.";

            await PopulateAsync(SearchText, product.Id);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanRegisterProduct() => !IsBusy && !string.IsNullOrWhiteSpace(NewProductName) && NewProductPrice > 0m;

    private bool CanDeleteProduct(InventoryItemModel? item) => !IsBusy && (item ?? SelectedItem) is not null;

    [RelayCommand(CanExecute = nameof(CanDeleteProduct))]
    private async Task DeleteProductAsync(InventoryItemModel? item)
    {
        var target = item ?? SelectedItem;
        if (target is null)
        {
            StatusMessage = "Selecciona un producto para eliminar.";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = null;

            SelectedItem = target;
            var productId = target.ProductId;

            var hasSales = await _context.SaleLines.AnyAsync(l => l.ProductId == productId);
            var hasPurchases = await _context.PurchaseLines.AnyAsync(l => l.ProductId == productId);

            if (hasSales || hasPurchases)
            {
                StatusMessage = "No es posible eliminar un producto con movimientos de venta o compra.";
                return;
            }

            var product = await _context.Products
                .Include(p => p.Lots)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product is null)
            {
                StatusMessage = "No se encontró el producto seleccionado.";
                return;
            }

            if (product.Lots.Any(l => l.RemainingQuantity > 0))
            {
                StatusMessage = "Ajusta el inventario a cero antes de eliminar el producto.";
                return;
            }

            if (product.Lots.Any())
            {
                _context.ProductLots.RemoveRange(product.Lots);
            }

            _context.Products.Remove(product);

            var session = _sessionService.Current;
            if (session is not null)
            {
                await _context.ActivityLogs.AddAsync(new ActivityLog
                {
                    UserId = session.UserId,
                    Action = "Producto eliminado",
                    Detail = $"Se eliminó el producto '{product.Name}' (ID {product.Id}).",
                    OccurredAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            await PopulateAsync(SearchText);

            if (!Items.Any())
            {
                DetailActivated = false;
            }

            StatusMessage = $"Producto \"{product.Name}\" eliminado.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRegisterProduct))]
    private async Task RegisterProductAsync()
    {
        if (NewProductCost < 0m || NewProductMinimum < 0m)
        {
            StatusMessage = "El costo y el mínimo deben ser mayores o iguales a cero.";
            return;
        }

        if (NewProductPrice < NewProductCost)
        {
            StatusMessage = "El precio debe ser mayor o igual al costo.";
            return;
        }

        try
        {
            IsBusy = true;

            var product = new Product
            {
                Name = NewProductName.Trim(),
                Barcode = string.IsNullOrWhiteSpace(NewProductBarcode) ? null : NewProductBarcode.Trim(),
                Description = string.IsNullOrWhiteSpace(NewProductDescription) ? null : NewProductDescription.Trim(),
                Cost = NewProductCost,
                Price = NewProductPrice,
                StockMinimum = NewProductMinimum,
                UsesBatches = NewProductUsesBatches
            };

            if (NewProductExpirationDate is DateTime expiration)
            {
                var lot = new ProductLot
                {
                    LotCode = $"AUTO-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    ExpirationDate = DateOnly.FromDateTime(expiration.Date),
                    Quantity = 0m,
                    RemainingQuantity = 0m
                };

                product.Lots.Add(lot);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            StatusMessage = $"Producto \"{product.Name}\" registrado.";

            var filter = product.Name;
            SearchText = filter;

            if (!DetailActivated)
            {
                DetailActivated = true;
            }

            await PopulateAsync(filter, product.Id);

            ResetNewProductFormFields();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ClearNewProductForm() => ResetNewProductFormFields();

    private void ResetNewProductFormFields()
    {
        NewProductName = string.Empty;
        NewProductBarcode = null;
        NewProductDescription = null;
        NewProductCost = 0m;
        NewProductPrice = 0m;
        NewProductMinimum = 0m;
        NewProductUsesBatches = false;
        NewProductExpirationDate = null;
    }

    private void UpdateMetrics()
    {
        TotalSkus = Items.Count;
        LowStockSkus = Items.Count(i => i.IsBelowMinimum);
        ExpiringSoonSkus = Items.Count(i => i.IsExpiringSoon);
        TotalUnits = Items.Sum(i => i.Stock);
    }

}
