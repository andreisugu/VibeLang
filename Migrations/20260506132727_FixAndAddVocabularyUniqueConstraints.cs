using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeLang.Migrations
{
    /// <inheritdoc />
    public partial class FixAndAddVocabularyUniqueConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old indexes first
            migrationBuilder.DropIndex(
                name: "IX_VocabularyWords_LessonId",
                table: "VocabularyWords");

            migrationBuilder.DropIndex(
                name: "IX_UserVocabularies_UserId",
                table: "UserVocabularies");

            // Remove duplicate (UserId, WordId) entries, keeping the one with latest LastReviewed
            migrationBuilder.Sql(@"
                DELETE FROM ""UserVocabularies"" uv1
                WHERE ""Id"" NOT IN (
                    SELECT MAX(""Id"")
                    FROM ""UserVocabularies""
                    GROUP BY ""UserId"", ""WordId""
                );
            ");

            // Remove duplicate (LessonId, Word) entries in VocabularyWords (keep first one)
            migrationBuilder.Sql(@"
                DELETE FROM ""VocabularyWords"" vw1
                WHERE ""Id"" NOT IN (
                    SELECT MIN(""Id"")
                    FROM ""VocabularyWords""
                    GROUP BY ""LessonId"", ""Word""
                );
            ");

            // Create new unique indexes (after duplicates removed)
            migrationBuilder.CreateIndex(
                name: "IX_VocabularyWords_LessonId_Word",
                table: "VocabularyWords",
                columns: new[] { "LessonId", "Word" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserVocabularies_UserId_WordId",
                table: "UserVocabularies",
                columns: new[] { "UserId", "WordId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VocabularyWords_LessonId_Word",
                table: "VocabularyWords");

            migrationBuilder.DropIndex(
                name: "IX_UserVocabularies_UserId_WordId",
                table: "UserVocabularies");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyWords_LessonId",
                table: "VocabularyWords",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVocabularies_UserId",
                table: "UserVocabularies",
                column: "UserId");
        }
    }
}
