using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacia.Data.Contexts;
using Farmacia.Domain.Entities;
using Farmacia.UI.Wpf.Models;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.UI.Wpf.ViewModels;

public partial class InventoryViewModel : ViewModelBase
{
    private readonly PharmacyDbContext _context;

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
    private InventoryItemModel? _selectedItem;

    [ObservableProperty]
    private decimal _selectedProductCost;

    [ObservableProperty]
    private decimal _selectedProductPrice;

    [ObservableProperty]
    private decimal _selectedProductMinimum;

    [ObservableProperty]
    private decimal _selectedProductTaxRate;

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
    private decimal _newProductTaxRate = 16m;

    [ObservableProperty]
    private decimal _newProductMinimum;

    [ObservableProperty]
    private bool _newProductUsesBatches;

    public InventoryViewModel(PharmacyDbContext context)
    {
        _context = context;
    }

    partial void OnIsBusyChanged(bool value)
    {
        RegisterProductCommand?.NotifyCanExecuteChanged();
        UpdateSelectedProductCommand?.NotifyCanExecuteChanged();
    }

    partial void OnSelectedItemChanged(InventoryItemModel? value)
    {
        if (value is null)
        {
            SelectedProductCost = 0m;
            SelectedProductPrice = 0m;
            SelectedProductMinimum = 0m;
            SelectedProductTaxRate = 0m;
            SelectedProductUsesBatches = false;
        }
        else
        {
            SelectedProductCost = value.Cost;
            SelectedProductPrice = value.Price;
            SelectedProductMinimum = value.Minimum;
            SelectedProductTaxRate = value.TaxRatePercentage;
            SelectedProductUsesBatches = value.UsesBatches;
        }

        UpdateSelectedProductCommand?.NotifyCanExecuteChanged();
    }

    partial void OnNewProductNameChanged(string value) => RegisterProductCommand?.NotifyCanExecuteChanged();

    partial void OnNewProductPriceChanged(decimal value) => RegisterProductCommand?.NotifyCanExecuteChanged();

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
        var previousSelectionId = selectProductId ?? SelectedItem?.ProductId;

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

        if (previousSelectionId.HasValue)
        {
            SelectedItem = Items.FirstOrDefault(i => i.ProductId == previousSelectionId.Value);
        }

        if (SelectedItem is null)
        {
            SelectedItem = Items.FirstOrDefault();
        }

        if (!Items.Any())
        {
            StatusMessage = "No se encontraron productos.";
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

        if (SelectedProductTaxRate < 0m)
        {
            StatusMessage = "El impuesto no puede ser negativo.";
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
            product.TaxRate = SelectedProductTaxRate / 100m;
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

    [RelayCommand(CanExecute = nameof(CanRegisterProduct))]
    private async Task RegisterProductAsync()
    {
        if (NewProductCost < 0m || NewProductMinimum < 0m)
        {
            StatusMessage = "El costo y el mínimo deben ser mayores o iguales a cero.";
            return;
        }

        if (NewProductTaxRate < 0m)
        {
            StatusMessage = "El impuesto no puede ser negativo.";
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
                TaxRate = NewProductTaxRate / 100m,
                StockMinimum = NewProductMinimum,
                UsesBatches = NewProductUsesBatches
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            StatusMessage = $"Producto \"{product.Name}\" registrado.";

            var filter = product.Name;
            SearchText = filter;

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
        NewProductTaxRate = 16m;
        NewProductMinimum = 0m;
        NewProductUsesBatches = false;
    }

    private void UpdateMetrics()
    {
        TotalSkus = Items.Count;
        LowStockSkus = Items.Count(i => i.IsBelowMinimum);
        ExpiringSoonSkus = Items.Count(i => i.IsExpiringSoon);
        TotalUnits = Items.Sum(i => i.Stock);
    }
}
