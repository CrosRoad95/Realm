using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealmCore.Persistence.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class AddUserEventsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "UserInventories",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    EventType = table.Column<int>(type: "INTEGER", nullable: false),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEvents", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserInventories_Users_UserId1",
                table: "UserInventories");

            migrationBuilder.DropTable(
                name: "UserEvents");

            migrationBuilder.DropIndex(
                name: "IX_UserInventories_UserId1",
                table: "UserInventories");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserInventories");
        }
    }
}
