using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class DailyVisitsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyVisits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LastVisit = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(2023, 1, 5, 17, 0, 26, 636, DateTimeKind.Local).AddTicks(7849)),
                    VisitsInRow = table.Column<int>(type: "INTEGER", nullable: false),
                    VisitsInRowRecord = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyVisits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyVisits_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyVisits");
        }
    }
}
