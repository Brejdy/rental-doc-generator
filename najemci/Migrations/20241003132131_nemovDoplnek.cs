using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace najemci.Migrations
{
    /// <inheritdoc />
    public partial class nemovDoplnek : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CisloPopisne",
                table: "Nemovitosti",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LV",
                table: "Nemovitosti",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Obec",
                table: "Nemovitosti",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CisloPopisne",
                table: "Nemovitosti");

            migrationBuilder.DropColumn(
                name: "LV",
                table: "Nemovitosti");

            migrationBuilder.DropColumn(
                name: "Obec",
                table: "Nemovitosti");
        }
    }
}
