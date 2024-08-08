using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealmCore.Persistence.MySql.Migrations
{
    /// <inheritdoc />
    public partial class FixGroupsRolesKeyPart2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupsRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupsRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupsRoles_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_RoleId",
                table: "GroupMembers",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupsRoles_GroupId",
                table: "GroupsRoles",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_GroupsRoles_RoleId",
                table: "GroupMembers",
                column: "RoleId",
                principalTable: "GroupsRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupsRolesPermissions_GroupsRoles_GroupRoleId",
                table: "GroupsRolesPermissions",
                column: "GroupRoleId",
                principalTable: "GroupsRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_GroupsRoles_RoleId",
                table: "GroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupsRolesPermissions_GroupsRoles_GroupRoleId",
                table: "GroupsRolesPermissions");

            migrationBuilder.DropTable(
                name: "GroupsRoles");

            migrationBuilder.DropIndex(
                name: "IX_GroupMembers_RoleId",
                table: "GroupMembers");
        }
    }
}
