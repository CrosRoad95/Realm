using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealmCore.Persistence.MySql.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDuplicatedGroupsAndRolesRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_GroupsRoles_RoleId",
                table: "GroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_Groups_GroupId",
                table: "GroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_Users_UserId",
                table: "GroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupsRoles_Groups_GroupId1",
                table: "GroupsRoles");

            migrationBuilder.DropIndex(
                name: "IX_GroupsRoles_GroupId1",
                table: "GroupsRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupMembers",
                table: "GroupMembers");

            migrationBuilder.DropColumn(
                name: "GroupId1",
                table: "GroupsRoles");

            migrationBuilder.RenameTable(
                name: "GroupMembers",
                newName: "GroupsMembers");

            migrationBuilder.RenameIndex(
                name: "IX_GroupMembers_UserId",
                table: "GroupsMembers",
                newName: "IX_GroupsMembers_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupMembers_RoleId",
                table: "GroupsMembers",
                newName: "IX_GroupsMembers_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupMembers_GroupId",
                table: "GroupsMembers",
                newName: "IX_GroupsMembers_GroupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupsMembers",
                table: "GroupsMembers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupsMembers_GroupsRoles_RoleId",
                table: "GroupsMembers",
                column: "RoleId",
                principalTable: "GroupsRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupsMembers_Groups_GroupId",
                table: "GroupsMembers",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupsMembers_Users_UserId",
                table: "GroupsMembers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupsMembers_GroupsRoles_RoleId",
                table: "GroupsMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupsMembers_Groups_GroupId",
                table: "GroupsMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupsMembers_Users_UserId",
                table: "GroupsMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupsMembers",
                table: "GroupsMembers");

            migrationBuilder.RenameTable(
                name: "GroupsMembers",
                newName: "GroupMembers");

            migrationBuilder.RenameIndex(
                name: "IX_GroupsMembers_UserId",
                table: "GroupMembers",
                newName: "IX_GroupMembers_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupsMembers_RoleId",
                table: "GroupMembers",
                newName: "IX_GroupMembers_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupsMembers_GroupId",
                table: "GroupMembers",
                newName: "IX_GroupMembers_GroupId");

            migrationBuilder.AddColumn<int>(
                name: "GroupId1",
                table: "GroupsRoles",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupMembers",
                table: "GroupMembers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_GroupsRoles_GroupId1",
                table: "GroupsRoles",
                column: "GroupId1");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_GroupsRoles_RoleId",
                table: "GroupMembers",
                column: "RoleId",
                principalTable: "GroupsRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_Groups_GroupId",
                table: "GroupMembers",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_Users_UserId",
                table: "GroupMembers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupsRoles_Groups_GroupId1",
                table: "GroupsRoles",
                column: "GroupId1",
                principalTable: "Groups",
                principalColumn: "Id");
        }
    }
}
