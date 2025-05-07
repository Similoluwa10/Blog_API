using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog_API.Migrations
{
    /// <inheritdoc />
    public partial class CreatedUserBlogpostRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "BlogPostTable",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogPostTable_UserTable_UserId",
                table: "BlogPostTable");

            migrationBuilder.DropIndex(
                name: "IX_BlogPostTable_UserId",
                table: "BlogPostTable");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "BlogPostTable");
        }
    }
}
