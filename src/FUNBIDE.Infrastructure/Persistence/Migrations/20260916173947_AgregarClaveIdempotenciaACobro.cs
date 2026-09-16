using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUNBIDE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarClaveIdempotenciaACobro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClaveIdempotencia",
                schema: "funbide",
                table: "cobros",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_cobros_ClaveIdempotencia",
                schema: "funbide",
                table: "cobros",
                column: "ClaveIdempotencia",
                unique: true,
                filter: "\"ClaveIdempotencia\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cobros_ClaveIdempotencia",
                schema: "funbide",
                table: "cobros");

            migrationBuilder.DropColumn(
                name: "ClaveIdempotencia",
                schema: "funbide",
                table: "cobros");
        }
    }
}
