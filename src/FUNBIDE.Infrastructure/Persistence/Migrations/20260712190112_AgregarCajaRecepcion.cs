using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUNBIDE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCajaRecepcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TurnoCajaId",
                schema: "funbide",
                table: "movimientos_financieros",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cobros",
                schema: "funbide",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PacienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    CitaId = table.Column<Guid>(type: "uuid", nullable: true),
                    TurnoCajaId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Concepto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    MontoTotal = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    SeguroMedicoId = table.Column<Guid>(type: "uuid", nullable: true),
                    PorcentajeCobertura = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    MontoCobertura = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    CodigoAutorizacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MetodoPago = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MontoPagado = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    RegistradoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cobros", x => x.Id);
                    table.CheckConstraint("ck_cobro_monto_pagado_no_negativo", "\"MontoPagado\" >= 0");
                    table.CheckConstraint("ck_cobro_monto_total_positivo", "\"MontoTotal\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "seguros_medicos",
                schema: "funbide",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PorcentajeCobertura = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seguros_medicos", x => x.Id);
                    table.CheckConstraint("ck_seguro_medico_porcentaje_cobertura_rango", "\"PorcentajeCobertura\" > 0 AND \"PorcentajeCobertura\" <= 100");
                });

            migrationBuilder.CreateTable(
                name: "turnos_caja",
                schema: "funbide",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioAperturaId = table.Column<Guid>(type: "uuid", nullable: false),
                    MontoInicial = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    AbiertoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UsuarioCierreId = table.Column<Guid>(type: "uuid", nullable: true),
                    MontoFinalContado = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    Notas = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CerradoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_turnos_caja", x => x.Id);
                    table.CheckConstraint("ck_turno_caja_monto_inicial_no_negativo", "\"MontoInicial\" >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_cobros_CitaId",
                schema: "funbide",
                table: "cobros",
                column: "CitaId");

            migrationBuilder.CreateIndex(
                name: "IX_cobros_PacienteId",
                schema: "funbide",
                table: "cobros",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_cobros_RegistradoEn",
                schema: "funbide",
                table: "cobros",
                column: "RegistradoEn");

            migrationBuilder.CreateIndex(
                name: "IX_cobros_TurnoCajaId",
                schema: "funbide",
                table: "cobros",
                column: "TurnoCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_seguros_medicos_Nombre",
                schema: "funbide",
                table: "seguros_medicos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_turnos_caja_Estado",
                schema: "funbide",
                table: "turnos_caja",
                column: "Estado",
                unique: true,
                filter: "\"Estado\" = 'Abierto'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cobros",
                schema: "funbide");

            migrationBuilder.DropTable(
                name: "seguros_medicos",
                schema: "funbide");

            migrationBuilder.DropTable(
                name: "turnos_caja",
                schema: "funbide");

            migrationBuilder.DropColumn(
                name: "TurnoCajaId",
                schema: "funbide",
                table: "movimientos_financieros");
        }
    }
}
