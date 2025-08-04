using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogProject.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogEntryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Blogs_BlogId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Blogs_BlogId1",
                table: "Posts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Posts",
                table: "Posts");

            migrationBuilder.RenameTable(
                name: "Posts",
                newName: "BlogEntry");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_BlogId1",
                table: "BlogEntry",
                newName: "IX_BlogEntry_BlogId1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogEntry",
                table: "BlogEntry",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogEntry_Blogs_BlogId",
                table: "BlogEntry",
                column: "BlogId",
                principalTable: "Blogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogEntry_Blogs_BlogId1",
                table: "BlogEntry",
                column: "BlogId1",
                principalTable: "Blogs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogEntry_Blogs_BlogId",
                table: "BlogEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_BlogEntry_Blogs_BlogId1",
                table: "BlogEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogEntry",
                table: "BlogEntry");

            migrationBuilder.RenameTable(
                name: "BlogEntry",
                newName: "Posts");

            migrationBuilder.RenameIndex(
                name: "IX_BlogEntry_BlogId1",
                table: "Posts",
                newName: "IX_Posts_BlogId1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Posts",
                table: "Posts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Blogs_BlogId",
                table: "Posts",
                column: "BlogId",
                principalTable: "Blogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Blogs_BlogId1",
                table: "Posts",
                column: "BlogId1",
                principalTable: "Blogs",
                principalColumn: "Id");
        }
    }
}
