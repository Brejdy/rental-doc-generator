using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace najemci.Migrations
{
    /// <inheritdoc />
    public partial class newParcela : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Parcela",
                table: "Nemovitosti",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Parcela",
                table: "Nemovitosti");
        }
    }
}
