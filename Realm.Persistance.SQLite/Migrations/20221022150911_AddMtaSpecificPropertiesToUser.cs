using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Realm.Persistance.SQLite.Migrations
{
    public partial class AddMtaSpecificPropertiesToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastIp",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLogindDateTime",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSerial",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nick",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<ulong>(
                name: "PlayTime",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<string>(
                name: "RegisterIp",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisterSerial",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegisteredDateTime",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "Skin",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastIp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLogindDateTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastSerial",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Nick",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PlayTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RegisterIp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RegisterSerial",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RegisteredDateTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Skin",
                table: "Users");
        }
    }
}
