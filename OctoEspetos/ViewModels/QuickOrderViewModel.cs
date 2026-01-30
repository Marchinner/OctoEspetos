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
    [NotifyCanExecuteChangedFor(nameof(SaveOpenOrderCommand))]
    private decimal _totalAmount;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _clientName = "Consumidor Final";

    [ObservableProperty]
    private int? _editingOrderId;

    [ObservableProperty]
    private string _selectedPaymentMethod = "Dinheiro";

    public ObservableCollection<Product> Products { get; } = [];

    public ObservableCollection<OrderItem> Cart { get; } = [];

    public ObservableCollection<string> PaymentMethods { get; } = 
    [
        "Dinheiro",
        "PIX",
        "Cartão Débito",
        "Cartão Crédito"
    ];

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

    /// <summary>
    /// Carrega um pedido existente para edição
    /// </summary>
    public void LoadOrder(Order order)
    {
        EditingOrderId = order.Id;
        ClientName = order.Client?.Name ?? "Consumidor Final";
        SelectedClient = order.Client ?? new Client { Name = ClientName };

        Cart.Clear();

        using var db = new AppDbContext();
        var items = db.OrderItems
            .Include(i => i.Product)
            .Where(i => i.OrderId == order.Id)
            .ToList();

        foreach (var item in items)
        {
            Cart.Add(new OrderItem
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Product = item.Product,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                OrderId = item.OrderId
            });
        }

        RecalculateTotal();
        StatusMessage = $"Editando Pedido #{order.Id}";
    }

    [RelayCommand]
    private void AddToCart(Product p)
    {
        var existing = Cart.FirstOrDefault(i => i.ProductId == p.Id);

        if (existing is not null)
        {
            existing.Quantity++;
            RefreshCartItem(existing);
        }
        else
        {
            Cart.Add(new OrderItem { Product = p, ProductId = p.Id, Quantity = 1, UnitPrice = p.SellPrice });
        }

        RecalculateTotal();
    }

    [RelayCommand]
    private void IncrementQuantity(OrderItem item)
    {
        item.Quantity++;
        RefreshCartItem(item);
        RecalculateTotal();
    }

    [RelayCommand]
    private void DecrementQuantity(OrderItem item)
    {
        if (item.Quantity > 1)
        {
            item.Quantity--;
            RefreshCartItem(item);
            RecalculateTotal();
        }
        else
        {
            RemoveFromCart(item);
        }
    }

    private void RefreshCartItem(OrderItem item)
    {
        var index = Cart.IndexOf(item);
        if (index >= 0)
        {
            Cart.RemoveAt(index);
            Cart.Insert(index, item);
        }
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

    private bool CanSaveOrder() => TotalAmount > 0;

    [RelayCommand(CanExecute = nameof(CanSaveOrder))]
    private async Task SaveOpenOrderAsync()
    {
        StatusMessage = "Salvando pedido...";

        using var db = new AppDbContext();
        using var transaction = db.Database.BeginTransaction();

        try
        {
            Order order;

            if (EditingOrderId.HasValue)
            {
                order = await db.Orders
                    .Include(o => o.Client)
                    .FirstOrDefaultAsync(o => o.Id == EditingOrderId.Value)
                    ?? throw new Exception("Pedido não encontrado");

                var oldItems = db.OrderItems.Where(i => i.OrderId == order.Id);
                db.OrderItems.RemoveRange(oldItems);

                if (order.Client != null && order.Client.Name != ClientName)
                {
                    order.Client.Name = ClientName;
                }

                order.TotalAmount = TotalAmount;
            }
            else
            {
                var client = new Client { Id = Guid.NewGuid(), Name = ClientName };
                order = new Order
                {
                    CreatedAt = DateTime.Now,
                    TotalAmount = TotalAmount,
                    IsPaid = false,
                    Client = client,
                    ClientId = client.Id
                };
                db.Orders.Add(order);
                await db.SaveChangesAsync();
            }

            foreach (var item in Cart)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };
                db.OrderItems.Add(orderItem);
            }

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            var action = EditingOrderId.HasValue ? "Atualizado" : "Aberto";
            StatusMessage = $"Pedido #{order.Id} {action}!";

            Cart.Clear();
            RecalculateTotal();
            ClientName = "Consumidor Final";
            EditingOrderId = null;

            await Task.Delay(1000);
            OnFinished?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = "Erro: " + ex.Message;
            await transaction.RollbackAsync();
        }
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        StatusMessage = "Processando...";

        using var db = new AppDbContext();
        using var transaction = db.Database.BeginTransaction();

        try
        {
            Order order;

            if (EditingOrderId.HasValue)
            {
                order = await db.Orders
                    .Include(o => o.Client)
                    .FirstOrDefaultAsync(o => o.Id == EditingOrderId.Value)
                    ?? throw new Exception("Pedido não encontrado");

                var oldItems = db.OrderItems.Where(i => i.OrderId == order.Id);
                db.OrderItems.RemoveRange(oldItems);

                order.TotalAmount = TotalAmount;
                order.IsPaid = true;

                if (order.Client != null && order.Client.Name != ClientName)
                {
                    order.Client.Name = ClientName;
                }

                await db.SaveChangesAsync();
            }
            else
            {
                var client = new Client { Id = Guid.NewGuid(), Name = ClientName };
                order = new Order
                {
                    CreatedAt = DateTime.Now,
                    TotalAmount = TotalAmount,
                    IsPaid = true,
                    Client = client,
                    ClientId = client.Id
                };

                db.Orders.Add(order);
                await db.SaveChangesAsync();
            }

            foreach (var item in Cart)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };
                db.OrderItems.Add(orderItem);
            }

            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = TotalAmount,
                PaymentMethod = SelectedPaymentMethod,
                PaidAt = DateTime.Now
            };
            db.Payments.Add(payment);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            Cart.Clear();
            RecalculateTotal();
            StatusMessage = $"Pedido #{order.Id} - {SelectedPaymentMethod}!";
            ClientName = "Consumidor Final";
            EditingOrderId = null;
            SelectedPaymentMethod = "Dinheiro";

            await Task.Delay(1000);
            OnFinished?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = "Erro: " + ex.Message + "\n" + ex.InnerException?.Message;
            await transaction.RollbackAsync();
        }
    }
}
