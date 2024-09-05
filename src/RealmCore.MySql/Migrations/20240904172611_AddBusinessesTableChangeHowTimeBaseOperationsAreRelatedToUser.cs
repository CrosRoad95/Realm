using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealmCore.Persistence.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessesTableChangeHowTimeBaseOperationsAreRelatedToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeBaseOperationsGroupsUsers");

            migrationBuilder.CreateTable(
                name: "Businesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Metadata = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Businesses", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BusinessesUsers",
                columns: table => new
                {
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Metadata = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessesUsers", x => new { x.BusinessId, x.UserId });
                    table.ForeignKey(
                        name: "FK_BusinessesUsers_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusinessesUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TimeBaseOperationGroupBusinesses",
                columns: table => new
                {
                    OperationGroupId = table.Column<int>(type: "int", nullable: false),
                    BusinessId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeBaseOperationGroupBusinesses", x => new { x.OperationGroupId, x.BusinessId });
                    table.ForeignKey(
                        name: "FK_TimeBaseOperationGroupBusinesses_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimeBaseOperationGroupBusinesses_TimeBaseOperationsGroups_Op~",
                        column: x => x.OperationGroupId,
                        principalTable: "TimeBaseOperationsGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessesUsers_UserId",
                table: "BusinessesUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeBaseOperationGroupBusinesses_BusinessId",
                table: "TimeBaseOperationGroupBusinesses",
                column: "BusinessId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessesUsers");

            migrationBuilder.DropTable(
                name: "TimeBaseOperationGroupBusinesses");

            migrationBuilder.DropTable(
                name: "Businesses");

            migrationBuilder.CreateTable(
                name: "TimeBaseOperationsGroupsUsers",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Metadata = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeBaseOperationsGroupsUsers", x => new { x.GroupId, x.UserId });
                    table.ForeignKey(
                        name: "FK_TimeBaseOperationsGroupsUsers_TimeBaseOperationsGroups_Group~",
                        column: x => x.GroupId,
                        principalTable: "TimeBaseOperationsGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimeBaseOperationsGroupsUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TimeBaseOperationsGroupsUsers_UserId",
                table: "TimeBaseOperationsGroupsUsers",
                column: "UserId");
        }
    }
}
