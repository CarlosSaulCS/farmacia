using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacia.Data.Contexts;
using Farmacia.Domain.Entities;
using Farmacia.Domain.Enums;
using Farmacia.Domain.Services;
using Farmacia.UI.Wpf.Models;
using Farmacia.UI.Wpf.Services;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.UI.Wpf.ViewModels;

public partial class PosViewModel : ViewModelBase
{
    private readonly PharmacyDbContext _context;
    private readonly ISequenceService _sequenceService;
    private readonly IInventoryService _inventoryService;
    private readonly UserSessionService _sessionService;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private decimal _subtotal;

    [ObservableProperty]
    private decimal _taxTotal;

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private decimal _cashAmount;

    [ObservableProperty]
    private decimal _cardAmount;

    [ObservableProperty]
    private decimal _changeAmount;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private PaymentMethod _selectedPaymentMethod = PaymentMethod.Efectivo;

    public ObservableCollection<CartItemModel> CartItems { get; } = new();

    public IReadOnlyList<PaymentMethod> PaymentMethods { get; } = Enum.GetValues<PaymentMethod>();

    public PosViewModel(PharmacyDbContext context, ISequenceService sequenceService, IInventoryService inventoryService, UserSessionService sessionService)
    {
        _context = context;
        _sequenceService = sequenceService;
        _inventoryService = inventoryService;
        _sessionService = sessionService;
    }

    [RelayCommand]
    private async Task AddProductAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            StatusMessage = "Captura un código o nombre.";
            return;
        }

        try
        {
            IsBusy = true;
            var query = SearchQuery.Trim();

            var product = await _context.Products
                .Include(p => p.Lots)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Barcode == query || p.InternalCode == query || EF.Functions.Like(p.Name, $"%{query}%"));

            if (product is null)
            {
                StatusMessage = "Producto no encontrado.";
                return;
            }

            var (added, message) = await AddOrUpdateCartItemAsync(product, 1);
            if (!added)
            {
                StatusMessage = message ?? "No fue posible agregar el producto.";
                return;
            }

            SearchQuery = string.Empty;
            StatusMessage = message ?? $"Agregado: {product.Name}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void RemoveItem(CartItemModel? item)
    {
        if (item is null)
        {
            return;
        }

        CartItems.Remove(item);
        RecalculateTotals();
        StatusMessage = "Artículo removido.";
    }

    [RelayCommand]
    private void ClearCart()
    {
        CartItems.Clear();
        RecalculateTotals();
        StatusMessage = "Carrito vacío.";
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (!CartItems.Any())
        {
            StatusMessage = "No hay productos en el carrito.";
            return;
        }

        if (SelectedPaymentMethod == PaymentMethod.Efectivo)
        {
            if (CashAmount < Total)
            {
                StatusMessage = "El efectivo no cubre el total.";
                return;
            }
            ChangeAmount = CashAmount - Total;
            CardAmount = 0;
        }
        else if (SelectedPaymentMethod == PaymentMethod.Tarjeta)
        {
            CardAmount = Total;
            CashAmount = 0;
            ChangeAmount = 0;
        }
        else
        {
            if (CashAmount + CardAmount < Total)
            {
                StatusMessage = "El pago mixto es insuficiente.";
                return;
            }
            ChangeAmount = Math.Max(0, CashAmount + CardAmount - Total);
        }

        try
        {
            IsBusy = true;
            var session = _sessionService.Current;
            if (session is null)
            {
                StatusMessage = "No hay un usuario autenticado.";
                return;
            }

            var folio = await _sequenceService.GetNextFolioAsync("VENTA");
            var sale = new Sale
            {
                Folio = folio,
                SaleDate = DateTime.Now,
                Subtotal = Subtotal,
                TaxTotal = TaxTotal,
                Total = Total,
                CashReceived = CashAmount,
                CardReceived = CardAmount,
                ChangeGiven = ChangeAmount,
                PaymentMethod = SelectedPaymentMethod,
                UserId = session.UserId
            };

            foreach (var item in CartItems)
            {
                var lineSubtotal = (item.UnitPrice - item.Discount) * item.Quantity;
                var lineTax = lineSubtotal * item.TaxRate;
                var saleLine = new SaleLine
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Discount = item.Discount,
                    TaxRate = item.TaxRate,
                    LineTotal = lineSubtotal + lineTax,
                    Sale = sale
                };

                sale.Lines.Add(saleLine);
            }

            _context.Sales.Add(sale);
            await _inventoryService.ApplySaleAsync(sale);
            await _context.SaveChangesAsync();

            StatusMessage = $"Venta registrada con folio {folio} por {session.FullName}.";
            CartItems.Clear();
            CashAmount = 0;
            CardAmount = 0;
            ChangeAmount = 0;
            RecalculateTotals();
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

    private async Task<(bool Added, string? Message)> AddOrUpdateCartItemAsync(Product product, decimal quantity)
    {
        var existing = CartItems.FirstOrDefault(c => c.ProductId == product.Id);
        if (existing is null)
        {
            var available = product.UsesBatches
                ? await _context.ProductLots.AsNoTracking().Where(l => l.ProductId == product.Id).SumAsync(l => (decimal?)l.RemainingQuantity) ?? 0
                : 9999;

            if (available <= 0)
            {
                return (false, "Producto sin existencia.");
            }

            var quantityToAdd = Math.Min(quantity, available);
            if (quantityToAdd < quantity)
            {
                CartItems.Add(new CartItemModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = quantityToAdd,
                    UnitPrice = product.Price,
                    TaxRate = product.TaxRate,
                    Discount = 0
                });
                RecalculateTotals();
                return (true, "Se agregó la cantidad disponible restante.");
            }

            CartItems.Add(new CartItemModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = quantityToAdd,
                UnitPrice = product.Price,
                TaxRate = product.TaxRate,
                Discount = 0
            });
            RecalculateTotals();
            return (true, null);
        }
        else
        {
            if (product.UsesBatches)
            {
                var available = await _context.ProductLots.AsNoTracking().Where(l => l.ProductId == product.Id).SumAsync(l => (decimal?)l.RemainingQuantity) ?? 0;
                var remainingCapacity = Math.Max(0, available - existing.Quantity);
                if (remainingCapacity <= 0)
                {
                    return (false, "Producto sin existencia suficiente.");
                }

                var quantityToAdd = Math.Min(quantity, remainingCapacity);
                existing.Quantity += quantityToAdd;

                if (quantityToAdd < quantity)
                {
                    RecalculateTotals();
                    return (true, "Se agregó la cantidad disponible restante.");
                }
            }
            else
            {
                existing.Quantity += quantity;
            }
        }

        RecalculateTotals();
        return (true, null);
    }

    private void RecalculateTotals()
    {
        decimal subtotal = 0;
        decimal tax = 0;
        foreach (var item in CartItems)
        {
            var lineSubtotal = (item.UnitPrice - item.Discount) * item.Quantity;
            var lineTax = lineSubtotal * item.TaxRate;
            subtotal += lineSubtotal;
            tax += lineTax;
        }

        Subtotal = Math.Round(subtotal, 2);
        TaxTotal = Math.Round(tax, 2);
        Total = Subtotal + TaxTotal;
    }
}
