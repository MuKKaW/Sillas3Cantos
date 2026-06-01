using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public enum ProductoArchivoOperationError
{
    None,
    Validation,
    NotFound,
    UnsupportedType,
    FileTooLarge,
    Unexpected
}

public class ProductoArchivoOperationResult
{
    public ProductoArchivoOperationError Error { get; init; } = ProductoArchivoOperationError.None;
    public GetProductoArchivoDTO? Archivo { get; init; }
    public List<GetProductoArchivoDTO>? Archivos { get; init; }
    public string? AbsoluteFilePath { get; init; }
    public string? DownloadContentType { get; init; }
    public string? DownloadFileName { get; init; }

    public bool IsSuccess => Error == ProductoArchivoOperationError.None;

    public static ProductoArchivoOperationResult SuccessArchivo(GetProductoArchivoDTO archivo) =>
        new()
        {
            Error = ProductoArchivoOperationError.None,
            Archivo = archivo
        };

    public static ProductoArchivoOperationResult SuccessArchivos(List<GetProductoArchivoDTO> archivos) =>
        new()
        {
            Error = ProductoArchivoOperationError.None,
            Archivos = archivos
        };

    public static ProductoArchivoOperationResult SuccessDescarga(
        GetProductoArchivoDTO archivo,
        string absoluteFilePath,
        string contentType,
        string fileName) =>
        new()
        {
            Error = ProductoArchivoOperationError.None,
            Archivo = archivo,
            AbsoluteFilePath = absoluteFilePath,
            DownloadContentType = contentType,
            DownloadFileName = fileName
        };

    public static ProductoArchivoOperationResult Success() =>
        new()
        {
            Error = ProductoArchivoOperationError.None
        };

    public static ProductoArchivoOperationResult ValidationError() =>
        new()
        {
            Error = ProductoArchivoOperationError.Validation
        };

    public static ProductoArchivoOperationResult NotFoundError() =>
        new()
        {
            Error = ProductoArchivoOperationError.NotFound
        };

    public static ProductoArchivoOperationResult UnsupportedTypeError() =>
        new()
        {
            Error = ProductoArchivoOperationError.UnsupportedType
        };

    public static ProductoArchivoOperationResult FileTooLargeError() =>
        new()
        {
            Error = ProductoArchivoOperationError.FileTooLarge
        };

    public static ProductoArchivoOperationResult UnexpectedError() =>
        new()
        {
            Error = ProductoArchivoOperationError.Unexpected
        };
}
