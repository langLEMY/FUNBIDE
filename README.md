# FUNBIDE

Panel de administración de la Fundación Bienestar y Desarrollo: gestión de pacientes, citas, historial clínico, inventario de farmacia, finanzas y personal.

## Stack

- **Backend**: .NET 9 (Clean Architecture: `FUNBIDE.Domain`, `FUNBIDE.Application`, `FUNBIDE.Infrastructure`, `FUNBIDE.API`), Entity Framework Core, Postgres.
- **Frontend**: React 19 + TypeScript + Vite, React Router.
- **Auth y base de datos**: Supabase (Postgres + Auth). El login ocurre directo desde el navegador contra Supabase Auth; el backend valida el JWT resultante en cada petición.
- **Despliegue**: Docker (imagen única que sirve la API y el SPA compilado) + Nginx como reverse proxy/TLS.

### Roles

| Rol | Qué hace |
|---|---|
| `Admin` | Ve el dashboard, el resumen financiero y el directorio de personal. Solo lectura sobre inventario/pacientes desde el punto de vista de gestión de personal. |
| `Doctor` | Pacientes, citas (crear/programar/completar) e historial clínico. |
| `Fondos` | Registra y consulta movimientos financieros (ingresos/egresos). |
| `Farmacia` | Administra inventario (agregar ítems, despachar). |
| `Lemy` | Administra personal (altas/bajas, roles, contraseñas) y tiene la bitácora de actividad (`/actividad`) con filtros por login, cambios, inventario y pacientes. |

Pacientes e inventario están abiertos a los cinco roles para agregar/consultar; eliminar pacientes es exclusivo de `Admin`, `Lemy` y `Doctor`.

## Requisitos previos

- .NET 9 SDK
- Node.js 22+
- Una cuenta/proyecto de Supabase (Postgres + Auth)
- Docker + Docker Compose (solo para despliegue tipo producción)

## Variables de entorno

Hay dos plantillas de variables, cada una junto a su `.env.example`:

- **Raíz (`.env.example`)**: usada por `docker-compose.yml` — cadena de conexión a Postgres, URL/clave de servicio de Supabase, clave AES para los backups. Cópiala a `.env` y completa los valores reales.
- **`frontend/.env.example`**: `VITE_SUPABASE_URL`, `VITE_SUPABASE_ANON_KEY`, `VITE_API_BASE_URL`. Cópiala a `frontend/.env`.

Para desarrollo local sin Docker, el backend lee `src/FUNBIDE.API/appsettings.Development.json` / `appsettings.json` — reemplaza ahí los valores `CHANGE_ME` (o mejor, usa `dotnet user-secrets` para no dejarlos en el archivo).

**Nunca comitees `.env` ni secretos reales** — solo los `.env.example`.

## Cómo correr en local

### Opción 1: Docker (más parecido a producción)

```
docker compose up --build
```

Esto compila el frontend (stage `frontend-build` del `Dockerfile`) y lo empaqueta junto con la API en un solo contenedor, detrás de Nginx. Necesita `.env` en la raíz con las variables reales.

### Opción 2: desarrollo día a día

```
# Backend (desde la raíz)
dotnet run --project src/FUNBIDE.API

# Frontend (en otra terminal)
cd frontend
npm install
npm run dev
```

El frontend en modo `dev` apunta a `VITE_API_BASE_URL` (definido en `frontend/.env`) para hablar con el backend.

## Tests

```
# Backend
dotnet test FUNBIDE.sln

# Frontend
cd frontend
npm test
```

La cobertura hoy es un punto de partida (invariantes de dominio más críticas y un par de casos de uso/componentes), no exhaustiva — conviene seguir ampliándola.

## CI

`.github/workflows/ci.yml` corre build+test de backend y frontend en cada push/PR a `main`. No hay despliegue automático configurado — es una decisión pendiente que requiere secretos de producción.

## Notas de despliegue

- El `Dockerfile` de `FUNBIDE.API` ahora incluye un stage de Node que construye el frontend y copia el resultado a `wwwroot`; ya no depende de una copia manual.
- `docker-compose.yml` define un volumen `certbot_webroot` pero **no** incluye un servicio de certbot — hay que provisionar los certificados TLS de `deploy/nginx/ssl/` por fuera (certbot manual, u otro mecanismo) antes de exponer Nginx en producción.
- Verificar antes de lanzar: `appsettings.json`'s `Cors:OrigenesPermitidos` lista `https://funbide.org` y `https://app.funbide.org`, pero `deploy/nginx/nginx.conf` solo tiene `server_name api.funbide.org` y sirve el SPA desde el mismo origen que la API. Si el plan es alojar el frontend en un dominio separado, falta configurar ese origen en Nginx; si no, esos orígenes CORS son innecesarios y se pueden limpiar.
- Los backups automáticos (`DatabaseBackupHostedService`) usan `pg_dump` (incluido en la imagen) y cifran con AES — la clave (`Backup:AesKeyBase64` / `FUNBIDE_BACKUP_AES_KEY`) debe guardarse en un lugar seguro fuera del repo.
