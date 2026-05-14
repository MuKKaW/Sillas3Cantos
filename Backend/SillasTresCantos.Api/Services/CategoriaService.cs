using MySql.Data.MySqlClient;
using SillasTresCantos.Api.Data;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Services;

public class CategoriaService : ICategoriaService
{
    private const int MaxNombreLength = 100;
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
        if (!IsValidNombre(nombre))
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
            Nombre = nombre
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

        if (!IsValidNombre(nombreFinal))
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
            Nombre = nombreFinal
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
            Nombre = categoria.Nombre
        };

    private static bool IsValidNombre(string nombre) =>
        !string.IsNullOrWhiteSpace(nombre) && nombre.Length <= MaxNombreLength;

    private static bool IsDuplicateKey(MySqlException ex) => ex.Number == 1062;
}
