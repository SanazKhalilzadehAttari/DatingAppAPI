using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatingAppAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class addForeignKeyToPhotos1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppUSerId",
                table: "Photos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Photos_AppUSerId",
                table: "Photos",
                column: "AppUSerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Users_AppUSerId",
                table: "Photos",
                column: "AppUSerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Users_AppUSerId",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Photos_AppUSerId",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "AppUSerId",
                table: "Photos");
        }
    }
}
