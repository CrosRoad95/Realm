using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealmCore.Persistance.MySql.Migrations
{
    /// <inheritdoc />
    public partial class Cleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleInventory_Inventories_InventoryId",
                table: "VehicleInventory");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleInventory_Vehicles_VehicleId",
                table: "VehicleInventory");

            migrationBuilder.DropTable(
                name: "VehiclePlayerAccesses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleInventory",
                table: "VehicleInventory");

            migrationBuilder.RenameTable(
                name: "VehicleInventory",
                newName: "VehicleInventories");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleInventory_InventoryId",
                table: "VehicleInventories",
                newName: "IX_VehicleInventories_InventoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleInventories",
                table: "VehicleInventories",
                columns: new[] { "VehicleId", "InventoryId" });

            migrationBuilder.CreateTable(
                name: "VehicleUserAccess",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AccessType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CustomValue = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleUserAccess", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleUserAccess_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleUserAccess_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleUserAccess_UserId",
                table: "VehicleUserAccess",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleUserAccess_VehicleId",
                table: "VehicleUserAccess",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleInventories_Inventories_InventoryId",
                table: "VehicleInventories",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleInventories_Vehicles_VehicleId",
                table: "VehicleInventories",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleInventories_Inventories_InventoryId",
                table: "VehicleInventories");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleInventories_Vehicles_VehicleId",
                table: "VehicleInventories");

            migrationBuilder.DropTable(
                name: "VehicleUserAccess");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleInventories",
                table: "VehicleInventories");

            migrationBuilder.RenameTable(
                name: "VehicleInventories",
                newName: "VehicleInventory");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleInventories_InventoryId",
                table: "VehicleInventory",
                newName: "IX_VehicleInventory_InventoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleInventory",
                table: "VehicleInventory",
                columns: new[] { "VehicleId", "InventoryId" });

            migrationBuilder.CreateTable(
                name: "VehiclePlayerAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    AccessType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    CustomValue = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehiclePlayerAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehiclePlayerAccesses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehiclePlayerAccesses_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePlayerAccesses_UserId",
                table: "VehiclePlayerAccesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePlayerAccesses_VehicleId",
                table: "VehiclePlayerAccesses",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleInventory_Inventories_InventoryId",
                table: "VehicleInventory",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleInventory_Vehicles_VehicleId",
                table: "VehicleInventory",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
