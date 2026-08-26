using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FUNBIDE.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFondoInternoServiciosYSeedRenacerAps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Especialidad",
                schema: "funbide",
                table: "tarifario_procedimientos",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoFondo",
                schema: "funbide",
                table: "tarifario_procedimientos",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoFondo",
                schema: "funbide",
                table: "cobros",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "servicios",
                schema: "funbide",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Precio1 = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Precio2 = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Precio3 = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Especialidad = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servicios", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "funbide",
                table: "seguros_medicos",
                columns: new[] { "Id", "Activo", "Nombre", "PorcentajeCobertura" },
                values: new object[,]
                {
                    { new Guid("8f3c7d20-0003-4a3e-8a9a-000000000001"), true, "Renacer", 83m },
                    { new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002"), true, "Aps", 88m }
                });

            migrationBuilder.InsertData(
                schema: "funbide",
                table: "servicios",
                columns: new[] { "Id", "Activo", "Codigo", "Especialidad", "Nombre", "Precio1", "Precio2", "Precio3" },
                values: new object[,]
                {
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000001"), true, "CUL001", null, "CULTIVO", 800.00m, 800.00m, 8000.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000002"), true, "EV-PRE001", null, "EVALUACION PRE-QUIRURGICA", 1500.00m, 1500.00m, 1500.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000003"), true, "EV-PSI001", "Psicologia", "EVALLUCION PSICOLOGICA", 1500.00m, 1500.00m, 1500.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000004"), true, "FAR001", null, "ENTREGA MEDICAMENTO", 200.00m, 200.00m, 500.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000005"), true, "FAR002", null, "GLICEMIA", 100.00m, 100.00m, 100.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000006"), true, "FAR003", null, "EXAMEN DE LA VISTA", 200.00m, 200.00m, 200.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000007"), true, "IMG001", null, "RADIOGRAFÍA INTRAORAL PERIAPICAL MILIMET", 150.00m, 150.00m, 150.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000008"), true, "IMG002", null, "RADIOGRAFÍA INTRAORAL PERIAPICAL MOLARES", 100.00m, 100.00m, 100.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000009"), true, "IMG003", null, "RADIOGRAFÍA PANORÁMICA DE MAXILAR SUPERI", 320.00m, 320.00m, 320.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000010"), true, "IMG004", null, "RADIOGRAFÍA PANORÁMICA DE MAXILAR SUPERI", 320.00m, 320.00m, 320.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000011"), true, "IMG005", null, "RADIOGRAFÍA INTRAORAL PERIAPICAL PREMOLA", 100.00m, 100.00m, 100.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000012"), true, "IMG006", "Odontologia", "RADIOGRAFÍA INTRAORAL CORONALES", 150.00m, 150.00m, 150.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000013"), true, "IMG007", "Odontologia", "RADIOGRAFÍA INTRAORAL CORONALES", 150.00m, 150.00m, 150.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000014"), true, "IMG008", null, "RADIOGRAFÍA INTRAORAL PERIAPICAL DIENTES", 100.00m, 100.00m, 100.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000015"), true, "IMG009", null, "RADIOGRAFÍA INTRAORAL PERIAPICAL DIENTES", 100.00m, 100.00m, 100.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000016"), true, "IMG010", null, "RADIOGRAFÍA INTRAORAL PERIAPICAL DIENTES", 100.00m, 100.00m, 100.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000017"), true, "IMG011", null, "RADIOGRAFÍA INTRAORAL PERIAPICAL DIENTES", 100.00m, 100.00m, 100.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000018"), true, "IMG012", null, "RADIOGRAFÍA INTRAORAL PERIAPICAL MILIMET", 100.00m, 100.00m, 100.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000019"), true, "IMG013", null, "RADIOGRAFÍA INTRAORAL PERIAPICAL MOLARES", 100.00m, 100.00m, 100.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000020"), true, "IMG014", null, "RADIOGRAFÍA INTRAORAL PERIAPICAL PREMOLA", 100.00m, 100.00m, 100.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000021"), true, "IMG015", null, "RADIOGRAFÍA INTRAORAL PERIAPICAL ZONA DE", 100.00m, 100.00m, 100.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000022"), true, "IMG016", null, "RADIOGRAFÍA INTRAORAL PERIAPICAL ZONA DE", 100.00m, 100.00m, 100.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000023"), true, "IMG017", "Sonografia", "SONOGRAFÍA DE TIROIDE", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000024"), true, "IMG018", "Sonografia", "SONOGRAFÍA ABDOMINAL", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000025"), true, "IMG019", "Sonografia", "SONOGRAFÍA PÉLVICA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000026"), true, "IMG020", "Sonografia", "SONOGRAFÍA TRANSVAGINAL", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000027"), true, "IMG021", "Sonografia", "SONOGRAFÍA OBSTÉTRICA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000028"), true, "IMG022", "Sonografia", "SONOGRAFÍA ESCROTAL", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000029"), true, "IMG023", "Sonografia", "SONOGRAFÍA PROSTÁTICA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000030"), true, "IMG024", "Sonografia", "SONOGRAFÍA DE PARTES BLANDAS", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000031"), true, "IMG025", null, "ESTUDIO COLORACIÓN BÁSICA EN CITOLOGÍA", 700.00m, 700.00m, 700.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000032"), true, "IMG026", "Sonografia", "SONOGRAFÍA MAMARIAS", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000033"), true, "LAB001", null, "HEMOGRAMA", 200.00m, 200.00m, 200.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000034"), true, "LAB010", null, "HDL", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000035"), true, "LAB011", null, "LDL", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000036"), true, "LAB012", null, "COLESTEROL TOTAL", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000037"), true, "LAB013", null, "COPROLOGICO", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000038"), true, "LAB014", null, "TGO", 250.00m, 250.00m, 250.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000039"), true, "LAB015", null, "TGP", 250.00m, 250.00m, 250.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000040"), true, "LAB016", null, "ORINA", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000041"), true, "LAB017", null, "CREATININA", 250.00m, 250.00m, 250.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000042"), true, "LAB018", null, "TIPIFICACION SANQUINIA", 350.00m, 350.00m, 350.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000043"), true, "LAB019", null, "TOXOPLAMOSIS", 350.00m, 350.00m, 350.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000044"), true, "LAB020", null, "NITROGENO UREICO", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000045"), true, "LAB021", null, "FALCEMIA", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000046"), true, "LAB022", null, "HCV HEPATITIS C", 400.00m, 400.00m, 400.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000047"), true, "LAB023", null, "HBS AG. HEPATITIS B", 350.00m, 350.00m, 350.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000048"), true, "LAB024", null, "CULTIVO DE SECRECION NASAL", 350.00m, 350.00m, 350.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000049"), true, "LAB025", null, "HEPATITIS A -IGM", 400.00m, 400.00m, 400.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000050"), true, "LAB026", null, "HEPATITIS C -HVC", 400.00m, 400.00m, 400.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000051"), true, "LAB027", null, "HEMOGLOBINA GLICOSILADA", 350.00m, 350.00m, 350.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000052"), true, "LAB028", null, "HEMOCULTIVO", 450.00m, 450.00m, 450.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000053"), true, "LAB029", null, "HCG CUATITATIVA", 250.00m, 250.00m, 250.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000054"), true, "LAB030", null, "VITAMINA D", 1100.00m, 1100.00m, 1100.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000055"), true, "LAB031", null, "FRASCO DE UROCULTIVO", 50.00m, 50.00m, 50.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000056"), true, "LAB032", null, "FRASCO DE HEMOCULTIVO", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000057"), true, "LAB034", null, "FALCEMIA", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000058"), true, "LAB035", null, "ESTROGENOS TOTALES", 400.00m, 400.00m, 400.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000059"), true, "LAB036", null, "ERITROSEDIMENTACION", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000060"), true, "LAB037", null, "ACIDO FOLICO", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000061"), true, "LAB038", null, "CLORO", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000062"), true, "LAB039", null, "CLORURO", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000063"), true, "LAB040", null, "CALCIO", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000064"), true, "LAB041", null, "BILIRRUBINA", 350.00m, 350.00m, 350.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000065"), true, "LAB042", null, "DHGG CUANTITATIVA", 350.00m, 350.00m, 350.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000066"), true, "LAB043", null, "COCAINA", 350.00m, 350.00m, 350.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000067"), true, "LAB044", null, "COLESTEROL TOTAL", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000068"), true, "LAB045", null, "COLESTEROL HDL", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000069"), true, "LAB046", null, "COLESTEROL DLD", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000070"), true, "LAB047", null, "CONTEO DE PLAQUETAS", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000071"), true, "LAB048", null, "COPROCULTIVO", 400.00m, 400.00m, 400.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000072"), true, "LAB049", null, "CREATITINA EN SUERO", 250.00m, 250.00m, 250.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000073"), true, "LAB050", null, "CULTIVOS DE HECES", 400.00m, 400.00m, 400.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000074"), true, "LAB051", null, "CULTIVO DE SECRECION VAGINAL", 350.00m, 350.00m, 350.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000075"), true, "LAB052", null, "CULTIVO DE SECRECION SEMEN", 400.00m, 400.00m, 400.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000076"), true, "LAB053", null, "B- HCG CUANTITATIVA", 350.00m, 350.00m, 350.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000077"), true, "LAB054", null, "ANTI TIROGLABUTINA", 450.00m, 450.00m, 450.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000078"), true, "LAB055", null, "ANTI-DNA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000079"), true, "LAB056", null, "AMONIO", 350.00m, 350.00m, 350.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000080"), true, "LAB057", null, "TPT TIEMPO TROMBOPLASTINA", 350.00m, 350.00m, 350.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000081"), true, "LAB058", null, "TP TIEMPO PROTROMBINA", 350.00m, 350.00m, 350.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000082"), true, "LAB059", null, "DIMERO D CUANTITATIVO", 950.00m, 950.00m, 950.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000083"), true, "LAB060", null, "ACIDO URICO", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000084"), true, "MED001", "MedicinaGeneralYFamiliar", "CONSULTA MEDICINA GENERAL", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000085"), true, "MED002", "Diabetologia", "CONSULTA DIABETOLOGÍA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000086"), true, "MED003", "MedicinaGeneralYFamiliar", "CONSULTA MEDICINA FAMILIAR", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000087"), true, "MED004", "Sonografia", "CONSULTA SONOGRAFÍA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000088"), true, "MED005", "Ortopedia", "CONSULTA ORTOPEDIA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000089"), true, "MED006", "Nutricion", "CONSULTA NUTRICIONISTA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000090"), true, "MED007", null, "CONSULTA GERIATRÍA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000091"), true, "MED008", "MedicinaInterna", "CONSULTA MEDICINA INTERNA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000092"), true, "MED009", null, "CONSULTA GASTROENTEROLOGÍA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000093"), true, "MED010", "Pediatria", "CONSULTA PEDIATRÍA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000094"), true, "MED011", "Psicologia", "CONSULTA PSICOLÓGICA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000095"), true, "MED012", "Cardiologia", "CONSULTA CARDIOLOGÍA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000096"), true, "MED013", "Ginecologia", "CONSULTA GINECOBSTETRA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000097"), true, "MED014", "Psicologia", "TEST DE FAMILIA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000098"), true, "MED015", "Psicologia", "TEST DE FIGURA HUMANA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000099"), true, "MED016", "Psicologia", "TEST DE RAVEN", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000100"), true, "MED017", "Psicologia", "TERAPIA DE DEPRESIÓN Y ANSIEDAD", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000101"), true, "MED018", "Psicologia", "TERAPIA FAMILIAR", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000102"), true, "MED019", "Psicologia", "TERAPIA DE PAREJA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000103"), true, "MED020", null, "CONSULTA NEFROLOGÍA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000104"), true, "ODO001", "Odontologia", "CONSULTA ODONTOLÓGICA", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000105"), true, "ODO002", null, "ALISADO RADICULAR, CAMPO CERRADO SOD", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000106"), true, "ODO003", null, "APLICACIÓN DE RESINA PREVENTIVA", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000107"), true, "ODO004", null, "APLICACIÓN DE SELLANTES DE AUTOCURADO", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000108"), true, "ODO005", null, "APLICACIÓN DE SELLANTES DE FOTO CURADO", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000109"), true, "ODO006", null, "CIRUGÍA DE DIENTE INCLUIDO (INCLUYE COLG", 1220.00m, 1220.00m, 1220.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000110"), true, "ODO007", "Odontologia", "COLGAJO DESPLAZADO PARA ABORDAJE DE DIEN", 1740.00m, 1740.00m, 1740.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000111"), true, "ODO008", "Odontologia", "CONSULTA ODONTOLOGÍA GENERAL", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000112"), true, "ODO009", "Odontologia", "CONSULTA ODONTOLÓGICA EMERGENCIA Y/O URG", 575.00m, 575.00m, 575.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000113"), true, "ODO010", "Odontologia", "CONSULTA ODONTOLÓGICA ESPECIALIZADA", 500.00m, 500.00m, 500.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000114"), true, "ODO011", null, "CONTROL DE PLACA DENTAL NCOC (PROFILAXIS", 1000.00m, 1000.00m, 1000.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000115"), true, "ODO012", "Odontologia", "CURETAJE A CAMPO ABIERTO", 1800.00m, 1800.00m, 1800.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000116"), true, "ODO013", "Odontologia", "DETARTRAJE SUBGINGIVAL CUADRANTE INFERIO", 900.00m, 900.00m, 900.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000117"), true, "ODO014", "Odontologia", "DETARTRAJE SUBGINGIVAL CUADRANTE INFERIO", 900.00m, 900.00m, 900.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000118"), true, "ODO015", "Odontologia", "DETARTRAJE SUBGINGIVAL CUADRANTE SUPERIO", 900.00m, 900.00m, 900.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000119"), true, "ODO016", "Odontologia", "DETARTRAJE SUBGINGIVAL CUADRANTE SUPERIO", 900.00m, 900.00m, 900.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000120"), true, "ODO017", "Odontologia", "DETARTRAJE SUPRAGINGIVAL CUADRANTE INFER", 550.00m, 550.00m, 550.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000121"), true, "ODO018", "Odontologia", "DETARTRAJE SUPRAGINGIVAL CUADRANTE INFER", 550.00m, 550.00m, 550.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000122"), true, "ODO019", "Odontologia", "DETARTRAJE SUPRAGINGIVAL CUADRANTE SUPER", 550.00m, 550.00m, 550.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000123"), true, "ODO020", "Odontologia", "DETARTRAJE SUPRAGINGIVAL CUADRANTE SUPER", 400.00m, 400.00m, 400.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000124"), true, "ODO021", "Odontologia", "DRENAJE DE COLECCIÓN PERIODONTAL (CERRAD", 500.00m, 500.00m, 500.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000125"), true, "ODO022", null, "ESCISIÓN DE LESIÓN DE ENCÍA SOD U", 1000.00m, 1000.00m, 1000.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000126"), true, "ODO023", "Odontologia", "ESCISIÓN DE LESIÓN ODONTOGÉNICA SOD l", 1000.00m, 1000.00m, 1000.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000127"), true, "ODO024", "Odontologia", "EXODONCIA DE DIENTE INCLUIDO SOD +", 500.00m, 500.00m, 500.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000128"), true, "ODO025", "Odontologia", "EXODONCIA DE DIENTE PERMANENTE MULTIRRAD", 720.00m, 720.00m, 720.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000129"), true, "ODO026", "Odontologia", "EXODONCIA DE DIENTE PERMANENTE UNIRRADIC", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000130"), true, "ODO027", "Odontologia", "EXODONCIA DE DIENTE TEMPORAL MULTIRRADIC", 720.00m, 720.00m, 720.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000131"), true, "ODO028", "Odontologia", "EXODONCIA DE DIENTE TEMPORAL UNIRRADICUL", 600.00m, 600.00m, 600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000132"), true, "ODO029", "Odontologia", "EXODONCIA DE INCLUIDO EN POSICIÓN ECTÓPI", 3000.00m, 3000.00m, 3000.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000133"), true, "ODO030", "Odontologia", "EXODONCIA QUIRÚRGICA MULTIRRADICULARES S", 1300.00m, 1300.00m, 1300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000134"), true, "ODO031", "Odontologia", "EXODONCIA QUIRÚRGICA UNIRRADICULAR SOD", 1300.00m, 1300.00m, 1300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000135"), true, "ODO032", "Odontologia", "EXODONCIAS MÚLTIPLES CON ALVEOLOPLASTIA,", 2150.00m, 2150.00m, 2150.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000136"), true, "ODO033", "Odontologia", "GINGIVECTOMÍA SOD", 1240.00m, 1240.00m, 1240.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000137"), true, "ODO035", null, "TOPICACION DE FLÚOR EN GEL", 300.00m, 300.00m, 300.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000138"), true, "ODO036", null, "TOPICACION DE FLÚOR EN SOLUCIÓN", 200.00m, 200.00m, 200.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000139"), true, "ODO037", "Odontologia", "PRÓTESIS COMPLETAS EN VALPLAST IMPLANTES", 12000.00m, 12000.00m, 12000.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000140"), true, "ODO038", "Odontologia", "IMPLANTES", 36000.00m, 36000.00m, 36000.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000141"), true, "ODO039", "Odontologia", "CORONAS SOBRE IMPLANTES", 18000.00m, 18000.00m, 18000.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000142"), true, "ODO040", "Odontologia", "OBTURACION CON RECINA CLASE I", 1100.00m, 1100.00m, 1100.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000143"), true, "ODO041", "Odontologia", "OBTURACION CON RECINA CLASE II", 1600.00m, 1600.00m, 1600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000144"), true, "ODO042", "Odontologia", "OBTURACION CON RECINA CLASE III", 2000.00m, 2000.00m, 2000.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000145"), true, "ODO043", "Odontologia", "OBTURACION CON RECINA CLASE IV", 2500.00m, 2500.00m, 2500.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000146"), true, "ODO044", "Odontologia", "OBTURACION CON RECINA CLASE V", 3000.00m, 3000.00m, 3000.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000147"), true, "ODO045", null, "CAMBIO DE GOMAS", 1000.00m, 1000.00m, 1000.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000148"), true, "ODO046", "Odontologia", "PARCIAL VALPLAST CENTRAL SUPERIOR", 13000.00m, 13000.00m, 13000.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000149"), true, "ODO047", "Odontologia", "CARILLAS ESTETICAS", 2600.00m, 2600.00m, 2600.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000150"), true, "ODO048", "Odontologia", "ORTODONCIA", 7500.00m, 7500.00m, 7500.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000151"), true, "ODO049", "Odontologia", "ENDODONCIA", 6000.00m, 6000.00m, 6000.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000152"), true, "ODO050", null, "CAMBIO DE ARO", 1000.00m, 1000.00m, 1000.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000153"), true, "ODO051", null, "CEMENTACION DE BLACK", 1000.00m, 1000.00m, 1000.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000154"), true, "ODO052", "Odontologia", "APLICACION DE COLTOSOL", 800.00m, 800.00m, 800.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000155"), true, "ODO053", null, "PERIAPICAL", 400.00m, 400.00m, 400.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000156"), true, "ODO054", "Odontologia", "PROTESIS TOTAL REMOVIBLE", 11000.00m, 11000.00m, 11000.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000157"), true, "ORT001", "Ortopedia", "ORTOPEDA", 500.00m, 500.00m, 500.00m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000158"), true, "PAO001", "Ginecologia", "PAPANICOLAOU", 700.00m, 1000.00m, 1499.89m },
                    { new Guid("7f2b6c10-0002-4a3e-8a9a-000000000159"), true, "PAO002", "Ginecologia", "PAPANICOLAOU LIQUIDO", 1500.00m, 2000.00m, 1500.00m }
                });

            migrationBuilder.InsertData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                columns: new[] { "Id", "Activo", "Especialidad", "MontoFondo", "MontoPaciente", "MontoSeguro", "MontoTotal", "Plan", "Procedimiento", "SeguroMedicoId" },
                values: new object[,]
                {
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000001"), true, "MedicinaGeneralYFamiliar", null, 100.00m, 500.00m, 600.00m, "Estandar", "CONSULTA MEDICINA GENERAL", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000002"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA MEDICINA ESPECIALIZADA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000003"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA PSIQUIATRICA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000004"), true, "Psicologia", null, 250.00m, 600.00m, 850.00m, "Estandar", "CONSULTA PSICOLOGA CLINICA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000005"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA MEDICA QUIRURGICA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000006"), true, "Cardiologia", null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE CARDIOLOGIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000007"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DERMATOLOGICA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000008"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE ENDOCRINOLOGIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000009"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE GASTROENTEROLOGIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000010"), true, "Ginecologia", null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE GINECOLOGIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000011"), true, null, null, 100.00m, 1100.00m, 1200.00m, "Estandar", "CONSULTA DE HEMATOLOGIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000012"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE NEFROLOGIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000013"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE NEUMOLOGIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000014"), true, null, null, 100.00m, 1100.00m, 1200.00m, "Estandar", "CONSULTA DE NEUROCIRUGIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000015"), true, null, null, 100.00m, 1100.00m, 1200.00m, "Estandar", "CONSULTA DE NEUROLOGIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000016"), true, "Oftalmologia", null, 100.00m, 650.00m, 750.00m, "Estandar", "CONSULTA DE OFTALMOLOGIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000017"), true, "Ortopedia", null, 100.00m, 1100.00m, 1200.00m, "Estandar", "CONSULTA DE ORTOPEDIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000018"), true, null, null, 100.00m, 1100.00m, 1200.00m, "Estandar", "CONSULTA DE OTORRINOLARINGOLOGIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000019"), true, "Pediatria", null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE PEDIATRIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000020"), true, null, null, 100.00m, 860.00m, 960.00m, "Estandar", "CONSULTA DE UROLOGIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000021"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE ONCOLOGIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000022"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE GERIATRIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000023"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE REUMATOLOGIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000024"), true, "Diabetologia", null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE DIABETOLOGIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000025"), true, "Nutricion", null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE NUTRICION", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000026"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE INFECTOLOGIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000027"), true, "MedicinaInterna", null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA MEDICINA INTERNA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000028"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA CARDIO-VASCULAR", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000029"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE CIRUGIA PEDIATRICA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000030"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE ENDOCRINOLOGIA PEDIATRICA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000031"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE NEUMOLOGIA PEDIATRICA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000032"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE CIRUGIA GENERAL", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000033"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA PRE-ANESTESIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000034"), true, "MedicinaGeneralYFamiliar", null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE MEDICINA FAMILIAR", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000035"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE PROCTOLOGIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000036"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA DE MAXILOFACIAL", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000037"), true, null, null, 100.00m, 860.00m, 960.00m, "Estandar", "CONSULTA ALERGISTA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000038"), true, null, null, 100.00m, 860.00m, 960.00m, "Estandar", "CONSULTA DE NEONATOLOGIA", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000039"), true, null, null, 100.00m, 750.00m, 850.00m, "Estandar", "CONSULTA CLINICA DEL DOLOR", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000040"), true, null, null, 100.00m, 740.00m, 840.00m, "Estandar", "CONSULTA MEDICINA PIE DIABETICO", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002") },
                    { new Guid("8f3c7d20-0004-4a3e-8a9a-000000000041"), true, "MedicinaGeneralYFamiliar", 250.00m, 100.00m, 500.00m, 600.00m, "Estandar", "CONSULTA MEDICINA GENERAL", new Guid("8f3c7d20-0003-4a3e-8a9a-000000000001") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_servicios_Codigo",
                schema: "funbide",
                table: "servicios",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "servicios",
                schema: "funbide");

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "seguros_medicos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0003-4a3e-8a9a-000000000001"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "seguros_medicos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0003-4a3e-8a9a-000000000002"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000001"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000002"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000003"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000004"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000005"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000006"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000007"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000008"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000009"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000010"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000011"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000012"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000013"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000014"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000015"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000016"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000017"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000018"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000019"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000020"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000021"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000022"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000023"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000024"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000025"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000026"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000027"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000028"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000029"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000030"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000031"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000032"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000033"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000034"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000035"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000036"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000037"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000038"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000039"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000040"));

            migrationBuilder.DeleteData(
                schema: "funbide",
                table: "tarifario_procedimientos",
                keyColumn: "Id",
                keyValue: new Guid("8f3c7d20-0004-4a3e-8a9a-000000000041"));

            migrationBuilder.DropColumn(
                name: "Especialidad",
                schema: "funbide",
                table: "tarifario_procedimientos");

            migrationBuilder.DropColumn(
                name: "MontoFondo",
                schema: "funbide",
                table: "tarifario_procedimientos");

            migrationBuilder.DropColumn(
                name: "MontoFondo",
                schema: "funbide",
                table: "cobros");
        }
    }
}
