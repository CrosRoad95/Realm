using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class FractionsAndFractionMembersTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Shortcut = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fractions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FractionMembers",
                columns: table => new
                {
                    FractionId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Rank = table.Column<int>(type: "INTEGER", nullable: false),
                    RankName = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FractionMembers", x => new { x.FractionId, x.UserId });
                    table.ForeignKey(
                        name: "FK_FractionMembers_Fractions_FractionId",
                        column: x => x.FractionId,
                        principalTable: "Fractions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FractionMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FractionMembers_UserId",
                table: "FractionMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Fractions_Name",
                table: "Fractions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fractions_Shortcut",
                table: "Fractions",
                column: "Shortcut",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FractionMembers");

            migrationBuilder.DropTable(
                name: "Fractions");
        }
    }
}
