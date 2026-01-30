using System;

namespace OctoEspetos.Models;

public class Payment
{
    public Guid Id { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public DateTime? PaidAt { get; set; }
}
