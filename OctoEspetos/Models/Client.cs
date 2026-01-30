using System;
using System.Collections.Generic;

namespace OctoEspetos.Models;

public class Client
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "Consumidor Final";

    public ICollection<Order> Orders { get; set; } = [];
}
