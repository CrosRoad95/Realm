using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealmCore.Persistance.SQLite.Migrations
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
                type: "TEXT",
                precision: 38,
                scale: 18,
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<uint>(
                name: "Size",
                table: "Inventories",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldPrecision: 38,
                oldScale: 18);
        }
    }
}
