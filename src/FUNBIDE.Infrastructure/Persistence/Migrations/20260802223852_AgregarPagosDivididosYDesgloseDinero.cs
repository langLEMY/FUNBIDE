using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUNBIDE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPagosDivididosYDesgloseDinero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DineroEfectivo",
                schema: "funbide",
                table: "resumenes_diarios",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DineroTarjeta",
                schema: "funbide",
                table: "resumenes_diarios",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DineroTransferencia",
                schema: "funbide",
                table: "resumenes_diarios",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "cobros_pagos",
                schema: "funbide",
                columns: table => new
                {
                    Metodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CobroId = table.Column<Guid>(type: "uuid", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cobros_pagos", x => new { x.CobroId, x.Metodo });
                    table.CheckConstraint("ck_pago_recibido_monto_positivo", "\"Monto\" > 0");
                    table.ForeignKey(
                        name: "FK_cobros_pagos_cobros_CobroId",
                        column: x => x.CobroId,
                        principalSchema: "funbide",
                        principalTable: "cobros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Backfill: cada cobro ya registrado tenía exactamente un método de pago
            // (columna MetodoPago, a punto de eliminarse) — se traduce a una única línea
            // en cobros_pagos antes de borrar esa columna, para no perder el desglose de
            // los cobros históricos. Los cobros con MontoPagado = 0 (nada pagado
            // todavía, todo a deuda) no generan línea: cobros_pagos exige Monto > 0,
            // igual que el resto de la lógica de dominio.
            migrationBuilder.Sql(
                """
                INSERT INTO funbide.cobros_pagos ("CobroId", "Metodo", "Monto")
                SELECT "Id", "MetodoPago", "MontoPagado"
                FROM funbide.cobros
                WHERE "MontoPagado" > 0;
                """);

            migrationBuilder.DropColumn(
                name: "MetodoPago",
                schema: "funbide",
                table: "cobros");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cobros_pagos",
                schema: "funbide");

            migrationBuilder.DropColumn(
                name: "DineroEfectivo",
                schema: "funbide",
                table: "resumenes_diarios");

            migrationBuilder.DropColumn(
                name: "DineroTarjeta",
                schema: "funbide",
                table: "resumenes_diarios");

            migrationBuilder.DropColumn(
                name: "DineroTransferencia",
                schema: "funbide",
                table: "resumenes_diarios");

            migrationBuilder.AddColumn<string>(
                name: "MetodoPago",
                schema: "funbide",
                table: "cobros",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
