using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealmCore.Persistence.MySql.Migrations
{
    /// <inheritdoc />
    public partial class FixBusinessStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessStatistics_Businesses_StatisticId",
                table: "BusinessStatistics");

            migrationBuilder.DropIndex(
                name: "IX_BusinessStatistics_StatisticId",
                table: "BusinessStatistics");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessStatistics_Businesses_BusinessId",
                table: "BusinessStatistics",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessStatistics_Businesses_BusinessId",
                table: "BusinessStatistics");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessStatistics_StatisticId",
                table: "BusinessStatistics",
                column: "StatisticId");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessStatistics_Businesses_StatisticId",
                table: "BusinessStatistics",
                column: "StatisticId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
