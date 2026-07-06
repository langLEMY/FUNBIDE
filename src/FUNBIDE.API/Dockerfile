FROM node:22-alpine AS frontend-build
WORKDIR /frontend

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ ./
RUN npm run build

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

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# postgresql-client aporta pg_dump, requerido por DatabaseBackupHostedService.
RUN apk add --no-cache postgresql-client icu-libs \
    && addgroup -S funbide && adduser -S funbide -G funbide

ENV DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
COPY --from=frontend-build /frontend/dist ./wwwroot
RUN mkdir -p /var/backups/funbide && chown -R funbide:funbide /app /var/backups/funbide

USER funbide
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "FUNBIDE.API.dll"]
