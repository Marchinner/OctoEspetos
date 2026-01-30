using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    private DateTimeOffset _selectedDate; // To filter history

    // Navigation back to POS
    public Action? RequestGoBack { get; set; }

    public OrdersDashboardViewModel()
    {
        SelectedDate = DateTimeOffset.Now; // Default to Today
        LoadData();
    }

    // --- COMMANDS ---

    [RelayCommand]
    public void LoadData()
    {
        using var db = new AppDbContext();

        // 1. Fetch Open Orders (Any date, because they are pending)
        var open = db.Orders
                     .Include(o => o.Client) // Load Client Name
                     .Where(o => !o.IsPaid)
                     .OrderByDescending(o => o.CreatedAt)
                     .ToList();

        // 2. Fetch Finished Orders (Filtered by Date)
        // We compare Date parts to ignore the time component
        var targetDate = SelectedDate.Date; // This property returns a DateTime
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
}
