using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamGate.Worker.Migrations
{
    /// <inheritdoc />
    public partial class V2003 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "SensorConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "SensorConfigs");
        }
    }
}
