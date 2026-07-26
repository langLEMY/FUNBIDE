using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUNBIDE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDonaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "donaciones",
                schema: "funbide",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DonanteNombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DonanteContacto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Monto = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Concepto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistradoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_donaciones", x => x.Id);
                    table.CheckConstraint("ck_donacion_monto_positivo", "\"Monto\" > 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_donaciones_RegistradoEn",
                schema: "funbide",
                table: "donaciones",
                column: "RegistradoEn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "donaciones",
                schema: "funbide");
        }
    }
}
