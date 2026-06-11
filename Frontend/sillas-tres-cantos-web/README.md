# Frontend Sillas Tres Cantos (Angular)

Aplicacion SPA en Angular conectada a la API de `SillasTresCantos.Api`.

## Requisitos

- Node 24+
- Backend levantado en `http://localhost:8311`

## Arranque en desarrollo

Desde esta carpeta:

```bash
npm install
npm start
```

`npm start` usa `proxy.conf.json`, por lo que el frontend llama a `/api` y Angular lo redirige al backend en `http://localhost:8311`.

Abrir:
- `http://localhost:4200`

## Build

```bash
npm run build
```

## Funcionalidades incluidas

- Zona publica:
  - listado y filtro de productos/categorias/marcas
  - detalle de producto y descarga de archivos
  - busqueda de productos externos (`/api/integraciones/productos-externos`)
- Login JWT:
  - `POST /api/auth/login`
- Backoffice:
  - CRUD de productos, categorias y marcas
  - gestion de archivos de producto (subida/listado/descarga/borrado)
  - consultas de productos generales, `mios` y `por usuario`
  - CRUD de usuarios solo para `SuperAdmin`

## Credenciales demo

- `admin` / `Admin12345!`
- `user` / `User12345!`
