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
                // Atualizando pedido existente
                order = await db.Orders
                    .Include(o => o.Client)
                    .FirstOrDefaultAsync(o => o.Id == EditingOrderId.Value)
                    ?? throw new Exception("Pedido não encontrado");

                // Remover itens antigos
                var oldItems = db.OrderItems.Where(i => i.OrderId == order.Id);
                db.OrderItems.RemoveRange(oldItems);

                // Atualizar cliente se nome mudou
                if (order.Client != null && order.Client.Name != ClientName)
                {
                    order.Client.Name = ClientName;
                }

                order.TotalAmount = TotalAmount;
            }
            else
            {
                // Criando novo pedido
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

            // Adicionar itens
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

            // Reset UI
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

        using (var db = new AppDbContext())
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    Order order;

                    if (EditingOrderId.HasValue)
                    {
                        // Finalizando pedido existente
                        order = await db.Orders
                            .Include(o => o.Client)
                            .FirstOrDefaultAsync(o => o.Id == EditingOrderId.Value)
                            ?? throw new Exception("Pedido não encontrado");

                        // Remover itens antigos
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

                    // Adicionar itens
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

                    // Registrar pagamento
                    var payment = new Payment
                    {
                        OrderId = order.Id,
                        Amount = TotalAmount,
                        PaymentMethod = "Dinheiro",
                        PaidAt = DateTime.Now
                    };
                    db.Payments.Add(payment);

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    Cart.Clear();
                    RecalculateTotal();
                    StatusMessage = $"Pedido #{order.Id} Finalizado!";
                    ClientName = "Consumidor Final";
                    EditingOrderId = null;

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
    }
}
