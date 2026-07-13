using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUNBIDE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RediseñarPacientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                schema: "funbide",
                table: "pacientes");

            migrationBuilder.DropColumn(
                name: "NombreCompleto",
                schema: "funbide",
                table: "pacientes");

            migrationBuilder.AddColumn<string>(
                name: "Apellido",
                schema: "funbide",
                table: "pacientes",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FotoCedulaPath",
                schema: "funbide",
                table: "pacientes",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                schema: "funbide",
                table: "pacientes",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                schema: "funbide",
                table: "pacientes",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_pacientes_Nombre_Apellido",
                schema: "funbide",
                table: "pacientes",
                columns: new[] { "Nombre", "Apellido" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_pacientes_Nombre_Apellido",
                schema: "funbide",
                table: "pacientes");

            migrationBuilder.DropColumn(
                name: "Apellido",
                schema: "funbide",
                table: "pacientes");

            migrationBuilder.DropColumn(
                name: "FotoCedulaPath",
                schema: "funbide",
                table: "pacientes");

            migrationBuilder.DropColumn(
                name: "Nombre",
                schema: "funbide",
                table: "pacientes");

            migrationBuilder.DropColumn(
                name: "Telefono",
                schema: "funbide",
                table: "pacientes");

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaNacimiento",
                schema: "funbide",
                table: "pacientes",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "NombreCompleto",
                schema: "funbide",
                table: "pacientes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
