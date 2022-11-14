using System.Diagnostics;
using ProductsService.Configuration;
using ProductsService.Data.Entities;
using ProductsService.Models;

namespace ProductsService.Extensions;

public static class ProductExtensions
{
    private static readonly ActivitySource Source = new ActivitySource(Constants.ServiceName);
    
    public static ProductListModel ToListModel(this Product p)
    {
        using var activity = Source.StartActivity();
        activity?.SetTag("Product.Id", p.Id);
        activity?.SetTag("Product.Name", p.Name);
        
        return new ProductListModel
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price
        };
    }

    public static ProductDetailsModel ToDetailsModel(this Product p)
    {
        return new ProductDetailsModel
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Categories = p.Categories.ToArray()
        };
    }
}
