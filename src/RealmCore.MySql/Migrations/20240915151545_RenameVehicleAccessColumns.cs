using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealmCore.Persistence.MySql.Migrations
{
    /// <inheritdoc />
    public partial class RenameVehicleAccessColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomValue",
                table: "VehicleUserAccess");

            migrationBuilder.DropColumn(
                name: "CustomValue",
                table: "VehicleGroupAccesses");

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "VehicleUserAccess",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "VehicleGroupAccesses",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "VehicleUserAccess");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "VehicleGroupAccesses");

            migrationBuilder.AddColumn<string>(
                name: "CustomValue",
                table: "VehicleUserAccess",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CustomValue",
                table: "VehicleGroupAccesses",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
