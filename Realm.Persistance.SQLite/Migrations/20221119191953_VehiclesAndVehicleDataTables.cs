using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.SQLite.Migrations
{
    public partial class VehiclesAndVehicleDataTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
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
                    Removed = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleData",
                columns: table => new
                {
                    VehicleId = table.Column<string>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleData", x => new { x.VehicleId, x.Key });
                    table.ForeignKey(
                        name: "FK_VehicleData_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleData");

            migrationBuilder.DropTable(
                name: "Vehicles");
        }
    }
}
