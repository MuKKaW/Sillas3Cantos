# Backend Context

Este documento da contexto operativo para trabajar en el backend de ORTOPEDIA PUEBLOS 3C. Afecta a todo lo que vive dentro de `Backend/`.

## Resumen

El backend real esta en:

```text
Backend/SillasTresCantos.Api
```

Es una API ASP.NET Core 8 con MySQL, autenticacion JWT y Swagger en desarrollo. La API sirve el catalogo publico y el portal interno de empleados.

## Intencion De Producto

El backend debe proteger la gestion interna sin romper la consulta publica del catalogo.

Reglas importantes:

- Los GET publicos de productos, categorias, marcas y archivos deben funcionar sin token.
- El contenido oculto solo debe devolverse con `includeHidden=true` y usuario autenticado.
- Crear, editar o borrar catalogo requiere JWT.
- Gestionar usuarios queda restringido a `SuperAdmin`.
- Mantener mensajes de error utiles para frontend, pero sin filtrar detalles internos.
- No introducir credenciales nuevas ni secretos reales en codigo fuente.

## Arquitectura

Punto de entrada:

```text
SillasTresCantos.Api/Program.cs
```

Capas principales:

```text
SillasTresCantos.Api/Controllers/
SillasTresCantos.Api/Services/
SillasTresCantos.Api/Data/
SillasTresCantos.Api/DTOs/
SillasTresCantos.Api/Models/
SillasTresCantos.Api/Configuration/
SillasTresCantos.Api/Services/Storage/
```

Flujo habitual:

```text
Controller -> Service -> Repository -> MySQL
```

Los controladores traducen HTTP y autorizacion. Los servicios validan reglas de negocio y devuelven `*OperationResult`. Los repositorios usan `MySqlCommand` parametrizado y modelos de dominio.

## Entidades Y Rutas

Rutas principales:

```text
api/auth
api/productos
api/categorias
api/marcas
api/productos/{productoId}/archivos
api/integraciones
api/usuarios
```

Entidades principales:

```text
Producto
Categoria
Marca
ProductoArchivo
Usuario
```

Visibilidad publica:

- `productos.es_visible`
- `categorias.es_visible`
- `marcas.es_visible`

El frontend publico consulta por defecto con `includeHidden=false`.

## Configuracion

Archivos clave:

```text
SillasTresCantos.Api/appsettings.json
SillasTresCantos.Api/appsettings.Development.json
SillasTresCantos.Api/Properties/launchSettings.json
docker-compose.yml
docker/mysql/init/01_init_schema.sql
```

Conexiones locales por defecto:

```text
API Docker: http://localhost:8311
API dotnet run: http://localhost:5127
MySQL host: localhost:1138
Swagger Docker: http://localhost:8311/swagger/index.html
Swagger local: http://localhost:5127/swagger/index.html
```

Variables sensibles:

- `ConnectionStrings__DefaultConnection` puede venir de Docker Compose o entorno.
- `CLOUDINARY_URL` se carga desde entorno o `.env` mediante `dotenv.net`.
- La clave JWT de desarrollo existe solo para entorno local; no usarla como secreto real.

## Almacenamiento

Imagenes de producto:

```text
CloudinaryProductoImagenStorageService
IProductoImagenStorageService
```

Archivos/documentacion de producto:

```text
LocalProductoArchivoStorageService
IProductoArchivoStorageService
```

Reglas actuales:

- Imagenes soportadas: JPG, PNG y WEBP.
- Documentacion soportada: JPG, PNG, WEBP, PDF, DOC y DOCX.
- Tamano maximo por defecto: `5242880` bytes.
- Los archivos locales se guardan bajo `FileStorage:RootPath`.

## Base De Datos

El esquema inicial vive en:

```text
docker/mysql/init/01_init_schema.sql
```

Si cambias estructura SQL, actualiza este archivo y documenta si hace falta recrear volumen:

```bash
docker compose down -v
docker compose up -d --build
```

Evita SQL construido con datos de usuario. Sigue usando parametros (`AddWithValue` o equivalente) en repositorios.

## Comandos

Desde la raiz del repositorio:

```bash
dotnet build SillasTresCantos.sln
docker compose up -d --build
docker compose ps
docker compose logs -f api
docker compose logs -f mysql
```

Ejecutar API local contra MySQL dockerizada:

```bash
dotnet run --project Backend/SillasTresCantos.Api/SillasTresCantos.Api.csproj --launch-profile http
```

Validacion de permisos de backoffice:

```bash
powershell -ExecutionPolicy Bypass -File ./scripts/validar-auth-backoffice.ps1
```

## Reglas De Desarrollo

- Mantener `Nullable` habilitado y respetar tipos anulables.
- Validar entradas en servicios antes de llamar al repositorio.
- Mapear DTOs en la capa de servicio salvo que el patron local indique otra cosa.
- Preferir `CancellationToken` en operaciones de IO, subida/descarga y BBDD nuevas.
- Mantener respuestas publicas compatibles con el frontend Angular.
- No devolver productos, categorias o marcas ocultas en endpoints publicos por accidente.
- Si agregas endpoints protegidos, registra el requisito con `[Authorize]` y comprueba roles cuando aplique.

## Validacion Recomendada

Antes de entregar cambios de backend:

```bash
dotnet build SillasTresCantos.sln
```

Para comprobar API completa:

```bash
docker compose up -d --build
```

Abrir Swagger:

```text
http://localhost:8311/swagger/index.html
```

