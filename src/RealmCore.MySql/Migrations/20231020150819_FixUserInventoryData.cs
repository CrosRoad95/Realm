using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealmCore.Persistence.MySql.Migrations
{
    /// <inheritdoc />
    public partial class FixUserInventoryData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserInventories_Users_UserId1",
                table: "UserInventories");

            migrationBuilder.DropIndex(
                name: "IX_UserInventories_UserId1",
                table: "UserInventories");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserInventories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "UserInventories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserInventories_UserId1",
                table: "UserInventories",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserInventories_Users_UserId1",
                table: "UserInventories",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
