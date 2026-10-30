using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pump.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProtocolVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchivedVersion",
                table: "Protocols",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductFamilyId",
                table: "Protocols",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.CreateIndex(
                name: "IX_Protocols_ProductFamilyId",
                table: "Protocols",
                column: "ProductFamilyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Protocols_ProductFamilyId",
                table: "Protocols");

            migrationBuilder.DropColumn(
                name: "IsArchivedVersion",
                table: "Protocols");

            migrationBuilder.DropColumn(
                name: "ProductFamilyId",
                table: "Protocols");
        }
    }
}
