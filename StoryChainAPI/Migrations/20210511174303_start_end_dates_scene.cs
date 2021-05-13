using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StoryChainAPI.Migrations
{
    public partial class start_end_dates_scene : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WrittenOn",
                table: "Scenes",
                newName: "StartedOn");

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedOn",
                table: "Scenes",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinishedOn",
                table: "Scenes");

            migrationBuilder.RenameColumn(
                name: "StartedOn",
                table: "Scenes",
                newName: "WrittenOn");
        }
    }
}
