using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FUNBIDE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposClinicosEInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Condicion",
                schema: "funbide",
                table: "pacientes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Edad",
                schema: "funbide",
                table: "pacientes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                schema: "funbide",
                table: "pacientes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Activo");

            migrationBuilder.AddColumn<DateOnly>(
                name: "UltimaVisita",
                schema: "funbide",
                table: "pacientes",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                schema: "funbide",
                table: "inventario_items",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StockMinimo",
                schema: "funbide",
                table: "inventario_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                schema: "funbide",
                table: "inventario_items",
                columns: new[] { "Id", "Categoria", "Codigo", "Nombre", "StockActual", "StockMinimo" },
                values: new object[,]
                {
                    { new Guid("2f1a1a2a-0001-4a3e-8a9a-000000000001"), "Medicamento", "MED-001", "Acetaminofén 500mg", 320, 100 },
                    { new Guid("2f1a1a2a-0001-4a3e-8a9a-000000000002"), "Medicamento", "MED-002", "Ibuprofeno 400mg", 40, 80 },
                    { new Guid("2f1a1a2a-0001-4a3e-8a9a-000000000003"), "Medicamento", "MED-003", "Amoxicilina 500mg", 150, 60 },
                    { new Guid("2f1a1a2a-0001-4a3e-8a9a-000000000004"), "Medicamento", "MED-004", "Loratadina 10mg", 25, 30 },
                    { new Guid("2f1a1a2a-0001-4a3e-8a9a-000000000005"), "Insumo", "INS-001", "Guantes de nitrilo (caja)", 60, 20 },
                    { new Guid("2f1a1a2a-0001-4a3e-8a9a-000000000006"), "Insumo", "INS-002", "Jeringas 5ml", 15, 40 },
                    { new Guid("2f1a1a2a-0001-4a3e-8a9a-000000000007"), "Insumo", "INS-003", "Gasas estériles (paquete)", 90, 30 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "inventario_items",
                keyColumn: "Id",
                keyValue: new Guid("2f1a1a2a-0001-4a3e-8a9a-000000000001"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "inventario_items",
                keyColumn: "Id",
                keyValue: new Guid("2f1a1a2a-0001-4a3e-8a9a-000000000002"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "inventario_items",
                keyColumn: "Id",
                keyValue: new Guid("2f1a1a2a-0001-4a3e-8a9a-000000000003"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "inventario_items",
                keyColumn: "Id",
                keyValue: new Guid("2f1a1a2a-0001-4a3e-8a9a-000000000004"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "inventario_items",
                keyColumn: "Id",
                keyValue: new Guid("2f1a1a2a-0001-4a3e-8a9a-000000000005"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "inventario_items",
                keyColumn: "Id",
                keyValue: new Guid("2f1a1a2a-0001-4a3e-8a9a-000000000006"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "inventario_items",
                keyColumn: "Id",
                keyValue: new Guid("2f1a1a2a-0001-4a3e-8a9a-000000000007"));

            migrationBuilder.DropColumn(
                name: "Condicion",
                schema: "funbide",
                table: "pacientes");

            migrationBuilder.DropColumn(
                name: "Edad",
                schema: "funbide",
                table: "pacientes");

            migrationBuilder.DropColumn(
                name: "Estado",
                schema: "funbide",
                table: "pacientes");

            migrationBuilder.DropColumn(
                name: "UltimaVisita",
                schema: "funbide",
                table: "pacientes");

            migrationBuilder.DropColumn(
                name: "Categoria",
                schema: "funbide",
                table: "inventario_items");

            migrationBuilder.DropColumn(
                name: "StockMinimo",
                schema: "funbide",
                table: "inventario_items");
        }
    }
}
