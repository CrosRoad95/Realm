using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealmCore.Persistence.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class ChangeColumnFuelTypeTypeToShort : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "FuelType",
                table: "VehicleFuels",
                type: "INTEGER",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 16);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FuelType",
                table: "VehicleFuels",
                type: "TEXT",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(short),
                oldType: "INTEGER",
                oldMaxLength: 16);
        }
    }
}
