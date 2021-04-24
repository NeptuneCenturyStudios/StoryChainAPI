using Microsoft.EntityFrameworkCore.Migrations;

namespace StoryChainAPI.Migrations
{
    public partial class genre_audiences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stories_Audience_AudienceId",
                table: "Stories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Audience",
                table: "Audience");

            migrationBuilder.RenameTable(
                name: "Audience",
                newName: "Audiences");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Audiences",
                table: "Audiences",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stories_Audiences_AudienceId",
                table: "Stories",
                column: "AudienceId",
                principalTable: "Audiences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stories_Audiences_AudienceId",
                table: "Stories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Audiences",
                table: "Audiences");

            migrationBuilder.RenameTable(
                name: "Audiences",
                newName: "Audience");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Audience",
                table: "Audience",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stories_Audience_AudienceId",
                table: "Stories",
                column: "AudienceId",
                principalTable: "Audience",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
