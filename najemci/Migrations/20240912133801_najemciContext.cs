using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace najemci.Migrations
{
    /// <inheritdoc />
    public partial class najemciContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Nemovitosti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Jmeno = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Adresa = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nemovitosti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Byty",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cislo = table.Column<int>(type: "int", nullable: false),
                    Najem = table.Column<int>(type: "int", nullable: false),
                    Kauce = table.Column<int>(type: "int", nullable: false),
                    NajemSluzby = table.Column<int>(type: "int", nullable: false),
                    NemovitostId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Byty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Byty_Nemovitosti_NemovitostId",
                        column: x => x.NemovitostId,
                        principalTable: "Nemovitosti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Najemnici",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Jmeno = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatumNarozeni = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatumNastehovani = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RodneCislo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CisloUctu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BytId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Najemnici", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Najemnici_Byty_BytId",
                        column: x => x.BytId,
                        principalTable: "Byty",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Byty_NemovitostId",
                table: "Byty",
                column: "NemovitostId");

            migrationBuilder.CreateIndex(
                name: "IX_Najemnici_BytId",
                table: "Najemnici",
                column: "BytId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Najemnici");

            migrationBuilder.DropTable(
                name: "Byty");

            migrationBuilder.DropTable(
                name: "Nemovitosti");
        }
    }
}
