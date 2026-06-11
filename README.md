# SillasTresCantos

## Requisitos
- Docker Desktop en ejecucion
- .NET 8 SDK
- Node.js 20+ y npm

## Puertos configurados
- API (contenedor): `8311`
- MySQL (host -> contenedor): `1138 -> 3306`

## Frontend
Desde la raiz del repositorio:
```powershell
npm ci
npm start
```

Compilar frontend:
```powershell
npm run build
```

## Levantar API y BBDD con Docker Compose
```powershell
docker compose up -d --build
```

Si ya tenias una base creada y has actualizado el esquema SQL, recrea volumen:
```powershell
docker compose down -v
docker compose up -d --build
```

Comprobar estado:
```powershell
docker compose ps
```

## Probar Swagger
Abrir en el navegador:
- `http://localhost:8311/swagger/index.html`

## Ver logs
```powershell
docker compose logs -f mysql
docker compose logs -f api
```

## Parar contenedores
```powershell
docker compose down
```

Parar y eliminar tambien el volumen de datos:
```powershell
docker compose down -v
```

## Ejecutar la API en local contra MySQL dockerizada
Con la BBDD levantada en Docker:
```powershell
dotnet run --project Backend/SillasTresCantos.Api/SillasTresCantos.Api.csproj --launch-profile http
```

Swagger local:
- `http://localhost:5127/swagger/index.html`

## Credenciales demo (desarrollo)
- SuperAdmin: `admin` / `Admin12345!`
- User: `user` / `User12345!`

## Validacion de permisos de backoffice
Ejecutar:
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\validar-auth-backoffice.ps1
```

El script valida:
- GET publicos sin login.
- POST/PUT/DELETE protegidos en catalogo para usuarios autenticados.
- Gestion de usuarios restringida a `SuperAdmin`.

## Visibilidad en catalogo
- `productos`, `categorias` y `marcas` usan `esVisible`.
- Los GET publicos solo devuelven elementos visibles.
- Para consultar ocultos usa `includeHidden=true` con token JWT valido.
