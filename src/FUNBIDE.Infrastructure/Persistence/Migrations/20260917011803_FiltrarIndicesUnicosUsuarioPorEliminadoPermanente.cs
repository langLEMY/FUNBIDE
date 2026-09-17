using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUNBIDE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FiltrarIndicesUnicosUsuarioPorEliminadoPermanente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_usuarios_Correo",
                schema: "funbide",
                table: "usuarios");

            migrationBuilder.DropIndex(
                name: "IX_usuarios_NombreUsuario",
                schema: "funbide",
                table: "usuarios");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Correo",
                schema: "funbide",
                table: "usuarios",
                column: "Correo",
                unique: true,
                filter: "NOT \"EliminadoPermanentemente\"");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_NombreUsuario",
                schema: "funbide",
                table: "usuarios",
                column: "NombreUsuario",
                unique: true,
                filter: "NOT \"EliminadoPermanentemente\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_usuarios_Correo",
                schema: "funbide",
                table: "usuarios");

            migrationBuilder.DropIndex(
                name: "IX_usuarios_NombreUsuario",
                schema: "funbide",
                table: "usuarios");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Correo",
                schema: "funbide",
                table: "usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_NombreUsuario",
                schema: "funbide",
                table: "usuarios",
                column: "NombreUsuario",
                unique: true);
        }
    }
}
