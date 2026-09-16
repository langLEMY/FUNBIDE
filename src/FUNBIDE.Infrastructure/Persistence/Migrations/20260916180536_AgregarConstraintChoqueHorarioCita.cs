using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUNBIDE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarConstraintChoqueHorarioCita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EXCLUDE USING gist no es DDL que EF Core sepa expresar con la API fluida
            // (no existe equivalente a HasIndex().IsUnique() para esto) -- Sql crudo.
            // btree_gist habilita comparar el DoctorId (uuid) con "=" dentro de la misma
            // constraint de exclusión que compara el rango de tiempo con "&&"; sin la
            // extensión, Postgres no sabe cómo indexar uuid con GiST.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            // Antes esto SOLO se validaba en AgendarCitaUseCase/ProgramarCitaUseCase
            // (TieneChoqueDeHorarioAsync) -- una condición de carrera clásica de
            // "verificar-luego-insertar": dos peticiones simultáneas para el mismo doctor
            // y la misma franja podían pasar ambas la validación antes de que cualquiera
            // insertara. Esta constraint es la última línea de defensa real, a nivel de
            // base de datos. Se verificó contra producción antes de aplicarla: no hay
            // ninguna fila que la viole hoy. El WHERE la limita a citas con horario
            // asignado (Pendiente todavía no tiene Intervalo) y en un estado "activo"
            // (Completada/Cancelada no compiten por la franja). ExceptionHandlingMiddleware
            // ya traduce cualquier violación de constraint única a un 409 -- mismo patrón
            // que el índice único de ClaveIdempotencia en Cobros.
            migrationBuilder.Sql(
                """
                ALTER TABLE funbide.citas
                ADD CONSTRAINT ck_cita_sin_choque_horario
                EXCLUDE USING gist (
                    "DoctorId" WITH =,
                    tstzrange(inicio, fin, '[)') WITH &&
                )
                WHERE (inicio IS NOT NULL AND "Estado" IN ('Programada', 'EnEspera'));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE funbide.citas DROP CONSTRAINT IF EXISTS ck_cita_sin_choque_horario;");
            // La extensión btree_gist se deja instalada a propósito: quitarla podría
            // romper otra constraint/índice que la use más adelante, y no cuesta nada
            // tenerla habilitada de más.
        }
    }
}
