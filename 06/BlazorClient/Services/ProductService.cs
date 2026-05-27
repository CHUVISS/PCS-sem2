using System.Net.Http.Json;
using BlazorClient.Models;

namespace BlazorClient.Services;

public class ProductService
{
    private readonly HttpClient _http;
    private const string ApiUrl = "http://localhost:5000/api/products";

    public ProductService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _http.GetFromJsonAsync<List<Product>>(ApiUrl) ?? new();
    }

    public async Task CreateAsync(Product product)
    {
        await _http.PostAsJsonAsync(ApiUrl, product);
    }

    public async Task UpdateAsync(Product product)
    {
        await _http.PutAsJsonAsync($"{ApiUrl}/{product.Id}", product);
    }

    public async Task DeleteAsync(int id)
    {
        await _http.DeleteAsync($"{ApiUrl}/{id}");
    }
}
