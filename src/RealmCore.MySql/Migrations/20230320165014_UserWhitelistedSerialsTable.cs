using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealmCore.Persistance.MySql.Migrations
{
    /// <inheritdoc />
    public partial class UserWhitelistedSerialsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserWhitelistedSerials",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Serial = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWhitelistedSerials", x => new { x.UserId, x.Serial });
                    table.ForeignKey(
                        name: "FK_UserWhitelistedSerials_Users_UserId",
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
                name: "UserWhitelistedSerials");
        }
    }
}
