
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace OrderManagementService.Services;

public class ProductServiceClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContext;

    public ProductServiceClient(
        HttpClient http,
        IConfiguration config,
        IHttpContextAccessor httpContext)
    {
        _http = http;
        _httpContext = httpContext;

        _http.BaseAddress = new Uri(
            config["ProductService:BaseUrl"]!
        );
    }

    public async Task<ProductInfo?> GetProduct(int id)
    {
        // Lấy JWT từ request của người dùng
        var token = _httpContext.HttpContext?
            .Request.Headers.Authorization.ToString();

        using var request = new HttpRequestMessage(
            HttpMethod.Get, $"products/{id}");

        // Chuyển tiếp JWT sang Product Service
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.TryAddWithoutValidation(
                "Authorization", token);
        }

        var response = await _http.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                $"Product Service returned {(int)response.StatusCode}: {error}");
        }

        return await response.Content.ReadFromJsonAsync<ProductInfo>();
    }
}

public class ProductInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("quantity")]
    public int Stock { get; set; }
}
