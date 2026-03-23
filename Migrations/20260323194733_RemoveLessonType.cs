using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeLang.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLessonType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LessonType",
                table: "Lessons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LessonType",
                table: "Lessons",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
