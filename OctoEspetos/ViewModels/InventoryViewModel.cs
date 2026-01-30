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

public partial class InventoryViewModel : ViewModelBase
{
    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private ProductCategory? _selectedCategory;

    [ObservableProperty]
    private string _newProductName = string.Empty;

    [ObservableProperty]
    private decimal _newProductPrice;

    [ObservableProperty]
    private string _newCategoryName = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isEditingProduct;

    public ObservableCollection<Product> Products { get; } = [];
    public ObservableCollection<ProductCategory> Categories { get; } = [];

    public Action? RequestGoBack { get; set; }

    public InventoryViewModel()
    {
        LoadData();
    }

    private void LoadData()
    {
        using var db = new AppDbContext();

        Products.Clear();
        Categories.Clear();

        var products = db.Products.Include(p => p.Category).ToList();
        foreach (var p in products) Products.Add(p);

        var categories = db.Categories.ToList();
        foreach (var c in categories) Categories.Add(c);
    }

    [RelayCommand]
    private void GoBack()
    {
        RequestGoBack?.Invoke();
    }

    [RelayCommand]
    private async Task AddProductAsync()
    {
        if (string.IsNullOrWhiteSpace(NewProductName) || NewProductPrice <= 0)
        {
            StatusMessage = "Preencha nome e preço válidos";
            return;
        }

        using var db = new AppDbContext();

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = NewProductName,
            SellPrice = NewProductPrice,
            Price = NewProductPrice,
            IsActive = true,
            CategoryId = SelectedCategory?.Id ?? Guid.Empty
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        Products.Add(product);
        NewProductName = string.Empty;
        NewProductPrice = 0;
        StatusMessage = $"Produto '{product.Name}' adicionado!";
    }

    [RelayCommand]
    private async Task SaveProductAsync()
    {
        if (SelectedProduct == null) return;

        using var db = new AppDbContext();
        var product = await db.Products.FindAsync(SelectedProduct.Id);
        
        if (product != null)
        {
            product.Name = SelectedProduct.Name;
            product.SellPrice = SelectedProduct.SellPrice;
            product.Price = SelectedProduct.Price;
            product.CategoryId = SelectedCategory?.Id ?? product.CategoryId;
            product.IsActive = SelectedProduct.IsActive;

            await db.SaveChangesAsync();
            StatusMessage = $"Produto '{product.Name}' atualizado!";
            IsEditingProduct = false;
        }
    }

    [RelayCommand]
    private async Task DeleteProductAsync()
    {
        if (SelectedProduct == null) return;

        using var db = new AppDbContext();
        var product = await db.Products.FindAsync(SelectedProduct.Id);
        
        if (product != null)
        {
            db.Products.Remove(product);
            await db.SaveChangesAsync();
            Products.Remove(SelectedProduct);
            StatusMessage = $"Produto removido!";
            SelectedProduct = null;
        }
    }

    [RelayCommand]
    private async Task AddCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
        {
            StatusMessage = "Digite um nome para a categoria";
            return;
        }

        using var db = new AppDbContext();

        var category = new ProductCategory
        {
            Id = Guid.NewGuid(),
            Name = NewCategoryName
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync();

        Categories.Add(category);
        NewCategoryName = string.Empty;
        StatusMessage = $"Categoria '{category.Name}' adicionada!";
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync()
    {
        if (SelectedCategory == null) return;

        using var db = new AppDbContext();
        var category = await db.Categories.FindAsync(SelectedCategory.Id);
        
        if (category != null)
        {
            db.Categories.Remove(category);
            await db.SaveChangesAsync();
            Categories.Remove(SelectedCategory);
            StatusMessage = $"Categoria removida!";
            SelectedCategory = null;
        }
    }

    [RelayCommand]
    private void EditProduct()
    {
        if (SelectedProduct != null)
        {
            IsEditingProduct = true;
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == SelectedProduct.CategoryId);
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditingProduct = false;
        LoadData();
    }
}
