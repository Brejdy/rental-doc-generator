using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace najemci.Migrations
{
    /// <inheritdoc />
    public partial class CisloOP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CisloOP",
                table: "Najemnici",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CisloOP",
                table: "Najemnici");
        }
    }
}
