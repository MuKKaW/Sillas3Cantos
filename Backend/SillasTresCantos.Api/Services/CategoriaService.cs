using MySql.Data.MySqlClient;
using SillasTresCantos.Api.Data;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Services;

public class CategoriaService : ICategoriaService
{
    private const int MaxNombreLength = 100;
    private const int MaxDescripcionLength = 255;
    private const int MaxOrdenVisual = 9999;
    private readonly ICategoriaRepository _categoriaRepository;

    public CategoriaService(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public async Task<List<GetCategoriaDTO>> GetCategoriasAsync(GetCategoriasFiltroDTO filtro)
    {
        List<Categoria> categorias = await _categoriaRepository.GetCategoriasAsync(filtro.IdCategoria, filtro.Nombre, filtro.OrderAscent);
        return categorias
            .Select(MapToGetCategoriaDTO)
            .ToList();
    }

    public async Task<GetCategoriaDTO?> GetCategoriaByIdAsync(int id)
    {
        if (id <= 0)
        {
            return null;
        }

        Categoria? existente = await _categoriaRepository.GetCategoriaByIdAsync(id);
        if (existente is null)
        {
            return null;
        }

        return MapToGetCategoriaDTO(existente);
    }

    public async Task<CategoriaOperationResult> PostCategoriaAsync(PostCategoriaDTO categoria)
    {
        string nombre = categoria.Nombre?.Trim() ?? string.Empty;
        string? descripcion = NormalizeOptional(categoria.Descripcion);
        int ordenVisual = categoria.OrdenVisual ?? 0;
        bool esVisible = categoria.EsVisible ?? true;

        if (!IsValidNombre(nombre)
            || !IsValidDescripcion(descripcion)
            || !IsValidOrdenVisual(ordenVisual))
        {
            return CategoriaOperationResult.ValidationError();
        }

        bool existeNombre = await _categoriaRepository.ExistsByNombreAsync(nombre);
        if (existeNombre)
        {
            return CategoriaOperationResult.ConflictError();
        }

        Categoria nuevaCategoria = new()
        {
            Id = 0,
            Nombre = nombre,
            Descripcion = descripcion,
            OrdenVisual = ordenVisual,
            EsVisible = esVisible,
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = null
        };

        Categoria? creada;
        try
        {
            creada = await _categoriaRepository.CreateCategoriaAsync(nuevaCategoria);
        }
        catch (MySqlException ex) when (IsDuplicateKey(ex))
        {
            return CategoriaOperationResult.ConflictError();
        }
        catch (MySqlException)
        {
            return CategoriaOperationResult.UnexpectedError();
        }

        if (creada is null)
        {
            return CategoriaOperationResult.UnexpectedError();
        }

        return CategoriaOperationResult.Success(MapToGetCategoriaDTO(creada));
    }

    public async Task<CategoriaOperationResult> PutCategoriaAsync(PutCategoriaDTO categoria)
    {
        if (categoria.Id <= 0)
        {
            return CategoriaOperationResult.ValidationError();
        }

        Categoria? existente = await _categoriaRepository.GetCategoriaByIdAsync(categoria.Id);
        if (existente is null)
        {
            return CategoriaOperationResult.NotFoundError();
        }

        if (categoria.Nombre is not null && string.IsNullOrWhiteSpace(categoria.Nombre))
        {
            return CategoriaOperationResult.ValidationError();
        }

        string nombreFinal = string.IsNullOrWhiteSpace(categoria.Nombre)
            ? existente.Nombre
            : categoria.Nombre.Trim();
        string? descripcionFinal = categoria.Descripcion is null
            ? existente.Descripcion
            : NormalizeOptional(categoria.Descripcion);
        int ordenVisualFinal = categoria.OrdenVisual ?? existente.OrdenVisual;
        bool esVisibleFinal = categoria.EsVisible ?? existente.EsVisible;

        if (!IsValidNombre(nombreFinal)
            || !IsValidDescripcion(descripcionFinal)
            || !IsValidOrdenVisual(ordenVisualFinal))
        {
            return CategoriaOperationResult.ValidationError();
        }

        bool nombreDuplicado = await _categoriaRepository.ExistsByNombreAsync(nombreFinal, categoria.Id);
        if (nombreDuplicado)
        {
            return CategoriaOperationResult.ConflictError();
        }

        Categoria actualizada = new()
        {
            Id = existente.Id,
            Nombre = nombreFinal,
            Descripcion = descripcionFinal,
            OrdenVisual = ordenVisualFinal,
            EsVisible = esVisibleFinal,
            FechaCreacion = existente.FechaCreacion,
            FechaActualizacion = DateTime.UtcNow
        };

        bool updated;
        try
        {
            updated = await _categoriaRepository.UpdateCategoriaAsync(actualizada);
        }
        catch (MySqlException ex) when (IsDuplicateKey(ex))
        {
            return CategoriaOperationResult.ConflictError();
        }
        catch (MySqlException)
        {
            return CategoriaOperationResult.UnexpectedError();
        }

        if (!updated)
        {
            return CategoriaOperationResult.NotFoundError();
        }

        return CategoriaOperationResult.Success(MapToGetCategoriaDTO(actualizada));
    }

    public async Task<CategoriaOperationResult> DeleteCategoriaAsync(int id)
    {
        if (id <= 0)
        {
            return CategoriaOperationResult.ValidationError();
        }

        bool deleted = await _categoriaRepository.DeleteCategoriaAsync(id);
        if (!deleted)
        {
            return CategoriaOperationResult.NotFoundError();
        }

        return CategoriaOperationResult.Success();
    }

    private static GetCategoriaDTO MapToGetCategoriaDTO(Categoria categoria) =>
        new()
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion,
            OrdenVisual = categoria.OrdenVisual,
            EsVisible = categoria.EsVisible,
            FechaCreacion = categoria.FechaCreacion,
            FechaActualizacion = categoria.FechaActualizacion
        };

    private static bool IsValidNombre(string nombre) =>
        !string.IsNullOrWhiteSpace(nombre) && nombre.Length <= MaxNombreLength;

    private static bool IsValidDescripcion(string? descripcion) =>
        descripcion is null || descripcion.Length <= MaxDescripcionLength;

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
