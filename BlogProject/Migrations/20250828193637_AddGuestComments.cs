using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogProject.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Comments",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "GuestEmail",
                table: "Comments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestName",
                table: "Comments",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuestEmail",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "GuestName",
                table: "Comments");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Comments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
