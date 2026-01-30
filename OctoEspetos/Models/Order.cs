using System;
using System.Collections.Generic;

namespace OctoEspetos.Models;

public class Order
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsPaid { get; set; } = false;

    public decimal TotalAmount { get; set; }

    public Guid ClientId { get; set; }

    public Client Client { get; set; } = null!;

    public ICollection<OrderItem> Items { get; set; } = [];

    public ICollection<Payment> Payments { get; set; } = [];
}
