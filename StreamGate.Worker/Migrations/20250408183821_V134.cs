using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamGate.Worker.Migrations
{
    /// <inheritdoc />
    public partial class V134 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "SensorValues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "SensorValues",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "SensorValues");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "SensorValues");
        }
    }
}
