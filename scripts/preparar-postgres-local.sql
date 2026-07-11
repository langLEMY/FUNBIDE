-- Prepara la base de datos de la instalacion offline/USB (Auth:Provider=Local).
-- Correr UNA VEZ en la PC destino, ya con PostgreSQL instalado, como superusuario:
--
--   psql -h localhost -U postgres -f scripts/preparar-postgres-local.sql
--
-- Reemplazar CHANGE_ME por la MISMA contrasena que despues va en
-- ConnectionStrings:FunbideDatabase de publish\appsettings.Local.json (ver
-- appsettings.Local.json.example). Es idempotente: se puede correr mas de
-- una vez sin romper nada si el usuario/base ya existen.

DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'funbide') THEN
        CREATE ROLE funbide WITH LOGIN PASSWORD 'CHANGE_ME';
    END IF;
END
$$;

SELECT 'CREATE DATABASE funbide OWNER funbide'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'funbide')
\gexec
