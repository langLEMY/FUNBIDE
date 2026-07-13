using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUNBIDE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InicialFunbide : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "funbide");

            migrationBuilder.CreateTable(
                name: "auditoria_logs",
                schema: "funbide",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    Accion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Recurso = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CodigoRespuestaHttp = table.Column<int>(type: "integer", nullable: true),
                    detalle = table.Column<string>(type: "jsonb", nullable: false),
                    RegistradoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoria_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "citas",
                schema: "funbide",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PacienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    inicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    fin = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NotasCierre = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_citas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "historial_clinico",
                schema: "funbide",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PacienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CitaId = table.Column<Guid>(type: "uuid", nullable: true),
                    contenido = table.Column<string>(type: "jsonb", nullable: false),
                    RegistradoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historial_clinico", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "inventario_items",
                schema: "funbide",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StockActual = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventario_items", x => x.Id);
                    table.CheckConstraint("ck_inventario_stock_no_negativo", "\"StockActual\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "movimientos_financieros",
                schema: "funbide",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Concepto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CitaId = table.Column<Guid>(type: "uuid", nullable: true),
                    RegistradoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos_financieros", x => x.Id);
                    table.CheckConstraint("ck_movimiento_financiero_monto_positivo", "\"Monto\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "pacientes",
                schema: "funbide",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento_identidad = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pacientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "resumenes_diarios",
                schema: "funbide",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    PacientesAtendidos = table.Column<int>(type: "integer", nullable: false),
                    DineroMovido = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resumenes_diarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                schema: "funbide",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupabaseUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Correo = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FotoPerfilUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "movimientos_inventario",
                schema: "funbide",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InventarioItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    StockResultante = table.Column<int>(type: "integer", nullable: false),
                    Referencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RegistradoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientos_inventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_movimientos_inventario_inventario_items_InventarioItemId",
                        column: x => x.InventarioItemId,
                        principalSchema: "funbide",
                        principalTable: "inventario_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_logs_Recurso",
                schema: "funbide",
                table: "auditoria_logs",
                column: "Recurso");

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_logs_RegistradoEn",
                schema: "funbide",
                table: "auditoria_logs",
                column: "RegistradoEn");

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_logs_detalle",
                schema: "funbide",
                table: "auditoria_logs",
                column: "detalle")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_citas_DoctorId_Estado",
                schema: "funbide",
                table: "citas",
                columns: new[] { "DoctorId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_historial_clinico_PacienteId",
                schema: "funbide",
                table: "historial_clinico",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_historial_clinico_RegistradoEn",
                schema: "funbide",
                table: "historial_clinico",
                column: "RegistradoEn");

            migrationBuilder.CreateIndex(
                name: "IX_historial_clinico_contenido",
                schema: "funbide",
                table: "historial_clinico",
                column: "contenido")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_inventario_items_Codigo",
                schema: "funbide",
                table: "inventario_items",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_financieros_RegistradoEn",
                schema: "funbide",
                table: "movimientos_financieros",
                column: "RegistradoEn");

            migrationBuilder.CreateIndex(
                name: "IX_movimientos_inventario_InventarioItemId",
                schema: "funbide",
                table: "movimientos_inventario",
                column: "InventarioItemId");

            migrationBuilder.CreateIndex(
                name: "IX_pacientes_documento_identidad",
                schema: "funbide",
                table: "pacientes",
                column: "documento_identidad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_resumenes_diarios_Fecha",
                schema: "funbide",
                table: "resumenes_diarios",
                column: "Fecha",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Correo",
                schema: "funbide",
                table: "usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_SupabaseUserId",
                schema: "funbide",
                table: "usuarios",
                column: "SupabaseUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auditoria_logs",
                schema: "funbide");

            migrationBuilder.DropTable(
                name: "citas",
                schema: "funbide");

            migrationBuilder.DropTable(
                name: "historial_clinico",
                schema: "funbide");

            migrationBuilder.DropTable(
                name: "movimientos_financieros",
                schema: "funbide");

            migrationBuilder.DropTable(
                name: "movimientos_inventario",
                schema: "funbide");

            migrationBuilder.DropTable(
                name: "pacientes",
                schema: "funbide");

            migrationBuilder.DropTable(
                name: "resumenes_diarios",
                schema: "funbide");

            migrationBuilder.DropTable(
                name: "usuarios",
                schema: "funbide");

            migrationBuilder.DropTable(
                name: "inventario_items",
                schema: "funbide");
        }
    }
}
