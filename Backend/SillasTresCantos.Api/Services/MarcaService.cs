using MySql.Data.MySqlClient;
using SillasTresCantos.Api.Data;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Services;

public class MarcaService : IMarcaService
{
    private const int MaxNombreLength = 100;
    private const int MaxDescripcionLength = 255;
    private const int MaxPaisOrigenLength = 100;
    private const int MinAnioFundacion = 1800;
    private const int MaxOrdenVisual = 9999;
    private readonly IMarcaRepository _marcaRepository;

    public MarcaService(IMarcaRepository marcaRepository)
    {
        _marcaRepository = marcaRepository;
    }

    public async Task<List<GetMarcaDTO>> GetMarcasAsync(GetMarcasFiltroDTO filtro)
    {
        List<Marca> marcas = await _marcaRepository.GetMarcasAsync(filtro.IdMarca, filtro.Nombre, filtro.OrderAscent);

        if (!filtro.IncludeHidden)
        {
            marcas = marcas
                .Where(marca => marca.EsVisible)
                .ToList();
        }

        return marcas
            .Select(MapToGetMarcaDTO)
            .ToList();
    }

    public async Task<GetMarcaDTO?> GetMarcaByIdAsync(int id)
    {
        if (id <= 0)
        {
            return null;
        }

        Marca? existente = await _marcaRepository.GetMarcaByIdAsync(id);
        if (existente is null)
        {
            return null;
        }

        return MapToGetMarcaDTO(existente);
    }

    public async Task<MarcaOperationResult> PostMarcaAsync(PostMarcaDTO marca)
    {
        string nombre = marca.Nombre?.Trim() ?? string.Empty;
        string? descripcion = NormalizeOptional(marca.Descripcion);
        string? paisOrigen = NormalizeOptional(marca.PaisOrigen);
        int? anioFundacion = marca.AnioFundacion;
        int ordenVisual = marca.OrdenVisual ?? 0;
        bool esVisible = marca.EsVisible ?? true;

        if (!IsValidNombre(nombre)
            || !IsValidDescripcion(descripcion)
            || !IsValidPaisOrigen(paisOrigen)
            || !IsValidAnioFundacion(anioFundacion)
            || !IsValidOrdenVisual(ordenVisual))
        {
            return MarcaOperationResult.ValidationError();
        }

        bool existeNombre = await _marcaRepository.ExistsByNombreAsync(nombre);
        if (existeNombre)
        {
            return MarcaOperationResult.ConflictError();
        }

        Marca nuevaMarca = new()
        {
            Id = 0,
            Nombre = nombre,
            Descripcion = descripcion,
            PaisOrigen = paisOrigen,
            AnioFundacion = anioFundacion,
            OrdenVisual = ordenVisual,
            EsVisible = esVisible,
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = null
        };

        Marca? creada;
        try
        {
            creada = await _marcaRepository.CreateMarcaAsync(nuevaMarca);
        }
        catch (MySqlException ex) when (IsDuplicateKey(ex))
        {
            return MarcaOperationResult.ConflictError();
        }
        catch (MySqlException)
        {
            return MarcaOperationResult.UnexpectedError();
        }

        if (creada is null)
        {
            return MarcaOperationResult.UnexpectedError();
        }

        return MarcaOperationResult.Success(MapToGetMarcaDTO(creada));
    }

