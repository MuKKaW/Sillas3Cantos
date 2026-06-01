using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public interface IProductosExternosService
{
    Task<ProductosExternosOperationResult> BuscarProductosAsync(
        BuscarProductosExternosFiltroDTO filtro,
        CancellationToken cancellationToken = default);
}
