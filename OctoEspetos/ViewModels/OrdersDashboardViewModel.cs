using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using OctoEspetos.Data;
using OctoEspetos.Models;

namespace OctoEspetos.ViewModels;

public partial class OrdersDashboardViewModel : ViewModelBase
{
    // --- COLLECTIONS ---
    public ObservableCollection<Order> OpenOrders { get; } = new();
    public ObservableCollection<Order> FinishedOrders { get; } = new();

    // --- PROPERTIES ---
    [ObservableProperty]
    private decimal _totalRevenueToday;

    [ObservableProperty]
    private DateTimeOffset _selectedDate;

    // Navigation back to POS
    public Action? RequestGoBack { get; set; }

    // Navigation to edit order
    public Action<Order>? RequestEditOrder { get; set; }

    public OrdersDashboardViewModel()
    {
        SelectedDate = DateTimeOffset.Now;
        LoadData();
    }

    // --- COMMANDS ---

    [RelayCommand]
    public void LoadData()
    {
        using var db = new AppDbContext();

        // 1. Fetch Open Orders
        var open = db.Orders
                     .Include(o => o.Client)
                     .Where(o => !o.IsPaid)
                     .OrderByDescending(o => o.CreatedAt)
                     .ToList();

        // 2. Fetch Finished Orders (Filtered by Date)
        var targetDate = SelectedDate.Date;
        var nextDate = targetDate.AddDays(1);

        var finished = db.Orders
                         .Include(o => o.Client)
                         .Where(o => o.IsPaid
                                  && o.CreatedAt >= targetDate
                                  && o.CreatedAt < nextDate)
                         .OrderByDescending(o => o.CreatedAt)
                         .ToList();

        // 3. Update UI Collections
        OpenOrders.Clear();
        foreach (var o in open) OpenOrders.Add(o);

        FinishedOrders.Clear();
        foreach (var o in finished) FinishedOrders.Add(o);

        // 4. Calculate Total
        TotalRevenueToday = FinishedOrders.Sum(o => o.TotalAmount);
    }

    [RelayCommand]
    public void GoBack()
    {
        RequestGoBack?.Invoke();
    }

    [RelayCommand]
    public void EditOrder(Order order)
    {
        RequestEditOrder?.Invoke(order);
    }

    [RelayCommand]
    public async Task PayOrderAsync(Order order)
    {
        using var db = new AppDbContext();
        using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            var dbOrder = await db.Orders.FindAsync(order.Id);
            if (dbOrder == null) return;

            dbOrder.IsPaid = true;

            // Registrar pagamento
            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                PaymentMethod = "Dinheiro",
                PaidAt = DateTime.Now
            };
            db.Payments.Add(payment);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            // Atualizar listas
            LoadData();
        }
        catch
        {
            await transaction.RollbackAsync();
        }
    }
}
