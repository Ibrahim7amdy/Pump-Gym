using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pump.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeleteSubscriptionEditAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AthleteProtocolAssignments_Users_AssignedByUserId",
                table: "AthleteProtocolAssignments");

            migrationBuilder.DropTable(
                name: "MembershipSubscriptions");

            migrationBuilder.DropColumn(
                name: "MembershipType",
                table: "Protocols");

            migrationBuilder.DropColumn(
                name: "SessionsPerMonth",
                table: "Protocols");

            migrationBuilder.RenameColumn(
                name: "StartedOnUtc",
                table: "AthleteProtocolAssignments",
                newName: "ExpiresOnUtc");

            migrationBuilder.AlterColumn<int>(
                name: "AssignedByUserId",
                table: "AthleteProtocolAssignments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateOnly>(
                name: "AssignedOnUtc",
                table: "AthleteProtocolAssignments",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<decimal>(
                name: "PaidPrice",
                table: "AthleteProtocolAssignments",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_AthleteProtocolAssignments_Users_AssignedByUserId",
                table: "AthleteProtocolAssignments",
                column: "AssignedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AthleteProtocolAssignments_Users_AssignedByUserId",
                table: "AthleteProtocolAssignments");

            migrationBuilder.DropColumn(
                name: "AssignedOnUtc",
                table: "AthleteProtocolAssignments");

            migrationBuilder.DropColumn(
                name: "PaidPrice",
                table: "AthleteProtocolAssignments");

            migrationBuilder.RenameColumn(
                name: "ExpiresOnUtc",
                table: "AthleteProtocolAssignments",
                newName: "StartedOnUtc");

            migrationBuilder.AddColumn<byte>(
                name: "MembershipType",
                table: "Protocols",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "SessionsPerMonth",
                table: "Protocols",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AssignedByUserId",
                table: "AthleteProtocolAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "MembershipSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProtocolId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JoinDateUtc = table.Column<DateOnly>(type: "date", nullable: false),
                    PaidPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    RenewsOnUtc = table.Column<DateOnly>(type: "date", nullable: false),
                    SessionsUsedThisMonth = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipSubscriptions_Protocols_ProtocolId",
                        column: x => x.ProtocolId,
                        principalTable: "Protocols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MembershipSubscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSubscriptions_ProtocolId",
                table: "MembershipSubscriptions",
                column: "ProtocolId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSubscriptions_UserId",
                table: "MembershipSubscriptions",
                column: "UserId",
                unique: true,
                filter: "[Status] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_AthleteProtocolAssignments_Users_AssignedByUserId",
                table: "AthleteProtocolAssignments",
                column: "AssignedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
