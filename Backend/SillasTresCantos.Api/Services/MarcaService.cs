using MySql.Data.MySqlClient;
using SillasTresCantos.Api.Data;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Services;

public class MarcaService : IMarcaService
{
    private const int MaxNombreLength = 100;
    private readonly IMarcaRepository _marcaRepository;

    public MarcaService(IMarcaRepository marcaRepository)
    {
        _marcaRepository = marcaRepository;
    }

    public async Task<List<GetMarcaDTO>> GetMarcasAsync(GetMarcasFiltroDTO filtro)
    {
        List<Marca> marcas = await _marcaRepository.GetMarcasAsync(filtro.IdMarca, filtro.Nombre, filtro.OrderAscent);
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
        if (!IsValidNombre(nombre))
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
            Nombre = nombre
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

        if (!IsValidNombre(nombreFinal))
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
            Nombre = nombreFinal
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
            Nombre = marca.Nombre
        };

    private static bool IsValidNombre(string nombre) =>
        !string.IsNullOrWhiteSpace(nombre) && nombre.Length <= MaxNombreLength;

    private static bool IsDuplicateKey(MySqlException ex) => ex.Number == 1062;
}
