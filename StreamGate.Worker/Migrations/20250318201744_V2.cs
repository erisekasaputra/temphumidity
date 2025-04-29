using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamGate.Worker.Migrations
{
    /// <inheritdoc />
    public partial class V2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SensorConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SensorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UCL = table.Column<float>(type: "real", nullable: false),
                    LCL = table.Column<float>(type: "real", nullable: false),
                    WarningUpperLevel = table.Column<float>(type: "real", nullable: false),
                    WarningLowerLevel = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorConfigs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SensorConfigs");
        }
    }
}
