using System.Linq;
using OctoEspetos.Models;

namespace OctoEspetos.Data;

public static class DataSeeder
{
    public static void SeedDatabase(AppDbContext context)
    {
        context.Database.EnsureCreated();

        if (!context.Categories.Any())
        {
            var newCategories = new[] {
                new ProductCategory{Name = "Espetinhos"},
                new ProductCategory{Name = "Bebidas"},
            };

            context.Categories.AddRange(newCategories);
            context.SaveChanges();
        }

        if (!context.Products.Any())
        {
            var espetinho = context.Categories.First(c => c.Name == "Espetinhos");
            var bebida = context.Categories.First(c => c.Name == "Bebidas");

            var products = new[]
            {
            new Product{Name = "Espeto Carne", Price = 3.50m, SellPrice = 7.00m, Category = espetinho},
            new Product{Name = "Espeto Coxa Frango", Price = 2.50m, SellPrice = 6.00m, Category = espetinho},
            new Product{Name = "Espeto Frango", Price = 2.80m, SellPrice = 6.00m, Category = espetinho},
            new Product{Name = "Espeto Linguiça", Price = 2.50m, SellPrice = 6.00m, Category = espetinho},
            new Product{Name = "Espeto Medalhão Frango", Price = 5.00m, SellPrice = 8.50m, Category = espetinho},
            new Product{Name = "Espeto Medalhão Queijo", Price = 5.00m, SellPrice = 7.50m, Category = espetinho},
            new Product{Name = "Espeto Kafta", Price = 3.50m, SellPrice = 7.50m, Category = espetinho},

            new Product{Name = "Guaraná Antarctica Lata 350 ml", Price = 3.00m, SellPrice = 5.00m, Category = bebida},
            new Product{Name = "Cola-Cola Lata 350 ml", Price = 3.00m, SellPrice = 5.00m, Category = bebida},
            new Product{Name = "Cerveja Brahma Lata 350 ml", Price = 3.00m, SellPrice = 5.00m, Category = bebida},
            new Product{Name = "Cerveja Skol Lata 350 ml", Price = 3.00m, SellPrice = 5.00m, Category = bebida},
        };

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}
