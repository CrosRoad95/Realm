using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.MySql.Migrations
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
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LastVisit = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValue: new DateTime(2023, 1, 5, 16, 59, 26, 395, DateTimeKind.Local).AddTicks(9930)),
                    VisitsInRow = table.Column<int>(type: "int", nullable: false),
                    VisitsInRowRecord = table.Column<int>(type: "int", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyVisits");
        }
    }
}
