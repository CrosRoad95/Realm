using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.MySql.Migrations
{
    /// <inheritdoc />
    public partial class RenameDiscordIntegrationsAndUserUpgradesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscordIntegration_Users_UserId",
                table: "DiscordIntegration");

            migrationBuilder.DropForeignKey(
                name: "FK_UserUpgrade_Users_UserId",
                table: "UserUpgrade");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserUpgrade",
                table: "UserUpgrade");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiscordIntegration",
                table: "DiscordIntegration");

            migrationBuilder.RenameTable(
                name: "UserUpgrade",
                newName: "UserUpgrades");

            migrationBuilder.RenameTable(
                name: "DiscordIntegration",
                newName: "DiscordIntegrations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserUpgrades",
                table: "UserUpgrades",
                columns: new[] { "UserId", "UpgradeId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiscordIntegrations",
                table: "DiscordIntegrations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordIntegrations_Users_UserId",
                table: "DiscordIntegrations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserUpgrades_Users_UserId",
                table: "UserUpgrades",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscordIntegrations_Users_UserId",
                table: "DiscordIntegrations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserUpgrades_Users_UserId",
                table: "UserUpgrades");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserUpgrades",
                table: "UserUpgrades");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiscordIntegrations",
                table: "DiscordIntegrations");

            migrationBuilder.RenameTable(
                name: "UserUpgrades",
                newName: "UserUpgrade");

            migrationBuilder.RenameTable(
                name: "DiscordIntegrations",
                newName: "DiscordIntegration");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserUpgrade",
                table: "UserUpgrade",
                columns: new[] { "UserId", "UpgradeId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiscordIntegration",
                table: "DiscordIntegration",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordIntegration_Users_UserId",
                table: "DiscordIntegration",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserUpgrade_Users_UserId",
                table: "UserUpgrade",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
