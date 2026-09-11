using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUNBIDE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTipoAHistorialClinico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tipo",
                schema: "funbide",
                table: "historial_clinico",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "NotaClinica");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tipo",
                schema: "funbide",
                table: "historial_clinico");
        }
    }
}
