# Cloudinary en una API .NET

## Objetivo

Cloudinary permite guardar imagenes y documentos fuera del servidor de la API. La API recibe un archivo, lo valida, lo sube a Cloudinary y guarda en base de datos la URL segura junto con los metadatos necesarios.

En SillasTresCantos se usara el patron ya aprobado en `PERPETUUM-backend`.

## Que es Cloudinary

Cloudinary es un servicio de almacenamiento y entrega de archivos multimedia. Permite subir imagenes, videos y documentos, obtener URLs `https` y aplicar transformaciones a imagenes.

Para una API Dockerizada es util porque evita depender del disco local del contenedor.

## Patron elegido

Referencia principal:

```text
PERPETUUM-backend
```

Piezas del patron:

- SDK .NET `CloudinaryDotNet`.
- Carga de `.env` con `dotenv.net`.
- Variable unica `CLOUDINARY_URL`.
- Wrapper que solo crea Cloudinary si la variable existe y es valida.
- App capaz de arrancar sin Cloudinary configurado.
- Subida de imagenes con `ImageUploadParams`.
- Transformaciones de imagen.

Formato de `CLOUDINARY_URL`:

```env
CLOUDINARY_URL=cloudinary://API_KEY:API_SECRET@CLOUD_NAME
```

No se deben guardar credenciales reales en `appsettings.json`, documentacion ni commits.

## Paquetes

En SillasTresCantos se usaran las mismas versiones base que `PERPETUUM`:

```xml
<PackageReference Include="CloudinaryDotNet" Version="1.28.0" />
<PackageReference Include="dotenv.net" Version="4.0.1" />
```

## Wrapper

El wrapper evita que Cloudinary rompa el arranque de la API si falta configuracion:

```csharp
public class CloudinaryWrapper
{
    public Cloudinary? Instance { get; }
    public bool IsConfigured => Instance != null;

    public CloudinaryWrapper(string? cloudinaryUrl)
    {
        if (!string.IsNullOrWhiteSpace(cloudinaryUrl) &&
            cloudinaryUrl.StartsWith("cloudinary://", StringComparison.OrdinalIgnoreCase))
        {
            Instance = new Cloudinary(cloudinaryUrl);
            Instance.Api.Secure = true;
        }
    }
}
```

Registro en `Program.cs`:

```csharp
DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));

builder.Services.AddSingleton(_ =>
{
    string? cloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL");
    return new CloudinaryWrapper(cloudinaryUrl);
});
```

## Subida de imagenes

Para imagenes se usa `ImageUploadParams`:

```csharp
ImageUploadParams uploadParams = new()
{
    File = new FileDescription(archivo.FileName, stream),
    Folder = folder,
    Transformation = new Transformation().Width(1200).Crop("limit")
};
```

El resultado importante es:

- `SecureUrl`
- `PublicId`
- `ResourceType = image`

## Subida de documentos

Para documentos se usa `RawUploadParams`:

```csharp
RawUploadParams uploadParams = new()
{
    File = new FileDescription(archivo.FileName, stream),
    Folder = folder
};
```

El resultado importante es:

- `SecureUrl`
- `PublicId`
- `ResourceType = raw`

## Borrado

Para borrar en Cloudinary hace falta el `publicId`. La URL no basta.

```csharp
DeletionParams deleteParams = new(publicId)
{
    ResourceType = resourceType
};

DeletionResult result = await cloudinary.DestroyAsync(deleteParams);
```

Resultados aceptables:

- `ok`
- `not found`

## Adaptacion a SillasTresCantos

| PERPETUUM | SillasTresCantos |
| --- | --- |
| `CloudinaryWrapper` | `Services/Storage/CloudinaryWrapper` |
| `Photo` | `IFormFile archivo` |
| `PhotoURL` / `MediaURL` | `storage_url` |
| Controladores suben imagenes | Storage service sube archivos |
| Transformacion 1:1 para retratos | Transformacion `w_1200,c_limit` para productos |

En SillasTresCantos el controlador debe seguir limpio:

- Recibe `multipart/form-data`.
- Llama al servicio de negocio.
- Devuelve DTO o descarga.

Cloudinary se encapsula en:

- `CloudinaryWrapper`.
- `CloudinaryProductoArchivoStorageService`.
- `IProductoArchivoStorageService`.

## Seguridad

Las URLs publicas de Cloudinary sirven para catalogos y archivos publicos. Si en algun momento los documentos deben ser realmente privados, habria que usar recursos privados o URLs firmadas.

Regla del proyecto:

- No subir secretos reales.
- Usar `.env` local.
- Mantener `.env.example` sin valores.
