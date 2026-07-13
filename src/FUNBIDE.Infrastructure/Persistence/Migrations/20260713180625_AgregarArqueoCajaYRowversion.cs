using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUNBIDE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarArqueoCajaYRowversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Diferencia",
                schema: "funbide",
                table: "turnos_caja",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoEsperado",
                schema: "funbide",
                table: "turnos_caja",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "funbide",
                table: "turnos_caja",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_financieros_TurnoCajaId",
                schema: "funbide",
                table: "movimientos_financieros",
                column: "TurnoCajaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_movimientos_financieros_TurnoCajaId",
                schema: "funbide",
                table: "movimientos_financieros");

            migrationBuilder.DropColumn(
                name: "Diferencia",
                schema: "funbide",
                table: "turnos_caja");

            migrationBuilder.DropColumn(
                name: "MontoEsperado",
                schema: "funbide",
                table: "turnos_caja");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "funbide",
                table: "turnos_caja");
        }
    }
}
