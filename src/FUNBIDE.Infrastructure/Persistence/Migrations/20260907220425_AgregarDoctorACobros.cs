using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUNBIDE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDoctorACobros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DoctorId",
                schema: "funbide",
                table: "cobros",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_cobros_DoctorId",
                schema: "funbide",
                table: "cobros",
                column: "DoctorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cobros_DoctorId",
                schema: "funbide",
                table: "cobros");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                schema: "funbide",
                table: "cobros");
        }
    }
}
