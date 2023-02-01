using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class JobStatisticsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobPoints",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobId = table.Column<short>(type: "INTEGER", nullable: false),
                    Points = table.Column<ulong>(type: "INTEGER", nullable: false),
                    TimePlayed = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPoints", x => new { x.UserId, x.JobId });
                    table.ForeignKey(
                        name: "FK_JobPoints_Users_UserId",
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
                name: "JobPoints");
        }
    }
}
