using System.Text.Json;
using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public class DummyJsonProductosExternosService : IProductosExternosService
{
    private const int MinQueryLength = 2;
    private const int MaxQueryLength = 80;
    private const int MinLimit = 1;
    private const int MaxLimit = 30;
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DummyJsonProductosExternosService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductosExternosOperationResult> BuscarProductosAsync(
        BuscarProductosExternosFiltroDTO filtro,
        CancellationToken cancellationToken = default)
    {
        string query = (filtro.Query ?? string.Empty).Trim();
        int limit = filtro.Limit;
        int skip = filtro.Skip;

        if (!IsValidQuery(query) || !IsValidLimit(limit) || skip < 0)
        {
            return ProductosExternosOperationResult.ValidationError();
        }

        string encodedQuery = Uri.EscapeDataString(query);
        string requestUri = $"products/search?q={encodedQuery}&limit={limit}&skip={skip}&select=id,title,description,category,price,stock,brand,thumbnail,rating";

        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ProductosExternosOperationResult.UpstreamError();
            }

            string content = await response.Content.ReadAsStringAsync(cancellationToken);
            DummyJsonSearchResponse? parsed = JsonSerializer.Deserialize<DummyJsonSearchResponse>(content, JsonOptions);

            if (parsed?.Products is null)
            {
                return ProductosExternosOperationResult.UpstreamError();
            }

            List<GetProductoExternoDTO> productos = parsed.Products
                .Select(MapToGetProductoExternoDTO)
                .ToList();

            GetBusquedaProductosExternosDTO resultado = new()
            {
                Fuente = "dummyjson",
                Query = query,
                Total = parsed.Total,
                Skip = parsed.Skip,
                Limit = parsed.Limit,
                Productos = productos
            };

            return ProductosExternosOperationResult.Success(resultado);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return ProductosExternosOperationResult.TimeoutError();
        }
        catch (HttpRequestException)
        {
            return ProductosExternosOperationResult.UpstreamError();
        }
        catch (JsonException)
        {
            return ProductosExternosOperationResult.UpstreamError();
        }
        catch (Exception)
        {
            return ProductosExternosOperationResult.UnexpectedError();
        }
    }

    private static GetProductoExternoDTO MapToGetProductoExternoDTO(DummyJsonProduct item)
    {
        string nombre = string.IsNullOrWhiteSpace(item.Title)
            ? $"producto-{item.Id}"
            : item.Title.Trim();
        string categoria = string.IsNullOrWhiteSpace(item.Category)
            ? "sin-categoria"
            : item.Category.Trim();

        return new GetProductoExternoDTO
        {
            IdExterno = item.Id,
            Nombre = nombre,
            Descripcion = item.Description,
            Categoria = categoria,
            Marca = item.Brand,
            Precio = item.Price,
            Stock = item.Stock,
            Rating = item.Rating,
            Thumbnail = item.Thumbnail
        };
    }

    private static bool IsValidQuery(string query) =>
        !string.IsNullOrWhiteSpace(query) &&
        query.Length >= MinQueryLength &&
        query.Length <= MaxQueryLength;

    private static bool IsValidLimit(int limit) =>
        limit >= MinLimit && limit <= MaxLimit;

    private sealed class DummyJsonSearchResponse
    {
        public List<DummyJsonProduct>? Products { get; init; }
        public int Total { get; init; }
        public int Skip { get; init; }
        public int Limit { get; init; }
    }

    private sealed class DummyJsonProduct
    {
        public int Id { get; init; }
        public string? Title { get; init; }
        public string? Description { get; init; }
        public string? Category { get; init; }
        public decimal Price { get; init; }
        public int Stock { get; init; }
        public string? Brand { get; init; }
        public string? Thumbnail { get; init; }
        public double Rating { get; init; }
    }
}
