using System.Data.Common;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductsService.Data.Entities;

namespace ProductsService.Data;

public class ProductsDbContext: DbContext
{
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options): base(options)
    {
        
    }
    
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      
        modelBuilder
            .Entity<Product>()
            .Property(p => p.Categories)
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                v => JsonSerializer.Deserialize<string[]>(v, new JsonSerializerOptions()) ?? Array.Empty<string>()
            );
    }
}