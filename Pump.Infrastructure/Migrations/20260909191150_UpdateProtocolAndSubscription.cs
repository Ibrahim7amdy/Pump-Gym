using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pump.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProtocolAndSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Protocols");

            migrationBuilder.RenameColumn(
                name: "PriceMonthly",
                table: "Protocols",
                newName: "Price");

            migrationBuilder.AddColumn<int>(
                name: "DurationDays",
                table: "Protocols",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.AddColumn<decimal>(
                name: "PaidPrice",
                table: "MembershipSubscriptions",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ProtocolId",
                table: "MembershipSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSubscriptions_ProtocolId",
                table: "MembershipSubscriptions",
                column: "ProtocolId");

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipSubscriptions_Protocols_ProtocolId",
                table: "MembershipSubscriptions",
                column: "ProtocolId",
                principalTable: "Protocols",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MembershipSubscriptions_Protocols_ProtocolId",
                table: "MembershipSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_MembershipSubscriptions_ProtocolId",
                table: "MembershipSubscriptions");

            migrationBuilder.DropColumn(
                name: "DurationDays",
                table: "Protocols");

            migrationBuilder.DropColumn(
                name: "MembershipType",
                table: "Protocols");

            migrationBuilder.DropColumn(
                name: "SessionsPerMonth",
                table: "Protocols");

            migrationBuilder.DropColumn(
                name: "PaidPrice",
                table: "MembershipSubscriptions");

            migrationBuilder.DropColumn(
                name: "ProtocolId",
                table: "MembershipSubscriptions");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Protocols",
                newName: "PriceMonthly");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Protocols",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
