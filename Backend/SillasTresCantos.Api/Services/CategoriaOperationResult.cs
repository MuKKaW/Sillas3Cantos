using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public enum CategoriaOperationError
{
    None,
    Validation,
    Conflict,
    NotFound,
    Unexpected
}

public class CategoriaOperationResult
{
    public CategoriaOperationError Error { get; init; } = CategoriaOperationError.None;
    public GetCategoriaDTO? Categoria { get; init; }

    public bool IsSuccess => Error == CategoriaOperationError.None;

    public static CategoriaOperationResult Success(GetCategoriaDTO? categoria = null) =>
        new()
        {
            Error = CategoriaOperationError.None,
            Categoria = categoria
        };

    public static CategoriaOperationResult ValidationError() =>
        new()
        {
            Error = CategoriaOperationError.Validation
        };

    public static CategoriaOperationResult ConflictError() =>
        new()
        {
            Error = CategoriaOperationError.Conflict
        };

    public static CategoriaOperationResult NotFoundError() =>
        new()
        {
            Error = CategoriaOperationError.NotFound
        };

    public static CategoriaOperationResult UnexpectedError() =>
        new()
        {
            Error = CategoriaOperationError.Unexpected
        };
}
