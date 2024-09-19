using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace najemci.Migrations
{
    /// <inheritdoc />
    public partial class RoleNajemnika : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatumNastehovani",
                table: "Najemnici");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DatumNarozeni",
                table: "Najemnici",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "NajemOd",
                table: "Najemnici",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoleNajemnika",
                table: "Najemnici",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NajemOd",
                table: "Najemnici");

            migrationBuilder.DropColumn(
                name: "RoleNajemnika",
                table: "Najemnici");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DatumNarozeni",
                table: "Najemnici",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DatumNastehovani",
                table: "Najemnici",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
