using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pump.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentIdToSessionAndNullableDayNumInProtocolDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProtocolDays_ProtocolId_DayNumber",
                table: "ProtocolDays");

            migrationBuilder.AddColumn<int>(
                name: "AthleteProtocolAssignmentId",
                table: "Sessions",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DayNumber",
                table: "ProtocolDays",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_AthleteProtocolAssignmentId",
                table: "Sessions",
                column: "AthleteProtocolAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolDays_ProtocolId_DayNumber",
                table: "ProtocolDays",
                columns: new[] { "ProtocolId", "DayNumber" },
                unique: true,
                filter: "[DayNumber] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_AthleteProtocolAssignments_AthleteProtocolAssignmentId",
                table: "Sessions",
                column: "AthleteProtocolAssignmentId",
                principalTable: "AthleteProtocolAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_AthleteProtocolAssignments_AthleteProtocolAssignmentId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_AthleteProtocolAssignmentId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_ProtocolDays_ProtocolId_DayNumber",
                table: "ProtocolDays");

            migrationBuilder.DropColumn(
                name: "AthleteProtocolAssignmentId",
                table: "Sessions");

            migrationBuilder.AlterColumn<int>(
                name: "DayNumber",
                table: "ProtocolDays",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolDays_ProtocolId_DayNumber",
                table: "ProtocolDays",
                columns: new[] { "ProtocolId", "DayNumber" },
                unique: true);
        }
    }
}
