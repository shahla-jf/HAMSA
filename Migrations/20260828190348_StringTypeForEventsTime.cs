using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HAMSA.Migrations
{
    /// <inheritdoc />
    public partial class StringTypeForEventsTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSessions");

            migrationBuilder.AddColumn<string>(
                name: "EventTime",
                table: "ResidentEvents",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventTime",
                table: "ResidentEvents");

            migrationBuilder.CreateTable(
                name: "EventSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ResidentEventId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EndTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventSessions_ResidentEvents_ResidentEventId",
                        column: x => x.ResidentEventId,
                        principalTable: "ResidentEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EventSessions_ResidentEventId",
                table: "EventSessions",
                column: "ResidentEventId");
        }
    }
}
