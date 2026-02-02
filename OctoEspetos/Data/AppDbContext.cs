using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using OctoEspetos.Models;

namespace OctoEspetos.Data;

public class AppDbContext : DbContext
{

    public DbSet<Product> Products { get; set; }

    public DbSet<ProductCategory> Categories { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<OrderItem> OrderItems { get; set; }

    public DbSet<Client> Clients { get; set; }

    public DbSet<Payment> Payments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Use Personal folder which is guaranteed to be writable on Android/iOS
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "PdvEspetinho");

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var dbPath = Path.Combine(folder, "pdv.db");

        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Client)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.ClientId);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
