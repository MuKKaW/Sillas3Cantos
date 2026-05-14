using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public enum MarcaOperationError
{
    None,
    Validation,
    Conflict,
    NotFound,
    Unexpected
}

public class MarcaOperationResult
{
    public MarcaOperationError Error { get; init; } = MarcaOperationError.None;
    public GetMarcaDTO? Marca { get; init; }

    public bool IsSuccess => Error == MarcaOperationError.None;

    public static MarcaOperationResult Success(GetMarcaDTO? marca = null) =>
        new()
        {
            Error = MarcaOperationError.None,
            Marca = marca
        };

    public static MarcaOperationResult ValidationError() =>
        new()
        {
            Error = MarcaOperationError.Validation
        };

    public static MarcaOperationResult ConflictError() =>
        new()
        {
            Error = MarcaOperationError.Conflict
        };

    public static MarcaOperationResult NotFoundError() =>
        new()
        {
            Error = MarcaOperationError.NotFound
        };

    public static MarcaOperationResult UnexpectedError() =>
        new()
        {
            Error = MarcaOperationError.Unexpected
        };
}
