using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public enum ProductoOperationError
{
    None,
    Validation,
    Conflict,
    NotFound,
    Unexpected
}

public class ProductoOperationResult
{
    public ProductoOperationError Error { get; init; } = ProductoOperationError.None;
    public GetProductoDTO? Producto { get; init; }

    public bool IsSuccess => Error == ProductoOperationError.None;

    public static ProductoOperationResult Success(GetProductoDTO? producto = null) =>
        new()
        {
            Error = ProductoOperationError.None,
            Producto = producto
        };

    public static ProductoOperationResult ValidationError() =>
        new()
        {
            Error = ProductoOperationError.Validation
        };

    public static ProductoOperationResult ConflictError() =>
        new()
        {
            Error = ProductoOperationError.Conflict
        };

    public static ProductoOperationResult NotFoundError() =>
        new()
        {
            Error = ProductoOperationError.NotFound
        };

    public static ProductoOperationResult UnexpectedError() =>
        new()
        {
            Error = ProductoOperationError.Unexpected
        };
}
