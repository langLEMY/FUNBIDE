using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUNBIDE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HacerOpcionalPorcentajeCoberturaSeguro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PorcentajeCobertura",
                schema: "funbide",
                table: "seguros_medicos",
                type: "numeric(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PorcentajeCobertura",
                schema: "funbide",
                table: "seguros_medicos",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldNullable: true);
        }
    }
}
