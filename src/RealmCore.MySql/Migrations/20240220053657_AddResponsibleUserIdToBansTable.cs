using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealmCore.Persistence.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddResponsibleUserIdToBansTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Responsible",
                table: "Bans",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ResponsibleUserId",
                table: "Bans",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bans_ResponsibleUserId",
                table: "Bans",
                column: "ResponsibleUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bans_Users_ResponsibleUserId",
                table: "Bans",
                column: "ResponsibleUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bans_Users_ResponsibleUserId",
                table: "Bans");

            migrationBuilder.DropIndex(
                name: "IX_Bans_ResponsibleUserId",
                table: "Bans");

            migrationBuilder.DropColumn(
                name: "ResponsibleUserId",
                table: "Bans");

            migrationBuilder.AlterColumn<string>(
                name: "Responsible",
                table: "Bans",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