    public async Task<MarcaOperationResult> PutMarcaAsync(PutMarcaDTO marca)
    {
        if (marca.Id <= 0)
        {
            return MarcaOperationResult.ValidationError();
        }

        Marca? existente = await _marcaRepository.GetMarcaByIdAsync(marca.Id);
        if (existente is null)
        {
            return MarcaOperationResult.NotFoundError();
        }

        if (marca.Nombre is not null && string.IsNullOrWhiteSpace(marca.Nombre))
        {
            return MarcaOperationResult.ValidationError();
        }

        string nombreFinal = string.IsNullOrWhiteSpace(marca.Nombre)
            ? existente.Nombre
            : marca.Nombre.Trim();
        string? descripcionFinal = marca.Descripcion is null
            ? existente.Descripcion
            : NormalizeOptional(marca.Descripcion);
        string? paisOrigenFinal = marca.PaisOrigen is null
            ? existente.PaisOrigen
            : NormalizeOptional(marca.PaisOrigen);
        int? anioFundacionFinal = marca.AnioFundacion ?? existente.AnioFundacion;
        int ordenVisualFinal = marca.OrdenVisual ?? existente.OrdenVisual;
        bool esVisibleFinal = marca.EsVisible ?? existente.EsVisible;

        if (!IsValidNombre(nombreFinal)
            || !IsValidDescripcion(descripcionFinal)
            || !IsValidPaisOrigen(paisOrigenFinal)
            || !IsValidAnioFundacion(anioFundacionFinal)
            || !IsValidOrdenVisual(ordenVisualFinal))
        {
            return MarcaOperationResult.ValidationError();
        }

        bool nombreDuplicado = await _marcaRepository.ExistsByNombreAsync(nombreFinal, marca.Id);
        if (nombreDuplicado)
        {
            return MarcaOperationResult.ConflictError();
        }

        Marca actualizada = new()
        {
            Id = existente.Id,
            Nombre = nombreFinal,
            Descripcion = descripcionFinal,
            PaisOrigen = paisOrigenFinal,
            AnioFundacion = anioFundacionFinal,
            OrdenVisual = ordenVisualFinal,
            EsVisible = esVisibleFinal,
            FechaCreacion = existente.FechaCreacion,
            FechaActualizacion = DateTime.UtcNow
        };

        bool updated;
        try
        {
            updated = await _marcaRepository.UpdateMarcaAsync(actualizada);
        }
        catch (MySqlException ex) when (IsDuplicateKey(ex))
        {
            return MarcaOperationResult.ConflictError();
        }
        catch (MySqlException)
        {
            return MarcaOperationResult.UnexpectedError();
        }

        if (!updated)
        {
            return MarcaOperationResult.NotFoundError();
        }

        return MarcaOperationResult.Success(MapToGetMarcaDTO(actualizada));
    }

    public async Task<MarcaOperationResult> DeleteMarcaAsync(int id)
    {
        if (id <= 0)
        {
            return MarcaOperationResult.ValidationError();
        }

        bool deleted = await _marcaRepository.DeleteMarcaAsync(id);
        if (!deleted)
        {
            return MarcaOperationResult.NotFoundError();
        }

        return MarcaOperationResult.Success();
    }

    private static GetMarcaDTO MapToGetMarcaDTO(Marca marca) =>
        new()
        {
            Id = marca.Id,
            Nombre = marca.Nombre,
            Descripcion = marca.Descripcion,
            PaisOrigen = marca.PaisOrigen,
            AnioFundacion = marca.AnioFundacion,
            OrdenVisual = marca.OrdenVisual,
            EsVisible = marca.EsVisible,
            FechaCreacion = marca.FechaCreacion,
            FechaActualizacion = marca.FechaActualizacion
        };

    private static bool IsValidNombre(string nombre) =>
        !string.IsNullOrWhiteSpace(nombre) && nombre.Length <= MaxNombreLength;

    private static bool IsValidDescripcion(string? descripcion) =>
        descripcion is null || descripcion.Length <= MaxDescripcionLength;

    private static bool IsValidPaisOrigen(string? paisOrigen) =>
        paisOrigen is null || paisOrigen.Length <= MaxPaisOrigenLength;

    private static bool IsValidAnioFundacion(int? anioFundacion)
    {
        if (!anioFundacion.HasValue)
        {
            return true;
        }

        int maxAnio = DateTime.UtcNow.Year + 1;
        return anioFundacion.Value >= MinAnioFundacion && anioFundacion.Value <= maxAnio;
    }

    private static bool IsValidOrdenVisual(int ordenVisual) =>
        ordenVisual >= 0 && ordenVisual <= MaxOrdenVisual;

    private static string? NormalizeOptional(string? value)
    {
        if (value is null)
        {
            return null;
        }

        string normalized = value.Trim();
        return normalized.Length == 0 ? null : normalized;
    }

    private static bool IsDuplicateKey(MySqlException ex) => ex.Number == 1062;
}
