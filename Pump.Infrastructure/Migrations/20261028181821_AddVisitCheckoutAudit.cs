using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pump.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitCheckoutAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CheckedOutByStaffUserId",
                table: "Visits",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_CheckedOutByStaffUserId",
                table: "Visits",
                column: "CheckedOutByStaffUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_Users_CheckedOutByStaffUserId",
                table: "Visits",
                column: "CheckedOutByStaffUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Users_CheckedOutByStaffUserId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_CheckedOutByStaffUserId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CheckedOutByStaffUserId",
                table: "Visits");
        }
    }
}
