using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUNBIDE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPermisosUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "permisos_usuario",
                schema: "funbide",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Modulo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Concedido = table.Column<bool>(type: "boolean", nullable: false),
                    ActualizadoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActualizadoPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permisos_usuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_permisos_usuario_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "funbide",
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_permisos_usuario_UsuarioId_Modulo",
                schema: "funbide",
                table: "permisos_usuario",
                columns: new[] { "UsuarioId", "Modulo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "permisos_usuario",
                schema: "funbide");
        }
    }
}
