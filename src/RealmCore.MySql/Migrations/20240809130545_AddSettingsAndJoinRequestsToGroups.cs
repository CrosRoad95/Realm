using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealmCore.Persistence.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingsAndJoinRequestsToGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupsEvents_Groups_GroupId",
                table: "GroupsEvents");

            migrationBuilder.DropIndex(
                name: "IX_GroupsEvents_GroupId",
                table: "GroupsEvents");

            migrationBuilder.AddColumn<int>(
                name: "GroupId1",
                table: "GroupsRoles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupDataId",
                table: "GroupsEvents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GroupsJoinRequests",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Metadata = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupsJoinRequests", x => new { x.GroupId, x.UserId });
                    table.ForeignKey(
                        name: "FK_GroupsJoinRequests_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupsJoinRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GroupsSettings",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    SettingId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupsSettings", x => new { x.GroupId, x.SettingId });
                    table.ForeignKey(
                        name: "FK_GroupsSettings_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GroupsRoles_GroupId1",
                table: "GroupsRoles",
                column: "GroupId1");

            migrationBuilder.CreateIndex(
                name: "IX_GroupsEvents_GroupDataId",
                table: "GroupsEvents",
                column: "GroupDataId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupsJoinRequests_UserId",
                table: "GroupsJoinRequests",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupsEvents_Groups_GroupDataId",
                table: "GroupsEvents",
                column: "GroupDataId",
                principalTable: "Groups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupsRoles_Groups_GroupId1",
                table: "GroupsRoles",
                column: "GroupId1",
                principalTable: "Groups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupsEvents_Groups_GroupDataId",
                table: "GroupsEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupsRoles_Groups_GroupId1",
                table: "GroupsRoles");

            migrationBuilder.DropTable(
                name: "GroupsJoinRequests");

            migrationBuilder.DropTable(
                name: "GroupsSettings");

            migrationBuilder.DropIndex(
                name: "IX_GroupsRoles_GroupId1",
                table: "GroupsRoles");

            migrationBuilder.DropIndex(
                name: "IX_GroupsEvents_GroupDataId",
                table: "GroupsEvents");

            migrationBuilder.DropColumn(
                name: "GroupId1",
                table: "GroupsRoles");

            migrationBuilder.DropColumn(
                name: "GroupDataId",
                table: "GroupsEvents");

            migrationBuilder.CreateIndex(
                name: "IX_GroupsEvents_GroupId",
                table: "GroupsEvents",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupsEvents_Groups_GroupId",
                table: "GroupsEvents",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
