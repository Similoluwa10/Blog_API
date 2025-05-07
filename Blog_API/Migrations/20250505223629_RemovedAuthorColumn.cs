using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog_API.Migrations
{
    /// <inheritdoc />
    public partial class RemovedAuthorColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogPostTable_UserTable_UserId",
                table: "BlogPostTable");

            migrationBuilder.DropIndex(
                name: "IX_BlogPostTable_UserId",
                table: "BlogPostTable");

            migrationBuilder.DropColumn(
                name: "Author",
                table: "BlogPostTable");

            migrationBuilder.AddColumn<int>(
                name: "UserModelId",
                table: "BlogPostTable",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogPostTable_UserModelId",
                table: "BlogPostTable",
                column: "UserModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPostTable_UserTable_UserModelId",
                table: "BlogPostTable",
                column: "UserModelId",
                principalTable: "UserTable",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogPostTable_UserTable_UserModelId",
                table: "BlogPostTable");

            migrationBuilder.DropIndex(
                name: "IX_BlogPostTable_UserModelId",
                table: "BlogPostTable");

            migrationBuilder.DropColumn(
                name: "UserModelId",
                table: "BlogPostTable");

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "BlogPostTable",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPostTable_UserId",
                table: "BlogPostTable",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPostTable_UserTable_UserId",
                table: "BlogPostTable",
                column: "UserId",
                principalTable: "UserTable",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
