using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.MySql.Migrations
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
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2023, 1, 5, 21, 55, 6, 760, DateTimeKind.Local).AddTicks(659),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2023, 1, 5, 16, 59, 26, 395, DateTimeKind.Local).AddTicks(9930));

            migrationBuilder.CreateTable(
                name: "Statistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TraveledDistanceInVehicleAsDriver = table.Column<float>(type: "float", nullable: false),
                    TraveledDistanceInVehicleAsPassager = table.Column<float>(type: "float", nullable: false),
                    TraveledDistanceSwimming = table.Column<float>(type: "float", nullable: false),
                    TraveledDistanceByFoot = table.Column<float>(type: "float", nullable: false),
                    TraveledDistanceInAir = table.Column<float>(type: "float", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Statistics");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastVisit",
                table: "DailyVisits",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2023, 1, 5, 16, 59, 26, 395, DateTimeKind.Local).AddTicks(9930),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2023, 1, 5, 21, 55, 6, 760, DateTimeKind.Local).AddTicks(659));
        }
    }
}
