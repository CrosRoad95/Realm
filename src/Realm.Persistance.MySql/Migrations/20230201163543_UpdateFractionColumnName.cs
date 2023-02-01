using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.MySql.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFractionColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Shortcut",
                table: "Fractions",
                newName: "Code");

            migrationBuilder.RenameIndex(
                name: "IX_Fractions_Shortcut",
                table: "Fractions",
                newName: "IX_Fractions_Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Fractions",
                newName: "Shortcut");

            migrationBuilder.RenameIndex(
                name: "IX_Fractions_Code",
                table: "Fractions",
                newName: "IX_Fractions_Shortcut");
        }
    }
}
