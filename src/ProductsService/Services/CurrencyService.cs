using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ProductsService.Models;

namespace ProductsService.Services;

public class CurrencyService
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    private ExchangeRates? _rates = null;
    
    public CurrencyService(HttpClient client, IOptions<JsonSerializerOptions> jsonSerializerOptions)
    {
        _client = client;
        _jsonSerializerOptions = jsonSerializerOptions.Value;
    }
    public async Task<CurrencyValue> Convert(string currency, double value)
    {
        if (_rates == null)
        {
            _rates = await _client.GetFromJsonAsync<ExchangeRates>("/latest", _jsonSerializerOptions);
        }

        var rate = _rates.Rates[currency];
        return new CurrencyValue(_rates.Date, value * rate);
    }

    private class ExchangeRates
    {
        public DateOnly Date { get; set; }
        public Dictionary<string,double> Rates { get; set; }
    }
}