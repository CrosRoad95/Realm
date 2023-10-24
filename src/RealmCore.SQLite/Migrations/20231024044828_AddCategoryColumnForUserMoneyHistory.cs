using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealmCore.Persistence.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryColumnForUserMoneyHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "UserMoneyHistory",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "UserMoneyHistory");
        }
    }
}
