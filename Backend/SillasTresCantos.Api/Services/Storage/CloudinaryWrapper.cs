using CloudinaryDotNet;

namespace SillasTresCantos.Api.Services.Storage;

public sealed class CloudinaryWrapper
{
    private const string CloudinaryUrlEnvironmentVariable = "CLOUDINARY_URL";

    public Cloudinary? Client { get; }
    public string? ConfigurationError { get; }
    public bool IsConfigured => Client is not null;

    public CloudinaryWrapper()
    {
        string? cloudinaryUrl = Environment.GetEnvironmentVariable(CloudinaryUrlEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(cloudinaryUrl))
        {
            ConfigurationError = "CLOUDINARY_URL no esta configurada.";
            return;
        }

        cloudinaryUrl = cloudinaryUrl.Trim();
        if (!cloudinaryUrl.StartsWith("cloudinary://", StringComparison.OrdinalIgnoreCase))
        {
            ConfigurationError = "CLOUDINARY_URL debe empezar por cloudinary://.";
            return;
        }

        try
        {
            Cloudinary cloudinary = new(cloudinaryUrl);
            cloudinary.Api.Secure = true;
            Client = cloudinary;
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or UriFormatException)
        {
            ConfigurationError = $"CLOUDINARY_URL no es valida: {ex.Message}";
        }
    }

    public Cloudinary GetRequiredClient()
    {
        if (Client is null)
        {
            throw new InvalidOperationException(ConfigurationError ?? "Cloudinary no esta configurado.");
        }

        return Client;
    }
}
