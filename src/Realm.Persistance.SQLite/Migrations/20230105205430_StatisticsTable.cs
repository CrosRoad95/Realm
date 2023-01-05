using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class StatisticsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastVisit",
                table: "DailyVisits",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2023, 1, 5, 21, 54, 30, 226, DateTimeKind.Local).AddTicks(1008),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2023, 1, 5, 17, 0, 26, 636, DateTimeKind.Local).AddTicks(7849));

            migrationBuilder.CreateTable(
                name: "Statistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TraveledDistanceInVehicleAsDriver = table.Column<float>(type: "REAL", nullable: false),
                    TraveledDistanceInVehicleAsPassager = table.Column<float>(type: "REAL", nullable: false),
                    TraveledDistanceSwimming = table.Column<float>(type: "REAL", nullable: false),
                    TraveledDistanceByFoot = table.Column<float>(type: "REAL", nullable: false),
                    TraveledDistanceInAir = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Statistics_Users_Id",
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
                name: "Statistics");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastVisit",
                table: "DailyVisits",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2023, 1, 5, 17, 0, 26, 636, DateTimeKind.Local).AddTicks(7849),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2023, 1, 5, 21, 54, 30, 226, DateTimeKind.Local).AddTicks(1008));
        }
    }
}
