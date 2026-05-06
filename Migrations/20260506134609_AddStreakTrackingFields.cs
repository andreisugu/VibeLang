using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeLang.Migrations
{
    /// <inheritdoc />
    public partial class AddStreakTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxStreakEver",
                table: "UserCourseStats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "StreakBrokenToday",
                table: "UserCourseStats",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxStreakEver",
                table: "UserCourseStats");

            migrationBuilder.DropColumn(
                name: "StreakBrokenToday",
                table: "UserCourseStats");
        }
    }
}
