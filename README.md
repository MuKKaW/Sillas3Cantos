# SillasTresCantos

## Requisitos
- Docker Desktop en ejecucion
- .NET 8 SDK

## Puertos configurados
- API (contenedor): `8311`
- MySQL (host -> contenedor): `1138 -> 3306`

## Levantar API y BBDD con Docker Compose
```powershell
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
