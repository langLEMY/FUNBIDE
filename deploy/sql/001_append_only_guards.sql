-- Defensa en profundidad: aunque la API ya bloquea PUT/PATCH/DELETE sobre
-- historial_clinico y auditoria_logs en AppendOnlyGuardMiddleware, esta migración
-- hace la regla irrompible también para accesos directos a la base de datos
-- (consola de Supabase, otro servicio, un error de configuración de la API).

CREATE SCHEMA IF NOT EXISTS funbide;

-- SET search_path = '' evita que la función quede vulnerable a "search path
-- hijacking" (un objeto malicioso en un esquema anterior en el search_path
-- del rol que la ejecuta) — hallazgo del linter de seguridad de Supabase.
CREATE OR REPLACE FUNCTION funbide.bloquear_modificacion_append_only()
RETURNS TRIGGER AS $$
BEGIN
    RAISE EXCEPTION
        'La tabla %.% es append-only: no se permiten operaciones UPDATE ni DELETE.',
        TG_TABLE_SCHEMA, TG_TABLE_NAME
        USING ERRCODE = '42501'; -- insufficient_privilege
END;
$$ LANGUAGE plpgsql SET search_path = '';

-- Se crean como DO blocks porque las tablas destino las crea EF Core Migrations;
-- este script debe ejecutarse DESPUÉS de la primera migración (ver README de deploy/sql).
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables
               WHERE table_schema = 'funbide' AND table_name = 'historial_clinico') THEN
        DROP TRIGGER IF EXISTS trg_historial_clinico_append_only ON funbide.historial_clinico;
        CREATE TRIGGER trg_historial_clinico_append_only
            BEFORE UPDATE OR DELETE ON funbide.historial_clinico
            FOR EACH ROW EXECUTE FUNCTION funbide.bloquear_modificacion_append_only();
    END IF;

    IF EXISTS (SELECT 1 FROM information_schema.tables
               WHERE table_schema = 'funbide' AND table_name = 'auditoria_logs') THEN
        DROP TRIGGER IF EXISTS trg_auditoria_logs_append_only ON funbide.auditoria_logs;
        CREATE TRIGGER trg_auditoria_logs_append_only
            BEFORE UPDATE OR DELETE ON funbide.auditoria_logs
            FOR EACH ROW EXECUTE FUNCTION funbide.bloquear_modificacion_append_only();

        ALTER TABLE funbide.auditoria_logs ALTER COLUMN "Id" SET DEFAULT gen_random_uuid();
    END IF;

    -- Cobro, MovimientoFinanciero y Donacion ya se documentan como append-only en el
    -- Dominio (ver AppendOnlyEntity y los comentarios de cada entidad), pero hasta ahora
    -- esa regla solo se aplicaba en la API — un acceso directo a la base la saltaba.
    IF EXISTS (SELECT 1 FROM information_schema.tables
               WHERE table_schema = 'funbide' AND table_name = 'cobros') THEN
        DROP TRIGGER IF EXISTS trg_cobros_append_only ON funbide.cobros;
        CREATE TRIGGER trg_cobros_append_only
            BEFORE UPDATE OR DELETE ON funbide.cobros
            FOR EACH ROW EXECUTE FUNCTION funbide.bloquear_modificacion_append_only();
    END IF;

    IF EXISTS (SELECT 1 FROM information_schema.tables
               WHERE table_schema = 'funbide' AND table_name = 'movimientos_financieros') THEN
        DROP TRIGGER IF EXISTS trg_movimientos_financieros_append_only ON funbide.movimientos_financieros;
        CREATE TRIGGER trg_movimientos_financieros_append_only
            BEFORE UPDATE OR DELETE ON funbide.movimientos_financieros
            FOR EACH ROW EXECUTE FUNCTION funbide.bloquear_modificacion_append_only();
    END IF;

    IF EXISTS (SELECT 1 FROM information_schema.tables
               WHERE table_schema = 'funbide' AND table_name = 'donaciones') THEN
        DROP TRIGGER IF EXISTS trg_donaciones_append_only ON funbide.donaciones;
        CREATE TRIGGER trg_donaciones_append_only
            BEFORE UPDATE OR DELETE ON funbide.donaciones
            FOR EACH ROW EXECUTE FUNCTION funbide.bloquear_modificacion_append_only();
    END IF;
END $$;
