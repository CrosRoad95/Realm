﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.SQLite.Migrations
{
    /// <inheritdoc />
    public partial class MakeIdsAGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nick = table.Column<string>(type: "TEXT", nullable: true),
                    RegisteredDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastLogindDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RegisterSerial = table.Column<string>(type: "TEXT", nullable: true),
                    RegisterIp = table.Column<string>(type: "TEXT", nullable: true),
                    LastSerial = table.Column<string>(type: "TEXT", nullable: true),
                    LastIp = table.Column<string>(type: "TEXT", nullable: true),
                    PlayTime = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Skin = table.Column<short>(type: "INTEGER", nullable: false),
                    Money = table.Column<decimal>(type: "TEXT", nullable: false),
                    LastTransformAndMotion = table.Column<string>(type: "TEXT", maxLength: 400, nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Model = table.Column<ushort>(type: "INTEGER", nullable: false),
                    TransformAndMotion = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false, defaultValue: "{\"Position\":{\"X\":0.0,\"Y\":0.0,\"Z\":0.0},\"Rotation\":{\"X\":0.0,\"Y\":0.0,\"Z\":0.0},\"Interior\":0,\"Dimension\":0,\"Velocity\":{\"X\":0.0,\"Y\":0.0,\"Z\":0.0},\"AngularVelocity\":{\"X\":0.0,\"Y\":0.0,\"Z\":0.0}}"),
                    Color = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false, defaultValue: "{\"Color1\":\"White\",\"Color2\":\"White\",\"Color3\":\"White\",\"Color4\":\"White\",\"HeadLightColor\":\"White\"}"),
                    Paintjob = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)3),
                    Platetext = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Variant = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "{\"Variant1\":255,\"Variant2\":255}"),
                    DamageState = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false, defaultValue: "{\"FrontLeftPanel\":0,\"FrontRightPanel\":0,\"RearLeftPanel\":0,\"RearRightPanel\":0,\"Windscreen\":0,\"FrontBumper\":0,\"RearBumper\":0,\"Hood\":0,\"Trunk\":0,\"FrontLeftDoor\":0,\"FrontRightDoor\":0,\"RearLeftDoor\":0,\"RearRightDoor\":0,\"FrontLeftLight\":0,\"FrontRightLight\":0,\"RearRightLight\":0,\"RearLeftLight\":0}"),
                    DoorOpenRatio = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: "{\"Hood\":0.0,\"Trunk\":0.0,\"FrontLeft\":0.0,\"FrontRight\":0.0,\"RearLeft\":0.0,\"RearRight\":0.0}"),
                    WheelStatus = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, defaultValue: "{\"FrontLeft\":0,\"RearLeft\":0,\"FrontRight\":0,\"RearRight\":0}"),
                    EngineState = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    LandingGearDown = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    OverrideLights = table.Column<byte>(type: "INTEGER", nullable: false, defaultValue: (byte)0),
                    SirensState = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Locked = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    TaxiLightState = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Health = table.Column<float>(type: "REAL", nullable: false, defaultValue: 1000f),
                    IsFrozen = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Removed = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Spawned = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Progress = table.Column<float>(type: "REAL", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    PrizeReceived = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => new { x.UserId, x.Name });
                    table.ForeignKey(
                        name: "FK_Achievements_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyVisits",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LastVisit = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)),
                    VisitsInRow = table.Column<int>(type: "INTEGER", nullable: false),
                    VisitsInRowRecord = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyVisits", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_DailyVisits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Size = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobUpgrades",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JobId = table.Column<short>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobUpgrades", x => new { x.UserId, x.JobId, x.Name });
                    table.ForeignKey(
                        name: "FK_JobUpgrades_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Statistics",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TraveledDistanceInVehicleAsDriver = table.Column<float>(type: "REAL", nullable: false),
                    TraveledDistanceInVehicleAsPassager = table.Column<float>(type: "REAL", nullable: false),
                    TraveledDistanceSwimming = table.Column<float>(type: "REAL", nullable: false),
                    TraveledDistanceByFoot = table.Column<float>(type: "REAL", nullable: false),
                    TraveledDistanceInAir = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statistics", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Statistics_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLicense",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LicenseId = table.Column<string>(type: "TEXT", nullable: false),
                    SuspendedUntil = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SuspendedReason = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLicense", x => new { x.UserId, x.LicenseId });
                    table.ForeignKey(
                        name: "FK_UserLicense_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleAccesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VehicleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "{\"Ownership\":false}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleAccesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleAccesses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VehicleAccesses_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleFuels",
                columns: table => new
                {
                    VehicleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FuelType = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    MinimumDistanceThreshold = table.Column<float>(type: "REAL", nullable: false),
                    FuelConsumptionPerOneKm = table.Column<float>(type: "REAL", nullable: false),
                    Amount = table.Column<float>(type: "REAL", nullable: false),
                    MaxCapacity = table.Column<float>(type: "REAL", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleFuels", x => new { x.VehicleId, x.FuelType });
                    table.ForeignKey(
                        name: "FK_VehicleFuels_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleUpgrades",
                columns: table => new
                {
                    VehicleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpgradeId = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleUpgrades", x => new { x.VehicleId, x.UpgradeId });
                    table.ForeignKey(
                        name: "FK_VehicleUpgrades_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    InventoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Number = table.Column<uint>(type: "INTEGER", nullable: false),
                    MetaData = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => new { x.Id, x.InventoryId });
                    table.ForeignKey(
                        name: "FK_InventoryItems_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_UserId",
                table: "Inventories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_InventoryId",
                table: "InventoryItems",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAccesses_UserId",
                table: "VehicleAccesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAccesses_VehicleId",
                table: "VehicleAccesses",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "DailyVisits");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "JobUpgrades");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "Statistics");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropTable(
                name: "UserLicense");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "VehicleAccesses");

            migrationBuilder.DropTable(
                name: "VehicleFuels");

            migrationBuilder.DropTable(
                name: "VehicleUpgrades");

            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}