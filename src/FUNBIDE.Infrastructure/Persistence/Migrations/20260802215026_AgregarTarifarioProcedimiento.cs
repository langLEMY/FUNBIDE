using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUNBIDE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTarifarioProcedimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TarifarioProcedimientoId",
                schema: "funbide",
                table: "cobros",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tarifario_procedimientos",
                schema: "funbide",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeguroMedicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Plan = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Procedimiento = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    MontoSeguro = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    MontoPaciente = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarifario_procedimientos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tarifario_procedimientos_SeguroMedicoId_Plan_Procedimiento",
                schema: "funbide",
                table: "tarifario_procedimientos",
                columns: new[] { "SeguroMedicoId", "Plan", "Procedimiento" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tarifario_procedimientos",
                schema: "funbide");

            migrationBuilder.DropColumn(
                name: "TarifarioProcedimientoId",
                schema: "funbide",
                table: "cobros");
        }
    }
}
