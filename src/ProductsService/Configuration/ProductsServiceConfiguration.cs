namespace ProductsService.Configuration;

public class ProductsServiceConfiguration
{
    public const string SectionName = "ProductsService";
    public string ZipkinEndpoint { get; set; } = String.Empty;
    public string PathBase { get; set; } = String.Empty;
}
