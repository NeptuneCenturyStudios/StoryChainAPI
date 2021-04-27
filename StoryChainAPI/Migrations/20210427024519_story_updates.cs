using Microsoft.EntityFrameworkCore.Migrations;

namespace StoryChainAPI.Migrations
{
    public partial class story_updates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Stories",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stories_CreatedById",
                table: "Stories",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Stories_AspNetUsers_CreatedById",
                table: "Stories",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stories_AspNetUsers_CreatedById",
                table: "Stories");

            migrationBuilder.DropIndex(
                name: "IX_Stories_CreatedById",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Stories");
        }
    }
}
