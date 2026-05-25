using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public enum UsuarioOperationError
{
    None,
    Validation,
    Conflict,
    ConflictEmail,
    ConflictUsername,
    NotFound,
    Unexpected
}

public class UsuarioOperationResult
{
    public UsuarioOperationError Error { get; init; } = UsuarioOperationError.None;
    public GetUsuarioDTO? Usuario { get; init; }

    public bool IsSuccess => Error == UsuarioOperationError.None;

    public static UsuarioOperationResult Success(GetUsuarioDTO? usuario = null) =>
        new()
        {
            Error = UsuarioOperationError.None,
            Usuario = usuario
        };

    public static UsuarioOperationResult ValidationError() =>
        new()
        {
            Error = UsuarioOperationError.Validation
        };

    public static UsuarioOperationResult ConflictError() =>
        new()
        {
            Error = UsuarioOperationError.Conflict
        };

    public static UsuarioOperationResult ConflictEmailError() =>
        new()
        {
            Error = UsuarioOperationError.ConflictEmail
        };

    public static UsuarioOperationResult ConflictUsernameError() =>
        new()
        {
            Error = UsuarioOperationError.ConflictUsername
        };

    public static UsuarioOperationResult NotFoundError() =>
        new()
        {
            Error = UsuarioOperationError.NotFound
        };

    public static UsuarioOperationResult UnexpectedError() =>
        new()
        {
            Error = UsuarioOperationError.Unexpected
        };
}
