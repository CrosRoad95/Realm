using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.MySql.Migrations
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
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    JobId = table.Column<short>(type: "smallint", nullable: false),
                    Points = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    TimePlayed = table.Column<ulong>(type: "bigint unsigned", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobPoints");
        }
    }
}
