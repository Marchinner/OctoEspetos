using System;

namespace OctoEspetos.Models;

public class Product
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal SellPrice { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid CategoryId { get; set; }

    public ProductCategory Category { get; set; } = null!;
}
