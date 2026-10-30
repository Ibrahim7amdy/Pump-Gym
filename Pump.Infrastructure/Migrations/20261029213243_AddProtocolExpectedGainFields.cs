using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pump.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProtocolExpectedGainFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExpectedMuscleGainLabel",
                table: "Protocols",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExpectedPoints",
                table: "Protocols",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpectedStrengthGainLabel",
                table: "Protocols",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedMuscleGainLabel",
                table: "Protocols");

            migrationBuilder.DropColumn(
                name: "ExpectedPoints",
                table: "Protocols");

            migrationBuilder.DropColumn(
                name: "ExpectedStrengthGainLabel",
                table: "Protocols");
        }
    }
}
