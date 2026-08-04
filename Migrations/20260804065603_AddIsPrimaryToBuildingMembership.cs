using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HAMSA.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPrimaryToBuildingMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "BuildingMemberships",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "BuildingMemberships");
        }
    }
}
