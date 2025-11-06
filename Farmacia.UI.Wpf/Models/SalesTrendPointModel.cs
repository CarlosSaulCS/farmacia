using System;

namespace Farmacia.UI.Wpf.Models;

public class SalesTrendPointModel
{
    public DateOnly Date { get; init; }
    public decimal TotalSales { get; init; }
    public int TicketCount { get; init; }
    public decimal AverageTicket { get; init; }
}
