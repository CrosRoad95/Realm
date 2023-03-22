using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.MySql.Migrations
{
    /// <inheritdoc />
    public partial class ChangeInventorySizeColumnType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Size",
                table: "Inventories",
                type: "decimal(38,18)",
                precision: 38,
                scale: 18,
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<uint>(
                name: "Size",
                table: "Inventories",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,18)",
                oldPrecision: 38,
                oldScale: 18);
        }
    }
}
