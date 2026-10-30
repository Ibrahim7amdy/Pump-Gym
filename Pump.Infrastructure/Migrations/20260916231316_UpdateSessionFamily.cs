using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pump.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSessionFamily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_ProtocolDays_ProtocolDayId",
                table: "Sessions");

            migrationBuilder.AddColumn<int>(
                name: "ProtocolExerciseId",
                table: "SessionStations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "IntensityPercent",
                table: "Sessions",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "XpAwarded",
                table: "Sessions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionStations_ProtocolExerciseId",
                table: "SessionStations",
                column: "ProtocolExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionSets_LoggedAtUtc",
                table: "SessionSets",
                column: "LoggedAtUtc");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_ProtocolDays_ProtocolDayId",
                table: "Sessions",
                column: "ProtocolDayId",
                principalTable: "ProtocolDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionStations_ProtocolExercises_ProtocolExerciseId",
                table: "SessionStations",
                column: "ProtocolExerciseId",
                principalTable: "ProtocolExercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_ProtocolDays_ProtocolDayId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionStations_ProtocolExercises_ProtocolExerciseId",
                table: "SessionStations");

            migrationBuilder.DropIndex(
                name: "IX_SessionStations_ProtocolExerciseId",
                table: "SessionStations");

            migrationBuilder.DropIndex(
                name: "IX_SessionSets_LoggedAtUtc",
                table: "SessionSets");

            migrationBuilder.DropColumn(
                name: "ProtocolExerciseId",
                table: "SessionStations");

            migrationBuilder.DropColumn(
                name: "IntensityPercent",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "XpAwarded",
                table: "Sessions");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_ProtocolDays_ProtocolDayId",
                table: "Sessions",
                column: "ProtocolDayId",
                principalTable: "ProtocolDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
