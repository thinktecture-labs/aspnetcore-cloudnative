﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductsService.Data;
using ProductsService.Extensions;
using ProductsService.Models;
using ProductsService.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace ProductsService.Controllers;

[ApiController]
[Route("products")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{ 
    private readonly ILogger<ProductsController> _logger;
    private readonly CurrencyService _currencyService;
    private readonly ProductsDbContext _dbContext;
    
    public ProductsController(ProductsDbContext dbContext, ILogger<ProductsController> logger, CurrencyService currencyService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _currencyService = currencyService;
    }

    [HttpGet]
    [Route("", Name= "GetAllProducts")]
    [SwaggerOperation(OperationId = "GetAllProducts", Tags = new []{"Products"}, Summary = "Get all products", Description = "Call into this endpoint to retrieve a list of all products")]
    [SwaggerResponse(200, Description = "A list with all products", Type = typeof(IEnumerable<ProductListModel>))]
    [SwaggerResponse(500)]
    public async Task<IActionResult> GetAllProductsAsync()
    {
        var products = await _dbContext.Products.ToListAsync();

        var results = products.Select(p => p.ToListModel()).ToList();
        foreach (var result in results)
        {
            result.LocalPrice = await _currencyService.Convert("USD", result.Price);
        }
        
        return Ok(results);
    }

    [HttpGet]
    [Route("{id:guid}", Name = "GetProductById")]
    [SwaggerOperation(OperationId = "GetProductById", Tags = new []{"Products"}, Summary = "Get a product by its id", Description = "Call into this endpoint to retrieve a particular product by its identifier")]
    [SwaggerResponse(200, Description = "The found product", Type = typeof(ProductDetailsModel))]
    [SwaggerResponse(400)]
    [SwaggerResponse(404, Description = "No product was found with the given id")]
    [SwaggerResponse(500)]
    public async Task<IActionResult> GetProductByIdAsync([FromRoute]Guid id){
        var res = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);

        if (res == null) 
        {
            _logger.LogTrace("Product with id {Id} not found. Will result in 404", id);

            return NotFound();
        }

        return Ok(res.ToDetailsModel());
    }
}
