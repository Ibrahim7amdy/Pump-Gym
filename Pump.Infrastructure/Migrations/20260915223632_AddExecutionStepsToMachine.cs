using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pump.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExecutionStepsToMachine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExecutionSteps",
                table: "Machines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutionSteps",
                table: "Machines");
        }
    }
}
