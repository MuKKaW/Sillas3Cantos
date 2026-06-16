# Frontend Context

Este documento da contexto operativo para trabajar en el frontend de ORTOPEDIA PUEBLOS 3C. Afecta a todo lo que vive dentro de `Frontend/`.

## Resumen

El frontend real esta en:

```text
Frontend/sillas-tres-cantos-web
```

Es una aplicacion Angular standalone. La parte publica debe sentirse como una web entregable a cliente, no como una demo de clase.

La web tiene tres rutas principales:

```text
/            Landing publica + catalogo visible
/login       Portal empleados
/backoffice  Portal interno protegido por login
```

## Intencion De Producto

La home es una landing limpia para una ortopedia/movilidad en Tres Cantos. No debe mostrar navegacion tecnica ni accesos evidentes al backoffice.

Reglas de presentacion publica:

- Header publico: solo `ORTOPEDIA PUEBLOS 3C`.
- No mostrar enlaces visibles tipo `Catalogo`, `Backoffice` o `Login` en la cabecera.
- No mostrar chips de sesion como `Invitado`.
- No mostrar credenciales demo ni botones para autorrellenar login.
- El acceso interno vive en el footer como enlace discreto: `Portal empleados`.
- Mantener enfoque mobile-first.
- Mantener la paleta en tonos azules, con fondo claro y azul navy para zonas oscuras.
- La landing usa una imagen local en `public/images/landing-showroom.png`.

Datos publicos actuales de contacto:

```text
Sector Pueblos 36
28760 Tres Cantos, Madrid
918 033 019
```

No usar el correo placeholder anterior `hola@sillas3cantos.es`.

## Arquitectura

Puntos de entrada:

```text
src/main.ts
src/app/app.config.ts
src/app/app.routes.ts
src/app/app.html
src/app/app.scss
src/styles.scss
```

Paginas:

```text
src/app/pages/public-catalog/
src/app/pages/login/
src/app/pages/backoffice/
```

Core:

```text
src/app/core/config/api.config.ts
src/app/core/guards/auth.guard.ts
src/app/core/interceptors/auth.interceptor.ts
src/app/core/models/api.models.ts
src/app/core/services/auth.service.ts
src/app/core/services/backend-api.service.ts
```

## Flujo Publico

`PublicCatalog` carga configuracion de catalogo, categorias, marcas y productos visibles usando `BackendApiService`.

La vista publica siempre consulta con `includeHidden: false`. Si la API no esta disponible, se muestra un mensaje amable orientado a cliente, no un texto tecnico.

La landing soporta dos modos de filtro:

- Filtro actual con inputs/selects.
- Filtro nuevo por tabs: primero categorias y, cuando hay categoria seleccionada, marcas. El buscador por nombre queda debajo de los tabs.

El modo activo lo decide `configuracionCatalogo.usarFiltroTabs`, expuesto por `api/configuracion/catalogo` y configurable desde backoffice.

Los archivos asociados a productos son internos. La vista publica no debe listar ni descargar documentacion de producto.

La landing contiene:

- Hero con imagen local.
- Banda de confianza: venta/alquiler, entrega local, soporte tecnico.
- Bloques de servicios.
- Catalogo filtrable.
- Detalle de producto sin archivos internos.
- Contacto.
- Legal basico.

## Flujo De Login

`Login` usa `AuthService.login()` y redirige a `/backoffice` cuando la autenticacion es correcta.

No debe incluir:

- Credenciales demo.
- Texto de clase o laboratorio.
- Botones para usar usuarios de prueba.

Debe incluir:

- Titulo `Portal empleados` o equivalente.
- Enlace para volver a la web publica.

## Flujo Interno

`Backoffice` esta protegido por `authGuard`.

El `authInterceptor` añade `Authorization: Bearer <token>` cuando existe token.

`AuthService` guarda en `localStorage`:

```text
s3c_front_token
s3c_front_role
s3c_front_expires
```

El portal interno gestiona:

- Productos.
- Categorias.
- Marcas.
- Archivos internos de producto, visibles solo para empleados.
- Administracion global del filtro de la landing, disponible para empleados autenticados.
- Administracion de visibilidad publica de precios y stock, sin ocultarlos en backoffice.
- Orden visual de categorias y marcas mediante arrastrar y soltar en backoffice.
- Usuarios, solo para `SuperAdmin`.

La edicion de producto se abre en modal desde el boton `Editar`. Ese modal gestiona datos, cambio de imagen y archivos asociados. Mientras el modal esta abierto, el scroll de fondo debe quedar bloqueado.

El logout esta dentro del backoffice porque el header publico no muestra estado de sesion.

## API Y Proxy

La base de API del frontend es:

```ts
export const API_BASE_URL = '/api';
```

El proxy de desarrollo esta en:

```text
proxy.conf.json
```

Actualmente apunta `/api` a:

```text
http://localhost:8311
```

## Comandos

Desde la raiz del repositorio:

```bash
npm ci
npm start
npm run build
```

Desde la app Angular:

```bash
cd Frontend/sillas-tres-cantos-web
npm start
npm run build
```

Notas:

- `npm start` sirve Angular con `proxy.conf.json`.
- Si `4200` esta ocupado, Angular puede pedir usar otro puerto.
- Validacion minima antes de cerrar cambios: `npm run build`.

## Estilos

Estilos globales:

```text
src/styles.scss
```

El tema define variables CSS para:

- Fondos claros azulados.
- Texto navy.
- Bordes azul claro.
- Acentos azules.

Evitar volver a una paleta verde/teal como color principal. La imagen del hero puede tener matices naturales, pero el overlay, CTA, links y fondo deben leerse azules.

Reglas visuales:

- Mobile-first.
- Cards con radio de `8px`.
- Nada de UI de demo en la landing.
- No anidar cards dentro de cards.
- Mantener texto legible en movil.
- Footer oscuro navy con `Portal empleados` discreto.

## Assets

Assets publicos:

```text
public/
public/images/landing-showroom.png
```

Si se reemplaza el hero, guardar el asset dentro de `public/images/` y actualizar referencias en:

```text
src/app/pages/public-catalog/public-catalog.html
src/app/pages/login/login.scss
```

## Validacion Recomendada

Antes de entregar cambios de frontend:

```bash
npm run build
```

Para revisar visualmente:

```bash
npm start
```

Abrir:

```text
http://localhost:4200/
http://localhost:4200/login
http://localhost:4200/backoffice
```

Comprobar especialmente:

- La home no muestra `Backoffice`, `Login`, `Invitado` ni credenciales demo.
- El footer contiene `Portal empleados`.
- El contacto usa `Sector Pueblos 36`, `28760 Tres Cantos, Madrid` y `918 033 019`.
- La paleta principal es azul.
- El catalogo respeta el modo configurado: filtro actual o filtro nuevo por tabs.
- La ficha publica de producto no muestra archivos internos ni descargas de documentacion.
- En backoffice, editar producto abre modal con cambio de imagen y gestion de archivos internos.
