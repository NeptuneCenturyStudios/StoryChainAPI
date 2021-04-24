using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StoryChainAPI.Migrations
{
    public partial class views : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "Stories",
                newName: "SceneTimeLimitInSeconds");

            migrationBuilder.RenameColumn(
                name: "LockedUntil",
                table: "Locks",
                newName: "LockStart");

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedOn",
                table: "Stories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MaxScenes",
                table: "Stories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ShowAllPreviousScenes",
                table: "Stories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedOn",
                table: "Stories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "WrittenOn",
                table: "Scenes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LockEnd",
                table: "Locks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Views",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ViewedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StoryId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Views", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Views_Stories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "Stories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Views_StoryId",
                table: "Views",
                column: "StoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Views");

            migrationBuilder.DropColumn(
                name: "FinishedOn",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "MaxScenes",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "ShowAllPreviousScenes",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "StartedOn",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "WrittenOn",
                table: "Scenes");

            migrationBuilder.DropColumn(
                name: "LockEnd",
                table: "Locks");

            migrationBuilder.RenameColumn(
                name: "SceneTimeLimitInSeconds",
                table: "Stories",
                newName: "Rating");

            migrationBuilder.RenameColumn(
                name: "LockStart",
                table: "Locks",
                newName: "LockedUntil");
        }
    }
}
