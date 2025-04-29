using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamGate.Worker.Migrations
{
    /// <inheritdoc />
    public partial class V3423 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlertType",
                table: "SensorValues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShiftDate",
                table: "SensorValues",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ShiftName",
                table: "SensorValues",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlertType",
                table: "SensorValues");

            migrationBuilder.DropColumn(
                name: "ShiftDate",
                table: "SensorValues");

            migrationBuilder.DropColumn(
                name: "ShiftName",
                table: "SensorValues");
        }
    }
}
