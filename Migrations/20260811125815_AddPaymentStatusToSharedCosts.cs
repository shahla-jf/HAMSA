using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HAMSA.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentStatusToSharedCosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCleaningPaid",
                table: "Buildings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsElectricityPaid",
                table: "Buildings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsElevatorPaid",
                table: "Buildings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWaterPaid",
                table: "Buildings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCleaningPaid",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "IsElectricityPaid",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "IsElevatorPaid",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "IsWaterPaid",
                table: "Buildings");
        }
    }
}
