using System;
using System.Collections.Generic;

namespace OctoEspetos.Models;

public class ProductCategory
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = [];
}
