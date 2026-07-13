FROM node:22-alpine AS frontend-build
WORKDIR /frontend

# "offline" arma el frontend con VITE_AUTH_MODE=local (ver frontend/.env.offline y
# frontend/src/lib/supabaseClient.ts) para instalaciones con Auth:Provider=Local -
# sin esto el frontend queda compilado contra Supabase real sin importar el backend.
ARG FRONTEND_BUILD_SCRIPT=build

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ ./
RUN npm run ${FRONTEND_BUILD_SCRIPT}

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

COPY ["FUNBIDE.sln", "."]
COPY ["src/FUNBIDE.Domain/FUNBIDE.Domain.csproj", "src/FUNBIDE.Domain/"]
COPY ["src/FUNBIDE.Application/FUNBIDE.Application.csproj", "src/FUNBIDE.Application/"]
COPY ["src/FUNBIDE.Infrastructure/FUNBIDE.Infrastructure.csproj", "src/FUNBIDE.Infrastructure/"]
COPY ["src/FUNBIDE.API/FUNBIDE.API.csproj", "src/FUNBIDE.API/"]
RUN dotnet restore "src/FUNBIDE.API/FUNBIDE.API.csproj"

COPY . .
RUN dotnet publish "src/FUNBIDE.API/FUNBIDE.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Debian/glibc en vez de Alpine/musl: musl falla la resolucion DNS combinada
# IPv4+IPv6 que usa .NET por defecto contra hosts sin registro AAAA (como el
# pooler de Supabase, un ELB de AWS solo-IPv4), devolviendo "Name does not
# resolve" aunque el registro A exista. glibc no tiene ese bug.
# postgresql-client aporta pg_dump, requerido por DatabaseBackupHostedService.
RUN apt-get update \
    && apt-get install -y --no-install-recommends postgresql-client curl \
    && rm -rf /var/lib/apt/lists/* \
    && groupadd -r funbide && useradd -r -g funbide funbide

ENV DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
COPY --from=frontend-build /frontend/dist ./wwwroot
# almacenamiento-local solo se usa en Auth:Provider=Local (ver LocalDiskStorageService) —
# se crea igual en toda imagen para que un volumen montado ahí no quede owned por root
# (Docker crea el punto de montaje como root si el directorio no existe ya en la imagen).
RUN mkdir -p /var/backups/funbide /app/almacenamiento-local \
    && chown -R funbide:funbide /app /var/backups/funbide

USER funbide
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD curl --fail --silent http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "FUNBIDE.API.dll"]
