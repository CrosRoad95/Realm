using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class DiscoveriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Discoveries",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DiscoveryId = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discoveries", x => new { x.UserId, x.DiscoveryId });
                    table.ForeignKey(
                        name: "FK_Discoveries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Discoveries");
        }
    }
}
