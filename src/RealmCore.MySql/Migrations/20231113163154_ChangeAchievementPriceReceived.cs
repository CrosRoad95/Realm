using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealmCore.Persistence.MySql.Migrations
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
                type: "datetime(6)",
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
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
