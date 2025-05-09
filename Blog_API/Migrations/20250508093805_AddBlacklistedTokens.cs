using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blog_API.Migrations
{
    /// <inheritdoc />
    public partial class AddBlacklistedTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "BlackListToken",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Jti = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlackListToken", x => x.Id);
                });

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

            migrationBuilder.DropTable(
                name: "BlackListToken");

            migrationBuilder.DropIndex(
                name: "IX_BlogPostTable_UserId",
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
    }
}
