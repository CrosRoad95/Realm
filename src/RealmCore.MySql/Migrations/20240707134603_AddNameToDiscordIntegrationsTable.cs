using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealmCore.Persistence.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddNameToDiscordIntegrationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "DiscordIntegrations",
                type: "varchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "DiscordIntegrations");
        }
    }
}
