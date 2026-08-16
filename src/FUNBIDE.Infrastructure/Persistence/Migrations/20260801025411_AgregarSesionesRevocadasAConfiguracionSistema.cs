using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUNBIDE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSesionesRevocadasAConfiguracionSistema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SesionesRevocadasEn",
                schema: "funbide",
                table: "configuracion_sistema",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SesionesRevocadasEn",
                schema: "funbide",
                table: "configuracion_sistema");
        }
    }
}
