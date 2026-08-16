using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUNBIDE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNombreUsuarioAUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Nullable primero: la tabla "usuarios" ya tiene filas reales (personal
            // existente). Agregar la columna directo como NOT NULL con un default fijo
            // (p. ej. "") dejaría a todos esos usuarios con el mismo valor, violando el
            // índice único de abajo apenas hubiera dos o más. En vez de eso, se rellena
            // cada fila con un nombre de usuario derivado de su correo actual (parte
            // antes de la @, saneada a [a-z0-9._-]) y recién después se exige NOT NULL.
            migrationBuilder.AddColumn<string>(
                name: "NombreUsuario",
                schema: "funbide",
                table: "usuarios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE funbide.usuarios AS u
                SET "NombreUsuario" = sub.candidato
                FROM (
                    SELECT
                        "Id",
                        CASE WHEN rn = 1 THEN base ELSE base || rn::text END AS candidato
                    FROM (
                        SELECT
                            "Id",
                            COALESCE(NULLIF(regexp_replace(lower(split_part("Correo", '@', 1)), '[^a-z0-9._-]', '', 'g'), ''), 'usuario') AS base,
                            ROW_NUMBER() OVER (
                                PARTITION BY COALESCE(NULLIF(regexp_replace(lower(split_part("Correo", '@', 1)), '[^a-z0-9._-]', '', 'g'), ''), 'usuario')
                                ORDER BY "Id"
                            ) AS rn
                        FROM funbide.usuarios
                    ) t
                ) sub
                WHERE u."Id" = sub."Id";
                """);

            migrationBuilder.AlterColumn<string>(
                name: "NombreUsuario",
                schema: "funbide",
                table: "usuarios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_NombreUsuario",
                schema: "funbide",
                table: "usuarios",
                column: "NombreUsuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_usuarios_NombreUsuario",
                schema: "funbide",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "NombreUsuario",
                schema: "funbide",
                table: "usuarios");
        }
    }
}
