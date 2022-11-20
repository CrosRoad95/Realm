using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.SQLite.Migrations
{
    public partial class IsFrozenPropertyForVehicleEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFrozen",
                table: "Vehicles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFrozen",
                table: "Vehicles");
        }
    }
}
