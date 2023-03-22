using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.MySql.Migrations
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
                type: "smallint",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(16)",
                oldMaxLength: 16)
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FuelType",
                table: "VehicleFuels",
                type: "varchar(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldMaxLength: 16)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
