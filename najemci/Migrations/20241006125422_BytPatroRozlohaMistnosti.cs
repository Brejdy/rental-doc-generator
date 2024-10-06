using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace najemci.Migrations
{
    /// <inheritdoc />
    public partial class BytPatroRozlohaMistnosti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Mistnosti",
                table: "Byty",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Patro",
                table: "Byty",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Rozloha",
                table: "Byty",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mistnosti",
                table: "Byty");

            migrationBuilder.DropColumn(
                name: "Patro",
                table: "Byty");

            migrationBuilder.DropColumn(
                name: "Rozloha",
                table: "Byty");
        }
    }
}
