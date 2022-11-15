namespace ProductsService.Models;

public class ProductListModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public double Price { get; set; }
    public CurrencyValue LocalPrice { get; set; }
    public string? Details { get; set; }
}

public class ProductDetailsModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string[] Categories { get; set; }
    public double Price { get; set; }
}
