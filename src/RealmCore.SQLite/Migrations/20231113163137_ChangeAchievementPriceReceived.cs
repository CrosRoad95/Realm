using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealmCore.Persistence.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAchievementPriceReceived : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrizeReceived",
                table: "Achievements");

            migrationBuilder.AddColumn<DateTime>(
                name: "PrizeReceivedDateTime",
                table: "Achievements",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrizeReceivedDateTime",
                table: "Achievements");

            migrationBuilder.AddColumn<bool>(
                name: "PrizeReceived",
                table: "Achievements",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
