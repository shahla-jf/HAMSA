using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HAMSA.Migrations
{
    /// <inheritdoc />
    public partial class CreatedByUserInBuildingExpense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "BuildingExpenses",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "BuildingExpenses",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_BuildingExpenses_CreatedByUserId",
                table: "BuildingExpenses",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingExpenses_Users_CreatedByUserId",
                table: "BuildingExpenses",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuildingExpenses_Users_CreatedByUserId",
                table: "BuildingExpenses");

            migrationBuilder.DropIndex(
                name: "IX_BuildingExpenses_CreatedByUserId",
                table: "BuildingExpenses");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "BuildingExpenses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BuildingExpenses");
        }
    }
}
