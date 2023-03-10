using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AchievementAddKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddPrimaryKey(
                name: "PK_Achievements",
                table: "Achievements",
                columns: new[] { "UserId", "AchievementId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Achievements_Users_UserId",
                table: "Achievements",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Achievements_Users_UserId",
                table: "Achievements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Achievements",
                table: "Achievements");
        }
    }
}
