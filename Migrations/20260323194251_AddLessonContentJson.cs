using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeLang.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonContentJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentJson",
                table: "Lessons",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentJson",
                table: "Lessons");
        }
    }
}
