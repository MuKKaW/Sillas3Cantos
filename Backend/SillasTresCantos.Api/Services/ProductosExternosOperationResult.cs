using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public enum ProductosExternosOperationError
{
    None,
    Validation,
    Timeout,
    Upstream,
    Unexpected
}

public class ProductosExternosOperationResult
{
    public ProductosExternosOperationError Error { get; init; } = ProductosExternosOperationError.None;
    public GetBusquedaProductosExternosDTO? Respuesta { get; init; }

    public bool IsSuccess => Error == ProductosExternosOperationError.None;

    public static ProductosExternosOperationResult Success(GetBusquedaProductosExternosDTO? respuesta = null) =>
        new()
        {
            Error = ProductosExternosOperationError.None,
            Respuesta = respuesta
        };

    public static ProductosExternosOperationResult ValidationError() =>
        new()
        {
            Error = ProductosExternosOperationError.Validation
        };

    public static ProductosExternosOperationResult TimeoutError() =>
        new()
        {
            Error = ProductosExternosOperationError.Timeout
        };

    public static ProductosExternosOperationResult UpstreamError() =>
        new()
        {
            Error = ProductosExternosOperationError.Upstream
        };

    public static ProductosExternosOperationResult UnexpectedError() =>
        new()
        {
            Error = ProductosExternosOperationError.Unexpected
        };
}
