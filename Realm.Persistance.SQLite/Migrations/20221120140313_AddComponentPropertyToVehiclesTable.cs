using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.SQLite.Migrations
{
    public partial class AddComponentPropertyToVehiclesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Components",
                table: "Vehicles",
                type: "TEXT",
                maxLength: 65535,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Components",
                table: "Vehicles");
        }
    }
}
