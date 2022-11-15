namespace ProductsService.Models;

public class ProductListModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = String.Empty;
    public string Description { get; init; } = String.Empty;
    public double Price { get; init; } = 0;
    public CurrencyValue? LocalPrice { get; set; } 
    public string? Details { get; set; }
}

public class ProductDetailsModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = String.Empty;
    public string Description { get; init; } = String.Empty;
    public string[] Categories { get; init; } = Array.Empty<string>();
    public double Price { get; init; } = 0;
}
