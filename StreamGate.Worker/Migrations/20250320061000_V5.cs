using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamGate.Worker.Migrations
{
    /// <inheritdoc />
    public partial class V5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SensorValues",
                table: "SensorValues");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SensorValues",
                table: "SensorValues",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SensorValues_SensorId",
                table: "SensorValues",
                column: "SensorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SensorValues",
                table: "SensorValues");

            migrationBuilder.DropIndex(
                name: "IX_SensorValues_SensorId",
                table: "SensorValues");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SensorValues",
                table: "SensorValues",
                column: "SensorId");
        }
    }
}
