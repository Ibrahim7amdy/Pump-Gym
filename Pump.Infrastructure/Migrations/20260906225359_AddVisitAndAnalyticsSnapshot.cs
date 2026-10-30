using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pump.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitAndAnalyticsSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalyticsDailySnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SnapshotDateUtc = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    NewMembers = table.Column<int>(type: "int", nullable: false),
                    ActiveMembers = table.Column<int>(type: "int", nullable: false),
                    TotalVisits = table.Column<int>(type: "int", nullable: false),
                    AvgOccupancy = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsDailySnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Visits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    GuestName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CheckInAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Source = table.Column<byte>(type: "tinyint", nullable: false),
                    CheckedInByStaffUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visits_Users_CheckedInByStaffUserId",
                        column: x => x.CheckedInByStaffUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsDailySnapshots_SnapshotDateUtc",
                table: "AnalyticsDailySnapshots",
                column: "SnapshotDateUtc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_CheckedInByStaffUserId",
                table: "Visits",
                column: "CheckedInByStaffUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_CheckInAtUtc_CheckOutAtUtc",
                table: "Visits",
                columns: new[] { "CheckInAtUtc", "CheckOutAtUtc" },
                filter: "[CheckOutAtUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_UserId",
                table: "Visits",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsDailySnapshots");

            migrationBuilder.DropTable(
                name: "Visits");
        }
    }
}
