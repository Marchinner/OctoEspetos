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

public partial class QuickOrderViewModel : ViewModelBase
{
    [ObservableProperty]
    private Client _selectedClient = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CheckoutCommand))]
    private decimal _totalAmount;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<Product> Products { get; } = [];

    public ObservableCollection<OrderItem> Cart { get; } = [];

    public Action? OnFinished { get; set; }

    public QuickOrderViewModel()
    {
        SelectedClient = new Client { Name = "Consumidor Final" };


        LoadProducts();
    }

    private void LoadProducts()
    {
        using var db = new AppDbContext();
        var all = db.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .ToList();

        Products.Clear();

        foreach (var p in all) Products.Add(p);
    }

    [RelayCommand]
    private void AddToCart(Product p)
    {
        var existing = Cart.FirstOrDefault(i => i.ProductId == p.Id);

        if (existing is not null)
        {
            existing.Quantity++;

            var index = Cart.IndexOf(existing);
            Cart.RemoveAt(index);
            Cart.Insert(index, existing);
        }
        else
        {
            Cart.Add(new OrderItem { Product = p, ProductId = p.Id, Quantity = 1, UnitPrice = p.SellPrice });
        }

        RecalculateTotal();
    }

    [RelayCommand]
    private void RemoveFromCart(OrderItem item)
    {
        Cart.Remove(item);
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        TotalAmount = Cart.Sum(x => x.Quantity * x.UnitPrice);
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        StatusMessage = "Processando...";

        using (var db = new AppDbContext())
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var client = SelectedClient.Id == Guid.Empty ? new(){Id = Guid.NewGuid(), Name = "Consumidor Final"} : SelectedClient;
                    // 1. Create the Order
                    var order = new Order
                    {
                        CreatedAt = DateTime.Now,
                        TotalAmount = TotalAmount,
                        IsPaid = true, // Immediate payment

                        // If client exists in DB, attach ID. If new, let EF create it.
                        Client = client,
                        ClientId = client.Id
                    };

                    db.Orders.Add(order);
                    await db.SaveChangesAsync();

                    // 2. Add Items
                    foreach (var item in Cart)
                    {
                        item.OrderId = order.Id;
                        item.Product = null; // Prevent EF from trying to re-add the Product
                        db.OrderItems.Add(item);
                    }

                    // 3. Register Payment (Simplified for this view)
                    var payment = new Payment // Assuming you have a Payment entity
                    {
                        OrderId = order.Id,
                        Amount = TotalAmount,
                        PaymentMethod = "Dinheiro", // In real app, bind this to UI selection
                        PaidAt = DateTime.Now
                    };
                    db.Payments.Add(payment); // Uncomment if Payment table exists

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // 4. Reset UI
                    Cart.Clear();
                    RecalculateTotal();
                    StatusMessage = $"Pedido #{order.Id} Finalizado!";

                    // Optional: Reset client to default walk-in
                    SelectedClient = new Client { Name = "Consumidor Final" };

                    await Task.Delay(1000);

                    OnFinished?.Invoke();
                }
                catch (Exception ex)
                {
                    StatusMessage = "Error: " + ex.Message + "\n" + ex.InnerException?.Message;
                    transaction.Rollback();
                }
            }
        }
    }
}
