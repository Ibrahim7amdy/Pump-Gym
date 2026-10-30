using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pump.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDropInPricingAndVisitFee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DropInFee",
                table: "Visits",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "GymPricingSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    DropInFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GymPricingSettings", x => x.Id);
                    table.CheckConstraint("CK_GymPricingSettings_DropInFee_NonNegative", "[DropInFee] >= 0");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GymPricingSettings");

            migrationBuilder.DropColumn(
                name: "DropInFee",
                table: "Visits");
        }
    }
}
