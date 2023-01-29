using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.MySql.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTablesNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserLicense_Users_UserId",
                table: "UserLicense");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserLicense",
                table: "UserLicense");

            migrationBuilder.RenameTable(
                name: "UserLicense",
                newName: "UserLicenses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserLicenses",
                table: "UserLicenses",
                columns: new[] { "UserId", "LicenseId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserLicenses_Users_UserId",
                table: "UserLicenses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserLicenses_Users_UserId",
                table: "UserLicenses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserLicenses",
                table: "UserLicenses");

            migrationBuilder.RenameTable(
                name: "UserLicenses",
                newName: "UserLicense");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserLicense",
                table: "UserLicense",
                columns: new[] { "UserId", "LicenseId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserLicense_Users_UserId",
                table: "UserLicense",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
