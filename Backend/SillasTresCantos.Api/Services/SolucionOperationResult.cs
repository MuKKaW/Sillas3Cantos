using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public enum SolucionOperationError
{
    None,
    Validation,
    Conflict,
    NotFound,
    Unexpected
}

public class SolucionOperationResult
{
    public SolucionOperationError Error { get; init; } = SolucionOperationError.None;
    public GetSolucionDTO? Solucion { get; init; }

    public bool IsSuccess => Error == SolucionOperationError.None;

    public static SolucionOperationResult Success(GetSolucionDTO? solucion = null) =>
        new()
        {
            Error = SolucionOperationError.None,
            Solucion = solucion
        };

    public static SolucionOperationResult ValidationError() =>
        new()
        {
            Error = SolucionOperationError.Validation
        };

    public static SolucionOperationResult ConflictError() =>
        new()
        {
            Error = SolucionOperationError.Conflict
        };

    public static SolucionOperationResult NotFoundError() =>
        new()
        {
            Error = SolucionOperationError.NotFound
        };

    public static SolucionOperationResult UnexpectedError() =>
        new()
        {
            Error = SolucionOperationError.Unexpected
        };
}
