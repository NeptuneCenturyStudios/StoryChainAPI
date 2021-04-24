using Microsoft.EntityFrameworkCore.Migrations;

namespace StoryChainAPI.Migrations
{
    public partial class genre_audience : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SceneText",
                table: "Scenes",
                newName: "Text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tags",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Stories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AudienceId",
                table: "Stories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "StoryId",
                table: "Scenes",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Audience",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audience", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GenreStory",
                columns: table => new
                {
                    GenresId = table.Column<int>(type: "int", nullable: false),
                    StoriesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenreStory", x => new { x.GenresId, x.StoriesId });
                    table.ForeignKey(
                        name: "FK_GenreStory_Genres_GenresId",
                        column: x => x.GenresId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GenreStory_Stories_StoriesId",
                        column: x => x.StoriesId,
                        principalTable: "Stories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Audience",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Children" },
                    { 2, "Middle Grade" },
                    { 3, "Young Adult" },
                    { 4, "Adult" }
                });

            migrationBuilder.InsertData(
                table: "Genres",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Fantasy" },
                    { 2, "Science Fiction" },
                    { 3, "Mystery" },
                    { 4, "Thriller" },
                    { 5, "Romance" },
                    { 6, "Western" },
                    { 7, "Dystopian" },
                    { 8, "Contemporary" },
                    { 9, "Historical Fiction" },
                    { 10, "Action and Adventure" },
                    { 11, "Horror" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stories_AudienceId",
                table: "Stories",
                column: "AudienceId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenes_StoryId",
                table: "Scenes",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_GenreStory_StoriesId",
                table: "GenreStory",
                column: "StoriesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Scenes_Stories_StoryId",
                table: "Scenes",
                column: "StoryId",
                principalTable: "Stories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Stories_Audience_AudienceId",
                table: "Stories",
                column: "AudienceId",
                principalTable: "Audience",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scenes_Stories_StoryId",
                table: "Scenes");

            migrationBuilder.DropForeignKey(
                name: "FK_Stories_Audience_AudienceId",
                table: "Stories");

            migrationBuilder.DropTable(
                name: "Audience");

            migrationBuilder.DropTable(
                name: "GenreStory");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropIndex(
                name: "IX_Stories_AudienceId",
                table: "Stories");

            migrationBuilder.DropIndex(
                name: "IX_Scenes_StoryId",
                table: "Scenes");

            migrationBuilder.DropColumn(
                name: "AudienceId",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "StoryId",
                table: "Scenes");

            migrationBuilder.RenameColumn(
                name: "Text",
                table: "Scenes",
                newName: "SceneText");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tags",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Stories",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
