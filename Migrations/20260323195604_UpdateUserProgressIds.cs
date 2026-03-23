using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeLang.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserProgressIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserVocabularies",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserLessonProgresses",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserCourseStats",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_UserVocabularies_UserId",
                table: "UserVocabularies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLessonProgresses_UserId",
                table: "UserLessonProgresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCourseStats_UserId",
                table: "UserCourseStats",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCourseStats_AspNetUsers_UserId",
                table: "UserCourseStats",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserLessonProgresses_AspNetUsers_UserId",
                table: "UserLessonProgresses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserVocabularies_AspNetUsers_UserId",
                table: "UserVocabularies",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCourseStats_AspNetUsers_UserId",
                table: "UserCourseStats");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLessonProgresses_AspNetUsers_UserId",
                table: "UserLessonProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_UserVocabularies_AspNetUsers_UserId",
                table: "UserVocabularies");

            migrationBuilder.DropIndex(
                name: "IX_UserVocabularies_UserId",
                table: "UserVocabularies");

            migrationBuilder.DropIndex(
                name: "IX_UserLessonProgresses_UserId",
                table: "UserLessonProgresses");

            migrationBuilder.DropIndex(
                name: "IX_UserCourseStats_UserId",
                table: "UserCourseStats");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserVocabularies",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserLessonProgresses",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserCourseStats",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
