using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamGate.Worker.Migrations
{
    /// <inheritdoc />
    public partial class V30 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SensorShiftResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShiftName = table.Column<int>(type: "int", nullable: false),
                    DateStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SensorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AverageOrErrorValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorShiftResults", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SensorShiftResults_DateEnd",
                table: "SensorShiftResults",
                column: "DateEnd");

            migrationBuilder.CreateIndex(
                name: "IX_SensorShiftResults_DateStart",
                table: "SensorShiftResults",
                column: "DateStart");

            migrationBuilder.CreateIndex(
                name: "IX_SensorShiftResults_SensorId",
                table: "SensorShiftResults",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorShiftResults_ShiftName",
                table: "SensorShiftResults",
                column: "ShiftName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SensorShiftResults");
        }
    }
}
