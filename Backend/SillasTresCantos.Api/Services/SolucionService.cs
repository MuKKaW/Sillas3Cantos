using MySql.Data.MySqlClient;
using SillasTresCantos.Api.Data;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Services;

public class SolucionService : ISolucionService
{
    private const int MaxTituloLength = 100;
    private const int MaxTextoLength = 255;
    private const int MaxEmojiLength = 32;
    private const int MaxOrdenVisual = 9999;
    private readonly ISolucionRepository _solucionRepository;

    public SolucionService(ISolucionRepository solucionRepository)
    {
        _solucionRepository = solucionRepository;
    }

    public async Task<List<GetSolucionDTO>> GetSolucionesAsync(GetSolucionesFiltroDTO filtro)
    {
        List<Solucion> soluciones = await _solucionRepository.GetSolucionesAsync(filtro.IdSolucion, filtro.Titulo, filtro.OrderAscent);

        return soluciones
            .Select(MapToGetSolucionDTO)
            .ToList();
    }

    public async Task<GetSolucionDTO?> GetSolucionByIdAsync(int id)
    {
        if (id <= 0)
        {
            return null;
        }

        Solucion? existente = await _solucionRepository.GetSolucionByIdAsync(id);
        if (existente is null)
        {
            return null;
        }

        return MapToGetSolucionDTO(existente);
    }

    public async Task<SolucionOperationResult> PostSolucionAsync(PostSolucionDTO solucion)
    {
        string titulo = solucion.Titulo?.Trim() ?? string.Empty;
        string texto = solucion.Texto?.Trim() ?? string.Empty;
        string emoji = solucion.Emoji?.Trim() ?? string.Empty;
        int ordenVisual = solucion.OrdenVisual ?? 0;

        if (!IsValidTitulo(titulo)
            || !IsValidTexto(texto)
            || !IsValidEmoji(emoji)
            || !IsValidOrdenVisual(ordenVisual))
        {
            return SolucionOperationResult.ValidationError();
        }

        bool existeTitulo = await _solucionRepository.ExistsByTituloAsync(titulo);
        if (existeTitulo)
        {
            return SolucionOperationResult.ConflictError();
        }

        Solucion nuevaSolucion = new()
        {
            Id = 0,
            Titulo = titulo,
            Texto = texto,
            Emoji = emoji,
            OrdenVisual = ordenVisual,
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = null
        };

        Solucion? creada;
        try
        {
            creada = await _solucionRepository.CreateSolucionAsync(nuevaSolucion);
        }
        catch (MySqlException ex) when (IsDuplicateKey(ex))
        {
            return SolucionOperationResult.ConflictError();
        }
        catch (MySqlException)
        {
            return SolucionOperationResult.UnexpectedError();
        }

        if (creada is null)
        {
            return SolucionOperationResult.UnexpectedError();
        }

        return SolucionOperationResult.Success(MapToGetSolucionDTO(creada));
    }

    public async Task<SolucionOperationResult> PutSolucionAsync(PutSolucionDTO solucion)
    {
        if (solucion.Id <= 0)
        {
            return SolucionOperationResult.ValidationError();
        }

        Solucion? existente = await _solucionRepository.GetSolucionByIdAsync(solucion.Id);
        if (existente is null)
        {
            return SolucionOperationResult.NotFoundError();
        }

        if ((solucion.Titulo is not null && string.IsNullOrWhiteSpace(solucion.Titulo))
            || (solucion.Texto is not null && string.IsNullOrWhiteSpace(solucion.Texto))
            || (solucion.Emoji is not null && string.IsNullOrWhiteSpace(solucion.Emoji)))
        {
            return SolucionOperationResult.ValidationError();
        }

        string tituloFinal = string.IsNullOrWhiteSpace(solucion.Titulo)
            ? existente.Titulo
            : solucion.Titulo.Trim();
        string textoFinal = string.IsNullOrWhiteSpace(solucion.Texto)
            ? existente.Texto
            : solucion.Texto.Trim();
        string emojiFinal = string.IsNullOrWhiteSpace(solucion.Emoji)
            ? existente.Emoji
            : solucion.Emoji.Trim();
        int ordenVisualFinal = solucion.OrdenVisual ?? existente.OrdenVisual;

        if (!IsValidTitulo(tituloFinal)
            || !IsValidTexto(textoFinal)
            || !IsValidEmoji(emojiFinal)
            || !IsValidOrdenVisual(ordenVisualFinal))
        {
            return SolucionOperationResult.ValidationError();
        }

        bool tituloDuplicado = await _solucionRepository.ExistsByTituloAsync(tituloFinal, solucion.Id);
        if (tituloDuplicado)
        {
            return SolucionOperationResult.ConflictError();
        }

        Solucion actualizada = new()
        {
            Id = existente.Id,
            Titulo = tituloFinal,
            Texto = textoFinal,
            Emoji = emojiFinal,
            OrdenVisual = ordenVisualFinal,
            FechaCreacion = existente.FechaCreacion,
            FechaActualizacion = DateTime.UtcNow
        };

        bool updated;
        try
        {
            updated = await _solucionRepository.UpdateSolucionAsync(actualizada);
        }
        catch (MySqlException ex) when (IsDuplicateKey(ex))
        {
            return SolucionOperationResult.ConflictError();
        }
        catch (MySqlException)
        {
            return SolucionOperationResult.UnexpectedError();
        }

        if (!updated)
        {
            return SolucionOperationResult.NotFoundError();
        }

        return SolucionOperationResult.Success(MapToGetSolucionDTO(actualizada));
    }

    public async Task<SolucionOperationResult> DeleteSolucionAsync(int id)
    {
        if (id <= 0)
        {
            return SolucionOperationResult.ValidationError();
        }

        bool deleted = await _solucionRepository.DeleteSolucionAsync(id);
        if (!deleted)
        {
            return SolucionOperationResult.NotFoundError();
        }

        return SolucionOperationResult.Success();
    }

    private static GetSolucionDTO MapToGetSolucionDTO(Solucion solucion) =>
        new()
        {
            Id = solucion.Id,
            Titulo = solucion.Titulo,
            Texto = solucion.Texto,
            Emoji = solucion.Emoji,
            OrdenVisual = solucion.OrdenVisual,
            FechaCreacion = solucion.FechaCreacion,
            FechaActualizacion = solucion.FechaActualizacion
        };

    private static bool IsValidTitulo(string titulo) =>
        !string.IsNullOrWhiteSpace(titulo) && titulo.Length <= MaxTituloLength;

    private static bool IsValidTexto(string texto) =>
        !string.IsNullOrWhiteSpace(texto) && texto.Length <= MaxTextoLength;

    private static bool IsValidEmoji(string emoji) =>
        !string.IsNullOrWhiteSpace(emoji) && emoji.Length <= MaxEmojiLength;

    private static bool IsValidOrdenVisual(int ordenVisual) =>
        ordenVisual >= 0 && ordenVisual <= MaxOrdenVisual;

    private static bool IsDuplicateKey(MySqlException ex) => ex.Number == 1062;
}
