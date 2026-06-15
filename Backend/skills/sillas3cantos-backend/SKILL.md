---
name: sillas3cantos-backend
description: Use when working on the ORTOPEDIA PUEBLOS 3C ASP.NET Core backend, especially API controllers, services, repositories, DTOs, auth/JWT, MySQL schema, Docker Compose, Swagger, storage, or backend documentation inside Backend/.
---

# ORTOPEDIA PUEBLOS 3C Backend

Use this skill for backend work under `Backend/`.

## First Step

Read `../../AGENTS.md` before editing. It contains the current API intent, routes, architecture, commands, security rules, storage notes, and validation checklist.

## Working Rules

- Keep public catalog reads available without login.
- Keep public catalog configuration reads available without login.
- Keep hidden products, categories, and brands behind `includeHidden=true` plus authenticated access.
- Keep product files/internal documentation behind JWT for list, download, upload, and delete.
- Require JWT for catalog mutations and protected backoffice flows.
- Product updates may be JSON-only or `multipart/form-data` when replacing the product image.
- Keep `GET api/configuracion/catalogo` public and `PUT api/configuracion/catalogo` authenticated.
- Keep user management restricted to `SuperAdmin`.
- Do not commit real secrets, production JWT keys, database passwords, or Cloudinary credentials.
- Keep SQL parameterized in repository classes.
- Validate domain rules in services and return the existing `*OperationResult` style.
- Preserve DTO compatibility with the Angular frontend unless the frontend is updated in the same change.
- Prefer clear Spanish client-facing error messages without exposing implementation details.

## Common Files

API startup and configuration:

```text
SillasTresCantos.Api/Program.cs
SillasTresCantos.Api/appsettings.json
SillasTresCantos.Api/appsettings.Development.json
SillasTresCantos.Api/Configuration/
```

HTTP surface:

```text
SillasTresCantos.Api/Controllers/
SillasTresCantos.Api/DTOs/
```

Business logic:

```text
SillasTresCantos.Api/Services/
SillasTresCantos.Api/Services/Storage/
```

Catalog configuration and internal files:

```text
SillasTresCantos.Api/Controllers/ConfiguracionCatalogoController.cs
SillasTresCantos.Api/Controllers/ProductoArchivosController.cs
SillasTresCantos.Api/Services/ConfiguracionCatalogoService.cs
SillasTresCantos.Api/Services/ProductoArchivoService.cs
```

Persistence:

```text
SillasTresCantos.Api/Data/
SillasTresCantos.Api/Models/
../docker/mysql/init/01_init_schema.sql
```

Containers and scripts:

```text
../docker-compose.yml
../scripts/validar-auth-backoffice.ps1
```

## API Patterns

Expected request flow:

```text
Controller -> Service -> Repository -> MySQL
```

Use controllers for:

- Routing and HTTP status mapping.
- `[Authorize]` and role restrictions.
- Extracting authenticated user claims.

Use services for:

- Input normalization and validation.
- Business rules.
- Mapping models to DTOs.
- Translating MySQL/storage failures into operation results.

Use repositories for:

- Parameterized SQL.
- Mapping DB rows into models.
- No HTTP or frontend-specific behavior.

## Validation

Run from repository root:

```bash
dotnet build SillasTresCantos.sln
```

For full local API checking:

```bash
docker compose up -d --build
```

Then inspect:

```text
http://localhost:8311/swagger/index.html
```

Auth/backoffice permission smoke test:

```bash
powershell -ExecutionPolicy Bypass -File ./scripts/validar-auth-backoffice.ps1
```
